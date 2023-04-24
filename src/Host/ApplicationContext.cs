using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ProjectS
{
    public class ApplicationContext : DbContext
    {
        internal object JsonData;

        public DbSet<Client> Clients { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().HasData(
                new Client { Name = "Tom",Surname = "Lebovski",BirthdayYear = "1999", Email = "LebovskiTom@gmail.com", Number = "339499908",Adress = "Wall Street" },
                new Client { Name = "Nik", Surname = "Johnson", BirthdayYear = "1995", Email = "NikJohnson@gmail.com", Number = "339499908", Adress = "Wall Street" },
                new Client { Name = "Mike", Surname = "Vazovski", BirthdayYear = "1973", Email = "MikeVazovski@gmail.com", Number = "339499908", Adress = "Wall Street"}
                );
        }
    }
}
