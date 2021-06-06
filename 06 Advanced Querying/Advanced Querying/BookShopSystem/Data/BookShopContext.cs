using BookShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Data
{
    public class BookShopContext :DbContext
    {
        public BookShopContext()
        {

        }

        public BookShopContext(DbContextOptions options) : base (options)
        {

        }

        public DbSet<Book> Books { get; set; }
        public DbSet <Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }

        public DbSet<BookCategory> BookCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) 
            {
                optionsBuilder.UseSqlServer("Server=192.168.0.105,1433;Database=BookShop;User ID=nlyutakov;Password=A123456a;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookCategory>(bc =>
            {
                bc.HasKey(e => new { e.BookId, e.CategoryId});

                bc.HasOne(e => e.Book)
                .WithMany(e => e.BookCategories)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Restrict);

                bc.HasOne(e => e.Category)
                .WithMany(e => e.CategoryBooks)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Book>(book =>
            {
                book.HasKey(b => b.BookId);

                book.HasOne(b => b.Author)
                    .WithMany(a => a.Books)
                    .HasForeignKey(b => b.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
           
            });
        }
    }
}
