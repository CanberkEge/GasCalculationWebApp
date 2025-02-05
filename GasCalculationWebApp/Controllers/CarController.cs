using GasCalculationWebApp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GasCalculationWebApp.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using HtmlAgilityPack;

namespace GasCalculationWebApp.Controllers
{
    public class CarController : Controller
    {


        private readonly ApplicationDbContext _context;

        public CarController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult SaveCar()
        {
            ViewBag.Brands = _context.CarDatas2.Select(c => c.Brand).Distinct().ToList();
            return View();
        }



        [Authorize]
        [HttpPost]
        public IActionResult SaveCar(int carId)

        {
            Console.WriteLine($"SaveCar metodu çağrıldı! Gelen carId: {carId}");
            if (carId <= 0)
            {
                Console.WriteLine("HATA: carId 0 veya geçersiz!"); //yeni eklendi 5.02.2025
                TempData["Error"] = "Invalid car selection!";
                return RedirectToAction("MyCars");
            }

            // Kullanıcı ID al
            int userId = GetLoggedInUserId();
            Console.WriteLine($"Oturum Açan Kullanıcı ID: {userId}");
            if (userId == 0) throw new Exception("Error: User ID is invalid!");

            // Seçilen araba var mı kontrol et
            var existingCar = _context.CarDatas2.Find(carId);
            Console.WriteLine($"Seçilen Araba ID: {carId}, Var mı?: {existingCar != null}");
            if (existingCar == null)
            {
                TempData["Error"] = "Car not found!";
                return RedirectToAction("MyCars");
            }

            // Kullanıcının daha önce ekleyip eklemediğini kontrol et
            bool isAlreadySaved = _context.UserCars.Any(uc => uc.UserId == userId && uc.CarId == carId);
            Console.WriteLine($"isAlreadySaved: {isAlreadySaved}");
            if (isAlreadySaved)
            {
                TempData["Error"] = "You already saved this car.";
                return RedirectToAction("MyCars");
            }

            // Kullanıcı ve araba ilişkisini kaydet
            var userCar = new UserCar
            {
                UserId = userId,
                CarId = carId,
                IsOwner = true
            };

            Console.WriteLine($"UserCar Eklenecek: UserId={userCar.UserId}, CarId={userCar.CarId}");

            try
            {
                _context.UserCars.Add(userCar);
                int affectedRows = _context.SaveChanges();
                Console.WriteLine($"UserCar başarıyla kaydedildi. Affected Rows: {affectedRows}");

                if (affectedRows > 0)
                {
                    TempData["Success"] = "Araç başarıyla kaydedildi!";
                }
                else
                {
                    TempData["Error"] = "Araç kaydedilirken bir hata oluştu!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA: UserCar kaydedilemedi! {ex.ToString()}");
                TempData["Error"] = "Beklenmeyen bir hata oluştu, lütfen tekrar deneyin.";
            }

            return RedirectToAction("MyCars");
        }










        public IActionResult MyCars()
        {
            int userId = GetLoggedInUserId();

            var cars = _context.UserCars
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Car) // Car verilerini doğrudan yükler, 29.01.2025
                .Select(uc => uc.Car)
                .ToList();


            if (!cars.Any())
            {
                ViewBag.Message = "You have not saved any cars.";
            }

            return View(cars);
        }

        /*
        private int GetLoggedInUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Kullanıcının ID'si
        }
        */

        // 29.01.2025
        private int GetLoggedInUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? int.Parse(userId) : 0;
        }


        #region Dropdown Data Endpoints


        [HttpGet]
        public IActionResult GetModels(string brand)
        {
            var models = _context.CarDatas2
                .Where(c => c.Brand == brand)
                .Select(c => c.Model)
                .Distinct()
                .ToList();

            if (!models.Any())
            {
                return Json(new[] { "No models available" });
            }

            return Json(models);
        }

        [HttpGet]
        public IActionResult GetGenerations(string brand, string model)
        {
            var generations = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model)
                .Select(c => c.Generation)
                .Distinct()
                .ToList();

            if (!generations.Any())
            {
                return Json(new[] { "No generations available" });
            }

            return Json(generations);
        }

        [HttpGet]
        public IActionResult GetYears(string brand, string model, string generation)
        {
            var years = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation)
                .Select(c => c.Year.ToString())
                .Distinct()
                .ToList();

            if (!years.Any())
            {
                return Json(new[] { "No years available" });
            }

            return Json(years);
        }

        [HttpGet]
        public IActionResult GetEngines(string brand, string model, string generation, string year)
        {
            var engines = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year.ToString() == year)
                .Select(c => c.Engine)
                .Distinct()
                .ToList();

            if (!engines.Any())
            {
                return Json(new[] { "No engines available" });
            }

            return Json(engines);
        }

        [HttpGet]
        public IActionResult GetFuelTypes(string brand, string model, string generation, string year, string engine)
        {
            var fuels = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year.ToString() == year && c.Engine == engine)
                .Select(c => c.Fuel)
                .Distinct()
                .ToList();

            if (!fuels.Any())
            {
                return Json(new[] { "No fuel types available" });
            }

            return Json(fuels);
        }

        [HttpGet]
        public IActionResult GetHPs(string brand, string model, string generation, string year, string engine, string fuel)
        {
            var hpValues = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year.ToString() == year && c.Engine == engine && c.Fuel == fuel)
                .Select(c => c.HP.ToString())
                .Distinct()
                .ToList();

            if (!hpValues.Any())
            {
                return Json(new[] { "No HP available" });
            }

            return Json(hpValues);
        }











        #endregion



        #region Fetch Car Details Endpoint

        [HttpGet]
        public IActionResult GetCarDetails(string brand, string model, string generation, string year, string engine, string fuel, int hp)
        {
            var car = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year == year && c.Engine == engine && c.Fuel == fuel && c.HP == hp)
                .Select(c => new
                {
                    c.Id,
                    c.Brand,
                    c.Model,
                    c.Generation,
                    c.Year,
                    c.Engine,
                    c.Fuel,
                    c.HP,
                    c.CityConsumption,
                    c.OutsideConsumption
                })
                .FirstOrDefault();

            if (car == null)
            {
                return Json(new { message = "Car details not found" });
            }

            return Json(car); 
        }

        #endregion
    




    }
}
