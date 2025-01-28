namespace GasCalculationWebApp.Model
{
    public class UserCar
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int CarId { get; set; }
        public CarData2 Car { get; set; }

        public bool IsOwner { get; set; }
    }
}
