using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestMaker.Database.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Users> Users { get; set; }

        public DatabaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity => { entity.Property(e => e.Username).IsRequired(); });
            modelBuilder.Entity<Users>(entity => { entity.Property(e => e.Password).IsRequired(); });

            #region Seed

            modelBuilder.Entity<Users>().HasData(new Users { Id = 1, Username = "testu", Password = "testp" });

            #endregion Seed
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=desktop-0lidgcq\mssql;Database=TestMaker;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}