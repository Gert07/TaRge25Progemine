using TaRge25Shop.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace TaRge25Shop.Data
{
    public class TaRge25ShopContext : DbContext
    {
        //see class tuleb teha DbContextist, et saaksime kasutada
        //Entity Frameworki andmebaasi operatsioone
        public TaRge25ShopContext(DbContextOptions<TaRge25ShopContext> options) 
            : base(options) { }

        //vaja lisada dbSet, mis on seotud meie domain klassiga Spaceship
        public DbSet<Spaceship> Spaceships { get; set; }

        public DbSet<Kindergarden> Kindergardens { get; set; }
    }
}
