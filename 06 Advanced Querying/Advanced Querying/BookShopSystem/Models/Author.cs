using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Models
{
    public class Author
    {

        /*
           o	AuthorId
           o	FirstName (up to 50 characters, unicode, not required)
           o	LastName (up to 50 characters, unicode)
         */

        public int AuthorId { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        public virtual ICollection<Book> Books { get; set; }
    }
}
