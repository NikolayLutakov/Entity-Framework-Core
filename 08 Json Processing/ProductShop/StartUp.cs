using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            var jsonStrings = Initialize();

            //Console.WriteLine(ImportUsers(context, jsonStrings[0])); //01
            //Console.WriteLine(ImportProducts(context, jsonStrings[1])); //02
            //Console.WriteLine(ImportCategories(context, jsonStrings[2])); //03
            //Console.WriteLine(ImportCategoryProducts(context, jsonStrings[3])); //04
            //Console.WriteLine(GetProductsInRange(context)); //05
            //Console.WriteLine(GetSoldProducts(context)); //06
            //Console.WriteLine(GetCategoriesByProductsCount(context)); //07
            //Console.WriteLine(GetUsersWithProducts(context)); //08

        }
        public static List<string> Initialize()
        {
            var jsonStrings = new List<string>();

            jsonStrings.Add(File.ReadAllText("../../../Datasets/users.json"));
            jsonStrings.Add(File.ReadAllText("../../../Datasets/products.json"));
            jsonStrings.Add(File.ReadAllText("../../../Datasets/categories.json"));
            jsonStrings.Add(File.ReadAllText("../../../Datasets/categories-products.json"));

            return jsonStrings;
        }

        //01
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {

            var users = JsonConvert.DeserializeObject<List<User>>(inputJson);

            context.AddRange(users);

            int afectedRows = context.SaveChanges();

            return $"Successfully imported {afectedRows}";
        }
        
        //02
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<List<Product>>(inputJson);

            context.AddRange(products);

            int afectedRows = context.SaveChanges();

            return $"Successfully imported {afectedRows}";
        }

        //03
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {

            var categories = JsonConvert.DeserializeObject<List<Category>>(inputJson)
                .Where(x => x.Name != null).ToList();

            //var items = categories.Where(x => x.Name == null).ToList();
            
            //foreach (var item in items)
            //{
            //    categories.Remove(item);
            //}

            context.AddRange(categories);


            int afectedRows = context.SaveChanges();

            return $"Successfully imported {afectedRows}";
        }


        //04
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson) 
        {
            var items = JsonConvert.DeserializeObject<List<CategoryProduct>>(inputJson).ToList();

            context.AddRange(items);

            int afectedRows = context.SaveChanges();

            return $"Successfully imported {afectedRows}";
        }

        //05
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products.Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new
                {
                    Name = x.Name,
                    Price = x.Price,
                    Seller = $"{x.Seller.FirstName} {x.Seller.LastName}"
                })
                .OrderBy(x => x.Price)
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(products, settings);
        }

        //06
        public static string GetSoldProducts(ProductShopContext context)
        {

            var users = context.Users.Where(p => p.ProductsSold.Any(x => x.BuyerId != null))
                .Select(x => new 
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SoldProducts = x.ProductsSold.Select(p => new
                    {
                        Name = p.Name,
                        Price = p.Price,
                        BuyerFirstName = p.Buyer.FirstName,
                        BuyerLastName = p.Buyer.LastName

                    })
                
                })
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(users, settings);
        }

        //07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {

            var categories = context.Categories.Select(x => new
                {
                    Category = x.Name,
                    ProductsCount = x.CategoryProducts.Count(),
                    AveragePrice = x.CategoryProducts.Average(s => s.Product.Price).ToString("F2"),
                    TotalRevenue = x.CategoryProducts.Sum(s => s.Product.Price).ToString("F2")
            })
                .OrderByDescending(x => x.ProductsCount)
                .ToList();
            
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
                Formatting = Formatting.Indented,
                
                
            };

            return JsonConvert.SerializeObject(categories, settings);

            
        }

        //08

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .ToList()
                .Where(x => x.ProductsSold.Any(p => p.Buyer != null))
                .OrderByDescending(x => x.ProductsSold.Count(p => p.Buyer != null))
                .Select(x => new 
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Age = x.Age,
                    SoldProducts = new 
                    {
                        Count = x.ProductsSold.Count(p => p.Buyer != null),
                        Products = x.ProductsSold
                            .Where(p => p.Buyer != null)
                            .Select(p => new 
                            {
                                Name = p.Name,
                                Price = p.Price
                            })
                    }
                })
                .ToList();


            var result = new { UsersCount = users.Count(), Users = users}; 

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore

            };

            return JsonConvert.SerializeObject(result, settings);

           
        }
    }

}