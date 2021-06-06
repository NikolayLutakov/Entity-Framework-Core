namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var result = new StringBuilder();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportBookDto>), new XmlRootAttribute("Books"));

            var booksToAdd = new List<Book>();

            using (var reader = new StringReader(xmlString))
            {
                var booksDto = xmlSerializer.Deserialize(reader) as List<ImportBookDto>;

                foreach (var book in booksDto)
                {
                    if (!IsValid(book))
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime publishedOn;
                    var dateParsed = DateTime.TryParseExact(book.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out publishedOn);
                   
                    if (!dateParsed)
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (book.Genre > 3 || book.Genre < 1)
                    {

                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    Genre genre = Enum.Parse<Genre>(book.Genre.ToString());

                    var curBook = new Book()
                    {
                        Name = book.Name,
                        Genre = genre,
                        Price = book.Price,
                        Pages = book.Pages,
                        PublishedOn = publishedOn
                    };

                    result.AppendLine(string.Format(SuccessfullyImportedBook, curBook.Name, curBook.Price));
                    booksToAdd.Add(curBook);
                }

            }

            context.Books.AddRange(booksToAdd);
            context.SaveChanges();

            return result.ToString().Trim();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var result = new StringBuilder();

            var authorsDto = JsonConvert.DeserializeObject<List<ImportAuthorDto>>(jsonString).ToList();
            

            var authorsToAdd = new List<Author>();

            foreach (var author in authorsDto)
            {
                if (!IsValid(author))
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }

                var emails = authorsToAdd.Select(x => x.Email.Trim());
                if (emails.Contains(author.Email.Trim()))
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }

                //if (authorsToAdd.Any(a => a.Email == author.Email))
                //{
                //    result.AppendLine(ErrorMessage);
                //    continue;
                //}

                var curAuthor = new Author() 
                {
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    Phone = author.Phone,
                    Email = author.Email
                };

               

                var booksToAdd = new List<AuthorBook>();

                foreach (var bookId in author.Books)
                {
                    //if (bookId.Id == null)
                    //{
                    //    continue;
                    //}

                    var book = context.Books.Find(bookId.Id);
                    if (book == null)
                    {
                        continue;
                    }

                    var authorBook = new AuthorBook()
                    {
                        Author = curAuthor,
                        Book = book
                    };

                    booksToAdd.Add(authorBook);

                }

                if (booksToAdd.Count == 0)
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }

                curAuthor.AuthorsBooks = booksToAdd;

                authorsToAdd.Add(curAuthor);
                result.AppendLine(string.Format(SuccessfullyImportedAuthor, author.FirstName + " " + author.LastName, booksToAdd.Count));
            }

            context.Authors.AddRange(authorsToAdd);
            context.SaveChanges();
            

            return result.ToString().Trim();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}