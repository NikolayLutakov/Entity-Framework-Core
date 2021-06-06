using BookShop.Data;
using BookShop.Initializer;
using BookShop.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BookShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            BookShopContext context = new BookShopContext();

            //DbInitializer.ResetDatabase(context);

            //Console.WriteLine(GetBooksByAgeRestriction(context, InputReader())); //02
            //Console.WriteLine(GetGoldenBooks(context)); //03
            //Console.WriteLine(GetBooksByPrice(context)); //04
            //Console.WriteLine(GetBooksNotReleasedIn(context, int.Parse(InputReader()))); //05
            Console.WriteLine(GetBooksByCategory(context, InputReader())); //06
            //Console.WriteLine(GetBooksReleasedBefore(context, InputReader())); //07
            //Console.WriteLine(GetAuthorNamesEndingIn(context, InputReader())); //08
            //Console.WriteLine(GetBookTitlesContaining(context, InputReader())); //09
            //Console.WriteLine(GetBooksByAuthor(context, InputReader())); //10
            //Console.WriteLine(CountBooks(context, int.Parse(InputReader()))); //11
            //Console.WriteLine(CountCopiesByAuthor(context)); //12
            //Console.WriteLine(GetTotalProfitByCategory(context)); //13
            //Console.WriteLine(GetMostRecentBooks(context)); //14
            //IncreasePrices(context); //15
            //Console.WriteLine(RemoveBooks(context)); //16

            
        }

        //16
        public static int RemoveBooks(BookShopContext context)
        {

            var bookCategoriesToDelete = context.BookCategories.Where(b => b.Book.Copies < 4200).ToList();
            context.RemoveRange(bookCategoriesToDelete);
            context.SaveChanges();

            var booksToDelete = context.Books.Where(b => b.Copies < 4200).ToList();

            context.RemoveRange(booksToDelete);

            int result = context.SaveChanges();
            return result;
        }

        //15
        public static void IncreasePrices(BookShopContext context)
        {
            var booksToUpdate = context.Books.Where(b => b.ReleaseDate.Value.Year < 2010).ToList();

            foreach (var b in booksToUpdate)
            {
                b.Price += 5;
            }

            context.SaveChanges();
        }

        //14
        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder result = new StringBuilder();

            var categories = context.Categories.Select(c => new
            {
                CategoryName = c.Name,
                Books = c.CategoryBooks.OrderByDescending(b => b.Book.ReleaseDate).Take(3)
                .Select(b => new
                {
                    BookTitle = b.Book.Title,
                    BookRealeaseYear = b.Book.ReleaseDate.Value.Year
                })
            }).OrderBy(c => c.CategoryName).ToList();

            foreach (var c in categories)
            {
                result.AppendLine($"--{c.CategoryName}");
                foreach (var book in c.Books)
                {
                    result.AppendLine($"{book.BookTitle} ({book.BookRealeaseYear})");
                }
            }
            return result.ToString().Trim();
        }

        //13
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            StringBuilder result = new StringBuilder();

            var categories = context.Categories.Select(c => new
            {
                Name = c.Name,
                TotalProffits = c.CategoryBooks.Select(b => b.Book.Price * b.Book.Copies).Sum()
            })
            .OrderByDescending(c => c.TotalProffits).ThenBy(c => c.Name)
            .ToList();

            foreach (var c in categories)
            {
                result.AppendLine($"{c.Name} ${c.TotalProffits:F2}");
            }

            return result.ToString().Trim();
        }

        //12

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            StringBuilder result = new StringBuilder();

            var authors = context.Authors.Select(a => new
            {
                FirstName = a.FirstName,
                LastName = a.LastName,
                BookCopies = a.Books.Select(b => b.Copies)
                .Sum()
            })
            .OrderByDescending(a => a.BookCopies)
            .ToList();

            foreach (var a in authors)
            {
                result.AppendLine($"{a.FirstName} {a.LastName} - {a.BookCopies}");
            }

            return result.ToString().Trim();
        }


        //11
        public static int CountBooks(BookShopContext context, int lengthCheck) => context.Books.Count(b => b.Title.Length > lengthCheck);


        //10
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            StringBuilder result = new StringBuilder();

            var books = context.Books.Select(b => new
            {
                BookId = b.BookId,
                BookTitle = b.Title,
                AuthorFirstName = b.Author.FirstName,
                AuthorLastName = b.Author.LastName
            })
                .Where(b => b.AuthorLastName
                .ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var book in books)
            {
                result.AppendLine($"{book.BookTitle} ({book.AuthorFirstName + " " + book.AuthorLastName })");
            }

            return result.ToString().Trim();
        }

        //09

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            StringBuilder result = new StringBuilder();

            var books = context.Books.Where(b => b.Title.ToLower().Contains(input.ToLower())).Select(b => b.Title).OrderBy(b => b).ToList();

            foreach (var book in books)
            {
                result.AppendLine(book);
            }

            return result.ToString().Trim();
        }

        //08
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            StringBuilder result = new StringBuilder();

            var authors = context.Authors
                .ToList()
                .Where(a => a.FirstName
                .Substring(a.FirstName.Length - input.Length) == input)
                .Select(a => a.FirstName + " " + a.LastName
                )
                .OrderBy(a => a)
                .ToList();

            foreach (var author in authors)
            {
                result.AppendLine($"{author}");
            }


            return result.ToString().Trim();
        }

        //07
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            StringBuilder result = new StringBuilder();

            var books = context.Books
                .Where(b => b.ReleaseDate < DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture))
                //.Where(b => b.ReleaseDate < DateTime.Parse(date)) not working in judge
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    Title = b.Title,
                    EditionType = b.EditionType,
                    Price = b.Price,
                })
                .ToList();

            foreach (var book in books)
            {
                result.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:F2}");
            }

            return result.ToString().Trim();
        }

        //06
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            StringBuilder result = new StringBuilder();

            string[] categories = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).ToArray();

            var books = context.BookCategories.Where(b => categories.Contains(b.Category.Name.ToLower())).Select(b => b.Book.Title).OrderBy(b => b).ToList();

            //List<string> allBooks = new List<string>();

            //foreach (var category in categories)
            //{
            //    var books = context.Books.Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == category)).Select(b => b.Title).ToList();

            //    allBooks.AddRange(books);
            //}

            //foreach (var book in allBooks.OrderBy(b => b))
            //{
            //    result.AppendLine(book);
            //}
            var result1 = string.Join(Environment.NewLine, books);

            return result1.ToString().Trim();
        }

        //05
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            StringBuilder result = new StringBuilder();

            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList();

            foreach (var book in books)
            {
                result.AppendLine($"{book}");
            }


            return result.ToString().Trim();
        }

        //04
        public static string GetBooksByPrice(BookShopContext context)
        {
            StringBuilder result = new StringBuilder();

            var books = context.Books
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .ToList();

            foreach (var book in books)
            {
                result.AppendLine($"{book.Title} - ${book.Price:F2}");
            }

            return result.ToString().Trim();
        }

        //03
        public static string GetGoldenBooks(BookShopContext context)
        {
            StringBuilder result = new StringBuilder();

            var books = context.Books.ToList().Where(b => b.EditionType.ToString() == "Gold" && b.Copies < 5000).Select(b => new { Title = b.Title, Id = b.BookId }).OrderBy(b => b.Id).ToList();

            foreach (var book in books)
            {
                result.AppendLine(book.Title);
            }

            return result.ToString().Trim();

        }




        //02
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            StringBuilder result = new StringBuilder();


            //AgeRestriction restriction = (AgeRestriction)Enum.Parse(typeof(AgeRestriction), command.Substring(0, 1).ToUpper() + command.Substring(1, command.Length - 1).ToLower());

            //var books = context.Books
            //    .Where(b => b.AgeRestriction == restriction)
            //    .Select(x => new
            //        {
            //            Title = x.Title,
            //        })
            //    .OrderBy(x => x.Title)
            //    .ToList() ;


            var books = context.Books
                .ToList()
                .Where(b => b.AgeRestriction
                     .ToString()
                     .ToLower() == command.ToLower())
                .Select(b => new
                {
                    Title = b.Title
                })
                .OrderBy(b => b.Title)
                .ToList();


            foreach (var book in books)
            {
                result.AppendLine(book.Title);
            }

            return result.ToString().Trim();
        }

        public static string InputReader()
        {
            return Console.ReadLine();
        }
    }
}
