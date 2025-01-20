using System.ComponentModel.DataAnnotations;

namespace GasCalculationWebApp.Models
{
    public class CarDatas
    {
        public int Id { get; set; } // Eğer tablo birincil anahtar içeriyorsa
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Generation { get; set; }
        public string Year { get; set; }
        public string Engine { get; set; }
        public string Fuel { get; set; }
        public int HP { get; set; }

        //yeni değişti
        public float CityConsumption { get; set; }
        public float OutsideConsumption { get; set; }
    }
}

