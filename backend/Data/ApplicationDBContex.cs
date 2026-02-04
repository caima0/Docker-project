using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContex : IdentityDbContext<User>
    {
        public ApplicationDBContex(DbContextOptions<ApplicationDBContex> dbContextOptions)
        :base(dbContextOptions)
        {
            
        }
        public DbSet<ResponseItem> ResponseItem {get; set;}
        public DbSet<Rate> Rates{get; set;}
        public DbSet<Balance> Balances { get; set; }
        public DbSet<BalanceHistory> BalanceHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            List<IdentityRole> roles= new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = "a1b2c3d4-e5f6-7890-1234-567890abcdef",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                   Id = "b2c3d4e5-f6a7-8901-2345-67890abcdef0",
                   Name = "User",
                   NormalizedName = "USER"
                },
            };
            modelBuilder.Entity<IdentityRole>().HasData(roles);

            modelBuilder.Entity<Rate>()
                .HasOne(r => r.ResponseItem)
                .WithMany(ri => ri.Rates)
                .HasForeignKey(r => r.ResponseItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Balance>()
                .HasOne(b => b.User)
                .WithMany(u => u.Balances)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Balance>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BalanceHistory>()
                .HasOne(bh => bh.User)
                .WithMany(u => u.BalanceHistory)
                .HasForeignKey(bh => bh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BalanceHistory>()
                .Property(bh => bh.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BalanceHistory>()
                .Property(bh => bh.BalanceAfterTransaction)
                .HasPrecision(18, 2);
        }
    }
}