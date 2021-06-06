namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var result = new StringBuilder();

            var authors = context.Authors
                .Select(x => new
                {
                    AuthorName = x.FirstName + " " + x.LastName,
                    Books = x.AuthorsBooks
                    .OrderByDescending(b => b.Book.Price)
                    .Select(b => new 
                    {
                        BookName = b.Book.Name,
                        BookPrice = b.Book.Price.ToString("F2")
                    })
                    .ToList()
                })
                .ToList()
                .OrderByDescending(x => x.Books.Count)
                .ThenBy(x => x.AuthorName);

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            result.AppendLine(JsonConvert.SerializeObject(authors, settings));

            return result.ToString().Trim();
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var result = new StringBuilder();

            Genre g = Enum.Parse<Genre>("3");
            
            var booksDto = context.Books.Where(x => x.PublishedOn < date && x.Genre == g)
                .ToList()
                .OrderByDescending(x => x.Pages)
                .ThenByDescending(x => x.PublishedOn)
                .Select(x => new ExportBookDto
                {
                    Pages = x.Pages,
                    Name = x.Name,
                    Date = x.PublishedOn.ToString("d", CultureInfo.InvariantCulture)
                })
                .Take(10)
                .ToList();


            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ExportBookDto>), new XmlRootAttribute("Books"));

            using (StringWriter writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, booksDto, namespaces);
            }

            return result.ToString().Trim();
        }
    }
}