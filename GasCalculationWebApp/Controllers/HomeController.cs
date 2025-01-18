using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace GasCalculationWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
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

            // Fiyat verisini doğru şekilde parse etmek için TryParse kullanıyoruz
            double price;
            if (!double.TryParse(priceText.Replace(".", ","), out price))
            {
                ViewBag.Result = $"Unable to parse price for {fuelType}.";
                return View("Index");
            }

            // Yakıt tüketimi hesaplama
            double totalGasSpend = Math.Round((averageGasSpend * distance) / 100, 2);

            // Toplam fiyat hesaplama
            double totalPrice = Math.Round(totalGasSpend * price, 2);

            // Gidiş-dönüş maliyeti
            double roundTripPrice = Math.Round(totalPrice * 2, 2);

            // Sonuçları ViewBag'e ekleyelim
            ViewBag.Result = $"Fuel Type: {fuelTypeLabel}<br>" +
                             $"Price per liter: {price} TL<br>" +
                             $"Total fuel consumption = {totalGasSpend} liters<br>" +
                             $"Total price = {totalPrice} TL<br>" +
                             $"Round trip price = {roundTripPrice} TL";

            return View("Index");
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
                    string priceText = fuelTypeNode.InnerText.Trim();
                    int startIndex = priceText.IndexOf(fuelTypeLabel) + fuelTypeLabel.Length;
                    int endIndex = priceText.IndexOf("TL", startIndex);
                    return priceText.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
