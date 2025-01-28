using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using GasCalculationWebApp.Data;
using System.Security.Claims;
using GasCalculationWebApp.Model;

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
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
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
            _context.Users.Add(user);
            _context.SaveChanges();

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
    }
}

