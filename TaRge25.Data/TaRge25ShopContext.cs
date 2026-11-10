using TaRge25Shop.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaRge25Shop.Core.Domain;

namespace TaRge25Shop.Data
{
    public class TaRge25ShopContext : IdentityDbContext<ApplicationUser>
    {
        //see class tuleb teha DbContextist, et saaksime kasutada
        //Entity Frameworki andmebaasi operatsioone
        public TaRge25ShopContext(DbContextOptions<TaRge25ShopContext> options) 
            : base(options) { }

        //vaja lisada dbSet, mis on seotud meie domain klassiga Spaceship
        public DbSet<Spaceship> Spaceships { get; set; }
        public DbSet<Kindergarden> Kindergardens { get; set; }
        public DbSet<RealEstate> RealEstates { get; set; }
        public DbSet<FileToApi> FileToApi { get; set; }
        public DbSet<FileToDatabase> FileToDatabases { get; set; }

        //public DbSet<IdentityRole> ApplicationUsers { get; set; }
    }
}
