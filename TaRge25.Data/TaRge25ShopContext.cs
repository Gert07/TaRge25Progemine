using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TaRge25Shop.Data
{
    public class TaRge25ShopContext : DbContext
    {
        //see class tuleb teha DbContextist, et saaksime kasutada
        //Entity Frameworki andmebaasi operatsioone
        public TaRge25ShopContext(DbContextOptions<TaRge25ShopContext> options) 
            : base(options) { }
    }
}
