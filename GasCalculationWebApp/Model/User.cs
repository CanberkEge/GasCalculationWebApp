

namespace GasCalculationWebApp.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<UserCar> UserCars { get; set; } // Ara tablodan ilişki
    }
}
