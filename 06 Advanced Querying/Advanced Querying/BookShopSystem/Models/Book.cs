using BookShop.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Models
{
    public class Book
    {
        public Book()
        {
            this.BookCategories = new HashSet<BookCategory>();
        }

        /*
         o	BookId
         o	Title (up to 50 characters, unicode)
         o	Description (up to 1000 characters, unicode)
         o	ReleaseDate (not required)
         o	Copies (an integer)
         o	Price
         o	EditionType – enum (Normal, Promo, Gold)
         o	AgeRestriction – enum (Minor, Teen, Adult)
         o	Author
         o	BookCategories
        */

        public int BookId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime? ReleaseDate { get; set; }
        
        [Required]
        public int Copies { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public EditionType EditionType { get; set; }

        [Required]
        public AgeRestriction AgeRestriction { get; set; }

        [Required]
        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }
        public ICollection<BookCategory> BookCategories { get; set; }
    }
}
