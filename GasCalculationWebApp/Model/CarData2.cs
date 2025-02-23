namespace GasCalculationWebApp.Model
{
    public class CarData2
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Generation { get; set; }
        public string Year { get; set; }
        public string Engine { get; set; }
        public string Fuel { get; set; }
        public int HP { get; set; }
        public decimal CityConsumption { get; set; }
        public decimal OutsideConsumption { get; set; }
        public decimal Acceleration_0_100 { get; set; } // Yeni eklenen alan


        public ICollection<UserCar> UserCars { get; set; } // Ara tablodan ilişki

        /*
        public int UserId { get; set; }
        public User User { get; set; }
        */

    }
}
