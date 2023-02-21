using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAPR.Models
{
    public class DataBaseContext:DbContext
    {
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Purchase> Purchases  { get; set; }


        public DataBaseContext(DbContextOptions<DataBaseContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
