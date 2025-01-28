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
        public IActionResult SaveCar(CarData2 carData)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View(carData);
            }

            // Kullanıcı oturumdan ID alınıyor
            int userId = GetLoggedInUserId();

            // Kullanıcının seçtiği araç veritabanında var mı kontrol et
            var existingCar = _context.CarDatas2.FirstOrDefault(c =>
                c.Brand == carData.Brand &&
                c.Model == carData.Model &&
                c.Generation == carData.Generation &&
                c.Year == carData.Year &&
                c.Engine == carData.Engine &&
                c.Fuel == carData.Fuel &&
                c.HP == carData.HP);

            if (existingCar == null)
            {
                _context.CarDatas2.Add(carData);
                _context.SaveChanges();
                existingCar = carData;



                _context.CarDatas2.Add(existingCar);
                _context.SaveChanges(); // Yeni aracı kaydet
            }

            // Kullanıcının bu aracı daha önce kaydedip kaydetmediğini kontrol et
            // Kullanıcının daha önce kaydedip kaydetmediğini kontrol et
            if (_context.UserCars.Any(uc => uc.UserId == userId && uc.CarId == existingCar.Id))
            {
                ViewBag.Error = "You already saved this car.";
                return RedirectToAction("MyCars");
            }

            // Kullanıcı ve araba ilişkisi kaydı
            _context.UserCars.Add(new UserCar
            {
                UserId = userId,
                CarId = existingCar.Id,
                IsOwner = true
            });

            

            
            _context.SaveChanges();

            return RedirectToAction("MyCars");
        }





        public IActionResult MyCars()
        {
            int userId = GetLoggedInUserId();

            var cars = _context.UserCars
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.Car)
                .ToList();

            return View(cars);
        }

        private int GetLoggedInUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Kullanıcının ID'si
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
