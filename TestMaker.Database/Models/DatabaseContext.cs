using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TestMaker.Database.Entities;

namespace TestMaker.Database.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Test> Test { get; set; }
        public DbSet<TestQuestion> TestQuestion { get; set; }
        public DbSet<TestAnswer> TestAnswer { get; set; }
        public DbSet<TestAccess> TestAccess { get; set; }

        public DbSet<TestResult> TestResult { get; set; }
        public DbSet<TestResultQuestion> TestResultQuestion { get; set; }
        public DbSet<TestResultAnswer> TestResultAnswer { get; set; }

        public DatabaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(new User { Id = 1, Username = "123", Password = "321" });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=desktop-0lidgcq\mssql;Database=TestMaker;Trusted_Connection=True;MultipleActiveResultSets=true");
        }
    }
}