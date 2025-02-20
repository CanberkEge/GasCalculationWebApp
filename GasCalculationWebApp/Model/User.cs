

namespace GasCalculationWebApp.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Username { get; set; }  // Kullanıcı adı

        public bool EmailConfirmed { get; set; } 
        public string? ConfirmationToken { get; set; }


        public ICollection<UserCar> UserCars { get; set; } // Ara tablodan ilişki
    }
}
