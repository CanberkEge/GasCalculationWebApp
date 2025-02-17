
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using GasCalculationWebApp.Data;
using System.Security.Claims;
using GasCalculationWebApp.Model;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace GasCalculationWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Giriş Sayfası
        public IActionResult Login()
        {
            return View();
        }

        // Giriş İşlemi


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }


            // Hata ayıklamak için kullanıcı bilgilerini konsola yazdıralım
            Console.WriteLine($"User Found: ID={user.Id}, Email={user.Email}, PasswordHash={user.PasswordHash}, EmailConfirmed={user.EmailConfirmed}");

            // Eğer user.PasswordHash NULL ise, hata vermesin diye kontrol ekle
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ViewBag.Error = "Your account is not properly set up. Please contact support.";
                return View();
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            if (!user.EmailConfirmed)
            {
                ViewBag.Error = "Please confirm your email before logging in.";
                return View();
            }

            // Kullanıcı oturum bilgilerini oluştur
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID'si
        new Claim(ClaimTypes.Email, user.Email),                // Kullanıcı email'i
        new Claim(ClaimTypes.Name, user.Name ?? "User")         // Kullanıcı adı (varsayılan User)
    };

            // ClaimsIdentity ile kimlik bilgilerini bağla
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Oturum özelliklerini ayarla
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Kalıcı oturum (tarayıcı kapansa bile devam eder)
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) // Oturumun süresi
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            TempData["Message"] = $"Welcome, {user.Email}!";

            return RedirectToAction("Index", "Home");
        }



        // Şifre Değiştirme Sayfasını Getiren Metod
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // Şifre Değiştirme İşlemini Yapan Metod
        [HttpPost]
        [Authorize]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (model.NewPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters long.";
                return RedirectToAction("ChangePassword");
            }

            if (model.NewPassword.Length > 25)
            {
                TempData["Error"] = "New password must be at most 25 characters long.";
                return RedirectToAction("ChangePassword");
            }

            if (!ModelState.IsValid)
            {
                // Hangi alanın geçersiz olduğunu görmek için hata mesajlarını yazdıralım
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                Console.WriteLine("Model validation errors: " + string.Join(", ", errors));

                TempData["Error"] = "Please fill in all fields correctly.";
                return RedirectToAction("ChangePassword"); // Profile sayfasına yönlendir
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) 
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("ChangePassword");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);

            if (user == null)
            {
                TempData["Error"] = "User not found in database.";
                return RedirectToAction("ChangePassword");
            }

            if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
            {
                TempData["Error"] = "Incorrect old password.";
                return RedirectToAction("ChangePassword");
            }


            // 🔴 Şifre eski şifreyle aynı mı kontrol et
            if (BCrypt.Net.BCrypt.Verify(model.NewPassword, user.PasswordHash))
            {
                TempData["Error"] = "New password cannot be the same as the old password.";
                return RedirectToAction("ChangePassword");
            }


            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            TempData["Success"] = "Your password has been changed successfully!";
            return RedirectToAction("ChangePassword");
        }













        // Kayıt Sayfası
        public IActionResult Register()
        {
            return View();
        }

        // Kayıt İşlemi
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ViewBag.Error = "This email is already registered.";
                return View();
            }

            // Şifreyi hash'le
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.EmailConfirmed = false; // E-posta doğrulanmamış olarak ayarla
            user.ConfirmationToken = Guid.NewGuid().ToString(); // Benzersiz bir token oluştur

            _context.Users.Add(user);
            _context.SaveChanges();

            // Doğrulama e-postası gönder
            SendConfirmationEmail(user);

            TempData["Message"] = "Registration successful! Please check your email to confirm your account.";
            return RedirectToAction("Login");
        }

        // Çıkış İşlemi
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Yetkisiz Erişim Sayfası
        public IActionResult AccessDenied()
        {
            return View();
        }






        // Kullanıcı Profili

        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users
                .Where(u => u.Id.ToString() == userId)
                .Select(u => new UserViewModel
                {
                    Name = u.Name,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Login"); // Kullanıcı bulunamazsa login sayfasına yönlendir
            }

            return View(user);
        }





        // 29.01.2025 17:29




































        // E-posta Doğrulama İşlemi
        public IActionResult ConfirmEmail(string token)
        {
            var user = _context.Users.FirstOrDefault(u => u.ConfirmationToken == token);
            if (user == null)
            {
                ViewBag.Error = "Invalid or expired confirmation token.";
                return View("Error");
            }

            user.EmailConfirmed = true;
            user.ConfirmationToken = null; // Token'ı temizle
            _context.SaveChanges();

            TempData["Message"] = "Your email has been confirmed. You can now log in.";
            return RedirectToAction("Login");
        }





























        // E-posta gönderme metodu
        private void SendConfirmationEmail(User user)
        {
            var confirmationUrl = Url.Action("ConfirmEmail", "Account", new { token = user.ConfirmationToken }, Request.Scheme);

            var emailBody = $@"
                <p>Hi {user.Name},</p>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationUrl}'>Confirm Email</a></p>
                <p>If you did not request this, please ignore this email.</p>";

            // SMTP Ayarları
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("berk.erden1960@gmail.com", "dxeodpoqwrqcigst"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("berk.erden1960@gmail.com"),
                Subject = "Confirm Your Email",
                Body = emailBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(user.Email);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send confirmation email: {ex.Message}");
            }
        }
    }
}

