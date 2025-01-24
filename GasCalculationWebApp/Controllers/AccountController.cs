
/*
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GasCalculationWebApp.Model;
using System;
using GasCalculationWebApp.Data;
using BCrypt.Net;

namespace GasCalculationWebApp.Controllers
{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
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

            // Şifreyi hash'le (Önemli!)
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // Giriş Sayfası
        public IActionResult Login()
        {
            return View();
        }

        // Giriş İşlemi
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ViewBag.Error = "Invalid login credentials.";
                return View();
            }

            // Kullanıcıyı oturumda tut (Session)
            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Home");
        }

        // Çıkış İşlemi
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

*/
