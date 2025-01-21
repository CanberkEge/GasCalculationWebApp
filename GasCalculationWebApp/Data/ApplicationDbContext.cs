using Microsoft.EntityFrameworkCore;
using GasCalculationWebApp.Model;

namespace GasCalculationWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

       // public DbSet<CarDatas> CarDatas { get; set; }

        public DbSet<CarData2> CarDatas2 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CarDatas yapılandırması
            /*   modelBuilder.Entity<CarDatas>()
                   .Property(c => c.CityConsumption)
                   .HasConversion<double>();

               modelBuilder.Entity<CarDatas>()
                   .Property(c => c.OutsideConsumption)
                   .HasConversion<double>();
            */
            // CarData2 yapılandırması
            modelBuilder.Entity<CarData2>()
                .Property(c => c.CityConsumption)
                .HasColumnType("decimal(5,2)"); // Veritabanı türü olarak decimal(5,2)

            modelBuilder.Entity<CarData2>()
                .Property(c => c.OutsideConsumption)
                .HasColumnType("decimal(5,2)"); // Veritabanı türü olarak decimal(5,2)
        }
    }
}
