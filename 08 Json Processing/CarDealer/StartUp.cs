using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();           

            //var jsonString = JsonInitialize();

            //Console.WriteLine(ImportSuppliers(context, jsonString[0])); //09
            //Console.WriteLine(ImportParts(context, jsonString[1])); //10
            //Console.WriteLine(ImportCars(context, jsonString[2])); //11
            //Console.WriteLine(ImportCustomers(context, jsonString[3])); //12
            //Console.WriteLine(ImportSales(context, jsonString[4])); //13

            //Console.WriteLine(GetOrderedCustomers(context)); //14
            //Console.WriteLine(GetCarsFromMakeToyota(context)); //15
            //Console.WriteLine(GetLocalSuppliers(context)); //16

            //Console.WriteLine(GetCarsWithTheirListOfParts(context)); //17
            //Console.WriteLine(GetTotalSalesByCustomer(context)); //18
            //Console.WriteLine(GetSalesWithAppliedDiscount(context)); //19




        }

        public static IMapper MapperInitialize()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            var mapper = config.CreateMapper();

            return mapper;
        }

        public static List<string> JsonInitialize()
        {
            var jsonStrings = new List<string>();

            jsonStrings.Add(File.ReadAllText("../../../Datasets/suppliers.json")); //0
            jsonStrings.Add(File.ReadAllText("../../../Datasets/parts.json"));     //1
            jsonStrings.Add(File.ReadAllText("../../../Datasets/cars.json"));      //2
            jsonStrings.Add(File.ReadAllText("../../../Datasets/customers.json")); //3
            jsonStrings.Add(File.ReadAllText("../../../Datasets/sales.json"));     //4


            return jsonStrings;
        }

        //09
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var mapper = MapperInitialize();

            var suppliersDto = JsonConvert
                .DeserializeObject<List<ImportSupplierDto>>(inputJson)
                .ToList();

            var supliers = mapper.Map<List<Supplier>>(suppliersDto)
                .ToList();

            context.AddRange(supliers);

            var rowCount = context.SaveChanges();

            return $"Successfully imported {rowCount}.";
        }
        //10
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var mapper = MapperInitialize();

            var suppliersIds = context.Suppliers
                .Select(x => x.Id)
                .ToList();

            var partsDto = JsonConvert
                .DeserializeObject<List<ImportPartDto>>(inputJson).ToList();


            var validParts = partsDto
                .Where(x => suppliersIds
                                .Contains(x.SupplierId))
                .ToList();
                

            var parts = mapper.Map<List<Part>>(validParts);

            context.AddRange(parts);

            var rowCount = context.SaveChanges();

            return $"Successfully imported {rowCount}.";
        }
        //11
        public static string ImportCars(CarDealerContext context, string inputJson)
        {

            var importCars = JsonConvert
                .DeserializeObject<List<ImportCarDto>>(inputJson)
                .ToList();


            var cars = new List<Car>();

            foreach (var car in importCars)
            {
                var curCar = new Car
                {
                    Make = car.Make,
                    Model = car.Model,
                    TravelledDistance = car.TravelledDistance
                };

                foreach (var id in car.PartsId.Distinct())
                {
                    curCar.PartCars.Add(new PartCar
                    {
                        PartId = id
                    });
                }

                cars.Add(curCar);
            }
            ;
            context.AddRange(cars);

            context.SaveChanges();

            var rowCount = cars.Count;

            return $"Successfully imported {rowCount}.";
        }
       
        //12
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var mapper = MapperInitialize();

            var importCustomersDto = JsonConvert.DeserializeObject<List<ImportCustomerDto>>(inputJson).ToList();

            var customers = mapper.Map<List<Customer>>(importCustomersDto).ToList();

            context.AddRange(customers);

            var rowCount = context.SaveChanges();

            return $"Successfully imported {rowCount}.";
        }

        //13
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var mapper = MapperInitialize();

            var importSalesDto = JsonConvert.DeserializeObject<List<ImportSaleDto>>(inputJson).ToList();

            var sales = mapper.Map<List<Sale>>(importSalesDto).ToList();

            context.AddRange(sales);

            var rowCount = context.SaveChanges();

            return $"Successfully imported {rowCount}.";
        }

        //14
        public static string GetOrderedCustomers(CarDealerContext context)
        {

            var customers = context.Customers
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver)
                .Select(x => new 
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate,
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                DateFormatString = "dd/MM/yyyy"

            };

            return JsonConvert.SerializeObject(customers, settings);
        }

        //15
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context.Cars.Where(x => x.Make == "Toyota")
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .Select(x => new
                {
                    Id = x.Id,
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(cars, settings);
        }

        //16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new 
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                })
                .ToList();



            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(suppliers, settings);
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(x => new 
                {
                    car = new
                    {
                        Make = x.Make,
                        Model = x.Model,
                        TravelledDistance = x.TravelledDistance
                    },
                    parts = x.PartCars.Select(p => new 
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price.ToString("F2")
                    })
                })
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(cars, settings);
        }

        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context) 
        {
            var customers = context.Customers
                .Where(x => x.Sales.Count > 0)
                .Select(x => new
                {
                    FullName = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpentMoney = x.Sales.Select(z => z.Car.PartCars.Select(d => d.Part.Price).Sum()).Sum()
                })
                .OrderByDescending(x => x.SpentMoney)
                .ThenByDescending(x => x.BoughtCars)
                .ToList();

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(customers, settings);
        }


        //19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Take(10)
                .Select(x => new
                {
                    car = new
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    customerName = x.Customer.Name,
                    Discount = x.Discount.ToString("F2"),
                    price = x.Car.PartCars.Sum(p => p.Part.Price).ToString("F2"),
                    priceWithDiscount = (x.Car.PartCars.Sum(p => p.Part.Price) - ((x.Car.PartCars.Sum(p => p.Part.Price) * (x.Discount / 100)))).ToString("F2")

                })
                .ToList();

          

            var settings = new JsonSerializerSettings()
            {
                //ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() },
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(sales, settings);
        }
    }
}