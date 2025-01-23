using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GasCalculationWebApp.Data;



namespace GasCalculationWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Marka listesini dropdown için gönder
           // List<string> brands = _context.CarDatas
           var brands = _context.CarDatas2
                .Select(c => c.Brand)
                .Distinct()
                .ToList();

            // ViewBag.Brands = brands;
            ViewBag.Brands = brands.Any() ? brands : new List<string> { "No brands available" };   

            return View();
        }

        [HttpPost]
        public IActionResult Calculate(string fuelType, double distance, double averageGasSpend)
        {
            var fuelTypeLabels = new Dictionary<string, string>
            {
                { "benzin", "V/Max Kurşunsuz 95" },
                { "dizel", "V/Max Diesel" },
                { "lpg", "PO/gaz Otogaz" }
            };

            if (!fuelTypeLabels.ContainsKey(fuelType))
            {
                ViewBag.Result = "Invalid fuel type selected.";
                return View("Index");
            }

            string fuelTypeLabel = fuelTypeLabels[fuelType];
            string priceText = GetFuelPrice(fuelTypeLabel);

            if (string.IsNullOrEmpty(priceText))
            {
                ViewBag.Result = $"Price for {fuelType} not found on the website.";
                return View("Index");
            }

            if (!double.TryParse(priceText.Replace(".", ","), out double price))
            {
                ViewBag.Result = $"Unable to parse price for {fuelType}.";
                return View("Index");
            }

            double totalGasSpend = Math.Round((averageGasSpend * distance) / 100, 2);
            double totalPrice = Math.Round(totalGasSpend * price, 2);
            double roundTripPrice = Math.Round(totalPrice * 2, 2);

            // Fuel type baş harfini büyük yapma
            string formattedFuelType = char.ToUpper(fuelType[0]) + fuelType.Substring(1);

            // Sayıları formatlamak (iki ondalık basamak göstermek için "N2" kullanıyoruz)
            string formattedTotalGasSpend = totalGasSpend.ToString("N2");
            string formattedTotalPrice = totalPrice.ToString("N2");
            string formattedRoundTripPrice = roundTripPrice.ToString("N2");

            ViewBag.Result = $"Fuel Type: {formattedFuelType}<br>" +
                             $"Price per liter: {price} TL<br>" +
                             $"Total fuel consumption = {formattedTotalGasSpend} liters<br>" +
                             $"Total price = {formattedTotalPrice} TL<br>" +
                             $"Round trip price = {formattedRoundTripPrice} TL";

            ViewBag.Brands = _context.CarDatas2
                .Select(c => c.Brand)
                .Distinct()
                .ToList();

            return View("Index");
        }

        [HttpPost]
        public IActionResult CalculateByCar(string brand, string model, string generation, string year, string engine, string fuel, double distance)
        {
            //yeni eklendi
            ReloadDropdowns();
            var selectedCar = _context.CarDatas2
                .FirstOrDefault(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year == year && c.Engine == engine && c.Fuel == fuel);

            if (selectedCar == null)
            {

                ViewBag.CarResult = "Selected car information not found. Please check your selections.";
                
                return View("Index");

            }

            string fuelTypeNormalized = selectedCar.Fuel.ToLower().Trim();
            var fuelTypeLabels = new Dictionary<string, string>
    {
        { "benzin", "V/Max Kurşunsuz 95" },
        { "dizel", "V/Max Diesel" },
        { "lpg", "PO/gaz Otogaz" }
    };


            if (!fuelTypeLabels.ContainsKey(fuelTypeNormalized))
            {
                ViewBag.CarResult = $"Fuel type '{selectedCar.Fuel}' not found in fuelTypeLabels.";
                return View("Index");
            }

            string fuelTypeLabel = fuelTypeLabels[fuelTypeNormalized];
            string priceText = GetFuelPrice(fuelTypeLabel);

            if (string.IsNullOrEmpty(priceText))
            {
                ViewBag.CarResult = $"Unable to fetch fuel price for '{fuelTypeLabel}'.";
                return View("Index");
            }

            if (!decimal.TryParse(priceText.Replace(".", ","), out var fuelPrice))
            {
                ViewBag.CarResult = "Invalid fuel price format received. Please try again.";
                return View("Index");
            }

            decimal cityConsumption = selectedCar.CityConsumption;
            decimal outsideConsumption = selectedCar.OutsideConsumption;

            if (cityConsumption <= 0 || outsideConsumption <= 0)
            {
                ViewBag.CarResult = "Fuel consumption data for the selected car is invalid.";
                return View("Index");
            }

            /* decimal fuelConsumption = selectedCar.CityConsumption;

                 if (fuelConsumption <= 0) {

                     ViewBag.CarResult = "Fuel consumption data for the selected car is invalid.";
                     return View("Index");
                 }

             */

            // Şehir içi hesaplaması
            decimal totalCityFuel = Math.Round((decimal)(distance / 100) * cityConsumption, 2);
            decimal cityCost = Math.Round(totalCityFuel * fuelPrice, 2);

            // Şehir dışı hesaplaması
            decimal totalOutsideFuel = Math.Round((decimal)(distance / 100) * outsideConsumption, 2);
            decimal outsideCost = Math.Round(totalOutsideFuel * fuelPrice, 2);

            // Şehir içi ve dışı maliyet farkını hesaplama
            decimal costDifference = Math.Abs(cityCost - outsideCost); // Farkın pozitif görünmesi için Math.Abs kullanıyoruz

            // Şehir içi ve dışı tüketim farkını hesaplama
            decimal fuelDifference = Math.Abs(totalCityFuel - totalOutsideFuel);

            // Sayıyı formatlamak (iki ondalık basamak göstermek için "N2" kullanıyoruz)

            string formattedTotalCityFuel = totalCityFuel.ToString("N2");
            string formattedCityCost = cityCost.ToString("N2");
            string formattedTotalOutsideFuel = totalOutsideFuel.ToString("N2");
            string formattedOutsideCost = outsideCost.ToString("N2");
            string formattedCostDifference = costDifference.ToString("N2");
            string formattedFuelDifference = fuelDifference.ToString("N2");



            /*
                decimal totalFuel = Math.Round((decimal)(distance / 100) * fuelConsumption, 2);
                decimal cost = Math.Round(totalFuel * fuelPrice, 2);
            */

            // Sonuçları göster
            ViewBag.CarResult = $"<b>Car:</b> {brand} {model} ({generation}, {year}, {engine})<br>" +
                                $"<b>Fuel Type:</b> {fuel}<br>" +
                                $"<b>Urban Fuel Consumption:</b> {cityConsumption} L/100km<br>" +
                                $"<b>Distance:</b> {distance} km<br>" +
                                $"<b>Out of Town Fuel Consumption:</b> {outsideConsumption} L/100km<br>" +
                                $"<b>Fuel Price for {fuel}:</b> {fuelPrice} TL/L<br><br>" +
                                $"<u><b>Urban Driving:</b></u><br>" +
                                $"Total Fuel: {formattedTotalCityFuel} liters<br>" +
                                $"Total Cost: {formattedCityCost} TL<br><br>" +
                                $"<u><b>Out of Town Driving:</b></u><br>" +
                                $"Total Fuel: {formattedTotalOutsideFuel} liters<br>" +
                                $"Total Cost: {formattedOutsideCost} TL<br><br>" +
                                $"<u><b>Difference:</b></u><br>" +
                                $"Cost Difference (Şehir İçi - Şehir Dışı): {formattedCostDifference}₺<br>" +
                                $"Fuel Difference (Şehir İçi - Şehir Dışı): {formattedFuelDifference}L";



            return View("Index");
        }

        [HttpGet]
        public IActionResult GetModels(string brand)
        {
            var models = _context.CarDatas2
                .Where(c => c.Brand == brand)
                .Select(c => c.Model)
                .Distinct()
                .ToList();

            if (models == null || !models.Any())
            {
                return Json(new List<string> {   "No models available" } );
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

            if (generations == null || !generations.Any())
            {
                return Json(new List<string> { "No generations available" });
            }

            return Json(generations);
        }



        [HttpGet]
        public IActionResult GetYears(string brand, string model, string generation)
        {
            var years = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation)
                .Select(c => c.Year)
                .Distinct()
                .ToList();
            /*if (years == null || !years.Any())
            {
                return Json(new List<string> { "No years available" });
            }
            return Json(years); 
            */
            return Json(years.Any() ? years : new List<string> { "No years available" });
        }

        [HttpGet]
        public IActionResult GetEngines(string brand, string model, string generation, string year)
        {
            var engines = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year == year)
                .Select(c => c.Engine)
                .Distinct()
                .ToList();

            return Json(engines.Any() ? engines : new List<string> { "No engines available" });
        }

        [HttpGet]
        public IActionResult GetFuelTypes(string brand, string model, string generation, string year, string engine)
        {
            var fuels = _context.CarDatas2
                .Where(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year == year && c.Engine == engine)
                .Select(c => c.Fuel)
                .Distinct()
                .ToList();

            return Json(fuels.Any() ? fuels : new List<string> { "No fuel types available" });
        }

        // Yeni eklenen GetCityConsumption metodunu burada tanımlıyoruz
        [HttpGet]
        public IActionResult GetCityConsumption(string brand, string model, string generation, string year, string engine, string fuel)
        {
            var carData = _context.CarDatas2
                .FirstOrDefault(c => c.Brand == brand && c.Model == model && c.Generation == generation && c.Year == year && c.Engine == engine && c.Fuel == fuel);

            if (carData != null)
            {
                Console.WriteLine($"City Consumption for {brand} {model}: {carData.CityConsumption}");
                return Json(new { cityConsumption = carData.CityConsumption });
            }
            Console.WriteLine("City Consumption not found.");
            return Json(new { cityConsumption = "Not available" });
        }






        private string GetFuelPrice(string fuelTypeLabel)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load("https://www.petrolofisi.com.tr/akaryakit-fiyatlari");

                HtmlNode fuelTypeNode = doc.DocumentNode.SelectSingleNode($"//div[contains(div[@class='mb-1 fs-7 text-primary'], '{fuelTypeLabel}')]");

                if (fuelTypeNode != null)
                {
                    Console.WriteLine($"Fuel Type Node Found: {fuelTypeNode.InnerText}");
                    string priceText = fuelTypeNode.InnerText.Trim();
                    int startIndex = priceText.IndexOf(fuelTypeLabel) + fuelTypeLabel.Length;
                    int endIndex = priceText.IndexOf("TL", startIndex);
                    string result = priceText.Substring(startIndex, endIndex - startIndex).Trim();
                    Console.WriteLine($"Extracted Price Text: {result}");
                    return result;
                }
                else {
                    Console.WriteLine($"Fuel Type Node Not Found for: {fuelTypeLabel}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching fuel price: {ex.Message}");
            }

            return null;
        }

        private void ReloadDropdowns()
        {
            // Marka listesini tekrar yükle
            ViewBag.Brands = _context.CarDatas2
                .Select(c => c.Brand)
                .Distinct()
                .ToList();
        }




    }
}

