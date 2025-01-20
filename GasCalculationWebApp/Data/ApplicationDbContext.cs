using Microsoft.EntityFrameworkCore;
using GasCalculationWebApp.Models;

namespace GasCalculationWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<CarDatas> CarDatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Burada kolon yapılandırmalarınızı yapabilirsiniz
            modelBuilder.Entity<CarDatas>()
                .Property(c => c.CityConsumption)
                .HasConversion<double>(); // CityConsumption verisini double olarak işle

            modelBuilder.Entity<CarDatas>()
                .Property(c => c.OutsideConsumption)
                .HasConversion<double>(); // OutsideConsumption verisini double olarak işle
        }
    }
}
