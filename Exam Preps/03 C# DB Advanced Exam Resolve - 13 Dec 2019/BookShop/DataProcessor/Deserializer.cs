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
            var xmlSerializer = new XmlSerializer(typeof(ImportBookDto[]), new XmlRootAttribute("Books"));

            var result = new StringBuilder();


            using (var reader = new StringReader(xmlString))
            {
                var xmlBooks = xmlSerializer.Deserialize(reader) as ImportBookDto[];

                foreach (var xmlBook in xmlBooks)
                {
                    if (!IsValid(xmlBook))
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime publishedOn; 
                    var parseDate = DateTime.TryParseExact(xmlBook.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out publishedOn);

                    

                    if (!parseDate)
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    var book = new Book
                    {
                        Name = xmlBook.Name,
                        Genre = (Genre)xmlBook.Genre,
                        Price = xmlBook.Price,
                        Pages = xmlBook.Pages,
                        PublishedOn = publishedOn

                    };

                    context.Add(book);
                    result.AppendLine(string.Format(SuccessfullyImportedBook, book.Name, book.Price));
                }
            }
            context.SaveChanges();
            return result.ToString().Trim();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var result = new StringBuilder();

            var jsonAuthors = JsonConvert.DeserializeObject<List<ImportAuthorDto>>(jsonString);

            foreach (var jsonAuthor in jsonAuthors)
            {
                var emails = context.Authors.Select(x => x.Email).ToList();

                if (!IsValid(jsonAuthor) || emails.Contains(jsonAuthor.Email))
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }

                var author = new Author
                {
                    FirstName = jsonAuthor.FirstName,
                    LastName = jsonAuthor.LastName,
                    Phone = jsonAuthor.Phone,
                    Email = jsonAuthor.Email
                };

               
                foreach (var jsonBook in jsonAuthor.Books)
                {
                    var book = context.Books.Find(jsonBook.Id);
                    if (book == null)
                    {
                        continue;
                    }

                    var authorBook = new AuthorBook
                    {
                        Book = book
                    };

                    author.AuthorsBooks.Add(authorBook);
                }

                if (author.AuthorsBooks.Count == 0)
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }

                context.Add(author);
                result.AppendLine(string
                    .Format(SuccessfullyImportedAuthor, author.FirstName + " " + author.LastName, 
                    author.AuthorsBooks.Count));
                context.SaveChanges();
            }

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