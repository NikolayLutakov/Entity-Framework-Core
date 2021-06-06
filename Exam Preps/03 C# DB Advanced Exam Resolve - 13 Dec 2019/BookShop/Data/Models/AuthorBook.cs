using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Data.Models
{
    public class AuthorBook
    {
        public int AuthorId { get; set; }

        public virtual Author Author { get; set; }

        public int BookId { get; set; }
        
        public virtual Book Book { get; set; }
    }
}
//•	AuthorId - integer, Primary Key, Foreign key (required)
//•	Author - Author
//•	BookId - integer, Primary Key, Foreign key (required)
//•	Book - Book
