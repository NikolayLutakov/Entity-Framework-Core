using ProductShop.Data;
using ProductShop.Dtos.Import;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using ProductShop.Models;
using System.Xml.Linq;
using ProductShop.Dtos.Export;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            //ResetDatabase(context);

            Console.WriteLine(GetUsersWithProducts(context));



        }

        //08
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .ToList()
                .Where(x => x.ProductsSold.Any(p => p.Buyer != null))
                .OrderByDescending(x => x.ProductsSold.Count)
                .Take(10)
                .Select(x => new ExportUserWithSell
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Age = x.Age,

                    SoldProducts = new ExportUsersWithSalesProductsMapDto
                    {
                        Count = x.ProductsSold.Count(c => c.Buyer != null),
                        SoldProductsList = x.ProductsSold.Where(ps => ps.Buyer != null)
                        .Select(ps => new ExportSoldProductDto
                        {
                            Name = ps.Name,
                            Price = ps.Price
                        })
                        .OrderByDescending(ps => ps.Price)
                        .ToList()
                    }
                }).ToList();
                
                

            var usersAndProducts = new ExportUsersProductsWithSaleDto
            {
                Count = context.Users.Count(u => u.ProductsSold.Any(p => p.Buyer != null)),
                Users = users
            };

            var xmlSerializer = new XmlSerializer(typeof(ExportUsersProductsWithSaleDto), new XmlRootAttribute("Users"));

            var result = new StringBuilder();

           

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, usersAndProducts, namespaces);
            }

            return result.ToString();
        }

        //07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            var categories = context.Categories
                .Select(x => new ExportCategoryDto
                {
                    Name = x.Name,
                    Count = x.CategoryProducts.Count,
                    AveragePrice = x.CategoryProducts.Average(cp => cp.Product.Price), //context.Products.Where(p => p.CategoryProducts.Any(c => c.Category.Name == x.Name)).Average(s => s.Price),
                    TotalRevenue = x.CategoryProducts.Sum(p => p.Product.Price)
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportCategoryDto>), new XmlRootAttribute("Categories"));

            var result = new StringBuilder();

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, categories, namespaces);
            }

            return result.ToString();
        }

        //06
        public static string GetSoldProducts(ProductShopContext context)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);


            var users = context.Users
                .Where(x => x.ProductsSold.Any(b => b.Buyer != null))
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Take(5)
                .Select(x => new ExportUserDto
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SoldProducts = x.ProductsSold
                    .Where(b => b.Buyer != null)
                    .Select(p => new ExportSoldProductDto
                    { 
                        Name = p.Name,
                        Price = p.Price

                    }).ToList()
                })
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportUserDto>), new XmlRootAttribute("Users"));

            var result = new StringBuilder();

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, users, namespaces);
            }

                return result.ToString();
        }

        //05
        public static string GetProductsInRange(ProductShopContext context) 
        {
            //var mapper = InitializeMapper();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);


            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                //.Include(x => x.Buyer)
                .OrderBy(x => x.Price)
                .Take(10)
                .Select(x => new ExportProductDto
                {
                    Name = x.Name,
                    Price = x.Price,
                    BuyerFullName = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .ToList();

            //var productsDto = mapper.Map<List<ExportProductDto>>(products); 
            //Automapper not working in Judge due to memory limit!!!

            
            var xmlSerializer = new XmlSerializer(typeof(List<ExportProductDto>), new XmlRootAttribute("Products"));

            var result = new StringBuilder();

            using (var output = new StringWriter(result))
            {
                xmlSerializer.Serialize(output, products, namespaces);
            }
            return result.ToString().Trim();
        }


        //01
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            var mapper = InitializeMapper();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportUserDto>), new XmlRootAttribute("Users"));

            using (var reader = new StringReader(inputXml))
            {
                var usersDto = (List<ImportUserDto>)xmlSerializer.Deserialize(reader);

                var users = mapper.Map<List<User>>(usersDto).ToList();

                context.AddRange(users);
            }
          
            int count = context.SaveChanges();
           
            return $"Successfully imported {count}";
        }

        //02
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            
            var mapper = InitializeMapper();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportProductDto>), new XmlRootAttribute("Products"));

            using (StringReader reader = new StringReader(inputXml)) 
            {
                var productsDto = (List<ImportProductDto>)xmlSerializer.Deserialize(reader);

                var products = mapper.Map<List<Product>>(productsDto);

                context.AddRange(products);
            }


            int count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        //03

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var mapper = InitializeMapper();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportCategoryDto>), new XmlRootAttribute("Categories"));

            using (StringReader reader = new StringReader(inputXml))
            {
                var categoriesDto = (List<ImportCategoryDto>)xmlSerializer.Deserialize(reader);

                var categories = mapper.Map<List<Category>>(categoriesDto).Where(x => x.Name != null);

                context.AddRange(categories);
            }


            int count = context.SaveChanges();

            return $"Successfully imported {count}";

        }


        //04
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var mapper = InitializeMapper();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportCategoryProductDto>), new XmlRootAttribute("CategoryProducts"));

            using (StringReader reader = new StringReader(inputXml))
            {
                var categoriesProductsDto = (List<ImportCategoryProductDto>)xmlSerializer.Deserialize(reader);

                var categoryIds = context.Categories.Select(x => x.Id).ToList();
                var productIds = context.Products.Select(x => x.Id).ToList();

                var categoriesProducts = mapper.Map<List<CategoryProduct>>(categoriesProductsDto).Where(x => categoryIds.Contains(x.CategoryId) && productIds.Contains(x.ProductId));



                context.AddRange(categoriesProducts);
            }

            int count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        private static string FileReader(string selector)
        {
            string input;

            switch (selector)
            {
                case "c":
                    input = File.ReadAllText("../../../Datasets/categories.xml");
                    break;
                case "cp":
                    input = File.ReadAllText("../../../Datasets/categories-products.xml");
                    break;
                case "p":
                    input = File.ReadAllText("../../../Datasets/products.xml");
                    break;
                case "u":
                    input = File.ReadAllText("../../../Datasets/users.xml");
                    break;
                default:
                    input = string.Empty;
                    break;
            }

            return input;
        }

        private static IMapper InitializeMapper()
        {
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ProductShopProfile>();
                });

                var mapper = config.CreateMapper();

                return mapper;
            }

        }
        private static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}