using Microsoft.EntityFrameworkCore;
using P03_SalesDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P03_SalesDatabase.Data
{
    class SalesContext : DbContext
    {

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Store> Stores { get; set; }


        public SalesContext()
        {

        }

        public SalesContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=192.168.0.105,1433;Database=SalesDatabase;User ID=nlyutakov;Password=A123456a;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sale>(sale =>
            {
                sale.HasKey(s => s.SaleId);

                sale.Property(s => s.Date).HasDefaultValueSql("getdate()");

                sale.HasOne(p => p.Product)
                    .WithMany(s => s.Sales)
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                sale.HasOne(p => p.Customer)
                    .WithMany(s => s.Sales)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                sale.HasOne(p => p.Store)
                    .WithMany(s => s.Sales)
                    .HasForeignKey(s => s.StoreId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Store>(store =>
            {
                store.HasKey(s => s.StoreId);

                store.Property(s => s.Name).HasMaxLength(80);
            });

            modelBuilder.Entity<Customer>(customer =>
            {
                customer.HasKey(c => c.CustomerId);

                customer.Property(c => c.Name).HasMaxLength(100);

                customer.Property(c => c.Email).HasMaxLength(80).IsUnicode(false);

            });

            modelBuilder.Entity<Product>(product =>
            {
                product.HasKey(p => p.ProductId);

                product.Property(p => p.Name).HasMaxLength(50);

                product.Property(p => p.Description)
                    .HasMaxLength(250)
                    .HasDefaultValue("No description");

            });
        }
    }
}
