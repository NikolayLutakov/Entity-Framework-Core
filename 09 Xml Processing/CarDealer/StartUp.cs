using CarDealer.Data;
using System.IO;
using System;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using CarDealer.Dtos.Export;
using Microsoft.EntityFrameworkCore;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();


            //var suppliersInputString = File.ReadAllText("../../../Datasets/suppliers.xml");
            //var partsInputString = File.ReadAllText("../../../Datasets/parts.xml");
            //var carsInputString = File.ReadAllText("../../../Datasets/cars.xml");
            //var customersInputString = File.ReadAllText("../../../Datasets/customers.xml");
            //var salesInputString = File.ReadAllText("../../../Datasets/sales.xml");

            //Console.WriteLine(ImportSuppliers(context, suppliersInputString)); //09
            //Console.WriteLine(ImportParts(context, partsInputString)); //10
            //Console.WriteLine(ImportCars(context, carsInputString)); //11
            //Console.WriteLine(ImportCustomers(context, customersInputString)); //12  
            //Console.WriteLine(ImportSales(context, salesInputString)); //13
            
            
            //Console.WriteLine(GetCarsWithDistance(context)); //14
            //Console.WriteLine(GetCarsFromMakeBmw(context)); //15
            //Console.WriteLine(GetLocalSuppliers(context)); //16
            //Console.WriteLine(GetCarsWithTheirListOfParts(context)); //17
            //Console.WriteLine(GetTotalSalesByCustomer(context)); //18
            Console.WriteLine(GetSalesWithAppliedDiscount(context)); //19

        }
        //19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                //.Include(x => x.Customer)
                //.Include(x => x.Car)
                //.ThenInclude(x => x.PartCars)
                //.ThenInclude(x => x.Part)
                .Select(x => new ExportSaleDto
                {
                    Car = new ExportSaleCarDto
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TraveledDistance = x.Car.TravelledDistance
                    },
                    Discount = x.Discount,
                    CustomerName = x.Customer.Name,
                    Price = x.Car.PartCars.Sum(p => p.Part.Price),
                    PriceWithDiscount = (x.Car.PartCars.Sum(p => p.Part.Price)) - (x.Car.PartCars.Sum(p => p.Part.Price) * x.Discount / 100)


                })
                .ToList();

            //var salesDto = new List<ExportSaleDto>();
            //foreach (var sale in sales)
            //{
            //    var saleDto = new ExportSaleDto();
            //    var car = new ExportSaleCarDto()
            //    {
            //        Make = sale.Car.Make,
            //        Model = sale.Car.Model,
            //        TraveledDistance = sale.Car.TravelledDistance

            //    };

            //    saleDto.Car = car;
            //    saleDto.Discount = sale.Discount;
            //    saleDto.CustomerName = sale.Customer.Name;
            //    saleDto.Price = sale.Car.PartCars.Sum(p => p.Part.Price);
            //    saleDto.PriceWithDiscount = (sale.Car.PartCars.Sum(p => p.Part.Price)) - (sale.Car.PartCars.Sum(p => p.Part.Price) * sale.Discount / 100);
            //    salesDto.Add(saleDto);
            //}

            var xmlSerializer = new XmlSerializer(typeof(List<ExportSaleDto>), new XmlRootAttribute("sales"));

            var result = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, sales, namespaces);
            }

            return result.ToString().Trim();
        }

        //18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Include(x => x.Sales)
                .ThenInclude(x => x.Car)
                .ThenInclude(x => x.PartCars)
                .ThenInclude(x => x.Part)
                .Where(x => x.Sales.Count > 0)
                .ToList()
                .Select(x => new ExportCustomerDto 
                { 
                    FullName = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpentMoney = x.Sales.Sum(s => s.Car.PartCars.Sum(p => p.Part.Price))
                })
                .OrderByDescending(x => x.SpentMoney)
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportCustomerDto>), new XmlRootAttribute("customers"));

            var result = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, customers, namespaces);
            }

            return result.ToString();
        }

        //17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                 .OrderByDescending(x => x.TravelledDistance)
                 .ThenBy(x => x.Model)
                 .Take(5)
                 .Select(x => new ExportCarWithListOfPartsDto
                 {
                     Make = x.Make,
                     Model = x.Model,
                     TraveledDistance = x.TravelledDistance,
                     Parts = x.PartCars.Select(p => new ExportPartDto
                     {
                         Name = p.Part.Name,
                         Price = p.Part.Price
                     })
                     .OrderByDescending(p => p.Price)
                     .ToList()
                 })
                 .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportCarWithListOfPartsDto>), new XmlRootAttribute("cars"));

            var result = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, cars, namespaces);
            }

            return result.ToString();
        }

        //16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new ExportLocalSuppliersDto 
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                })
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportLocalSuppliersDto>), new XmlRootAttribute("suppliers"));

            var result = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, suppliers, namespaces);
            }

            return result.ToString();
        }

        //15
        public static string GetCarsFromMakeBmw(CarDealerContext context) 
        {
            var bmws = context.Cars
                .Where(x => x.Make == "BMW")
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .Select(x => new ExportBmwDto 
                {
                    Id = x.Id,
                    Model = x.Model,
                    TraveledDistance = x.TravelledDistance
                })
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportBmwDto>), new XmlRootAttribute("cars"));

            var result = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, bmws, namespaces);
            }

            return result.ToString();
        }

        //14
        public static string GetCarsWithDistance(CarDealerContext context) 
        {
            var cars = context.Cars
                .Where(x => x.TravelledDistance > 2000000)
                .OrderBy(x => x.Make)
                .ThenBy(x => x.Model)
                .Take(10)
                .Select(x => new ExportCarDto
                {
                    Make = x.Make,
                    Model = x.Model,
                    TraveledDistance = x.TravelledDistance
                })
                .ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<ExportCarDto>), new XmlRootAttribute("cars"));

            var result = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(String.Empty, String.Empty);

            using (var writer = new StringWriter(result))
            {
                xmlSerializer.Serialize(writer, cars, namespaces);
            }
                
            return result.ToString();
        }

        //13
        public static string ImportSales(CarDealerContext context, string inputXml) 
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ImportSaleDto>), new XmlRootAttribute("Sales"));

            var count = -1;

            using (var reader = new StringReader(inputXml))
            {
                var salesDto = xmlSerializer.Deserialize(reader) as List<ImportSaleDto>;

                var cars = context.Cars.Select(x => x.Id);

                var sales = salesDto
                    .Where(x => cars.Contains(x.CarId))
                    .Select(x => new Sale()
                    {
                        CarId = x.CarId,
                        CustomerId = x.CustomerId,
                        Discount = x.Discount
                    });
                context.AddRange(sales);
                count = sales.Count();
            }

                context.SaveChanges();
            

            return $"Successfully imported {count}";
        }

        //12
        public static string ImportCustomers(CarDealerContext context, string inputXml) 
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ImportCustomerDto>), new XmlRootAttribute("Customers"));

            var count = -1;
            using (var reader = new StringReader(inputXml))
            {
                var customersDto = xmlSerializer.Deserialize(reader) as List<ImportCustomerDto>;



                var customers = customersDto.Select(x => new Customer
                {
                    Name = x.Name,
                    BirthDate = DateTime.Parse(x.BirthDate),
                    IsYoungDriver = x.IsYoungDriver
                });

                count = customers.Count();

                context.AddRange(customers);
            }



                context.SaveChanges();
           

            return $"Successfully imported {count}";
        }

        //11
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ImportCarDto>), new XmlRootAttribute("Cars"));

            var carsToImport = new List<Car>();
            using (var reader = new StringReader(inputXml))
            {
                var carsDto = xmlSerializer.Deserialize(reader) as List<ImportCarDto>;

                
               

                foreach (var car in carsDto)
                {
                    var curCar = new Car()
                    {
                        Make = car.Make,
                        Model = car.Model,
                        TravelledDistance = car.TravelledDistance
                    };

                    var parts = car
                        .Parts
                        .Where(pc => context.Parts.Any(p => p.Id == pc.PartId))
                        .Select(p => p.PartId)
                        .Distinct();
                    
                    var partCars = new List<PartCar>();

                    foreach (var part in parts)
                    {
                        PartCar partCar = new PartCar()
                        {
                            PartId = part,
                            Car = curCar
                        };

                        partCars.Add(partCar);
                    }
                    curCar.PartCars = partCars;

                    carsToImport.Add(curCar);
                   
                }
                context.Cars.AddRange(carsToImport);
                

            }
            context.SaveChanges();
            var count = carsToImport.Count;

            return $"Successfully imported {count}";
        }

        //10
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ImportPartDto>), new XmlRootAttribute("Parts"));

            using (var reader = new StringReader(inputXml))
            {
                var partsDto = xmlSerializer.Deserialize(reader) as List<ImportPartDto>;

                var validIds = context.Suppliers.Select(x => x.Id).ToList();

                var parts = partsDto
                    .Where(x => validIds.Contains(x.SupplierId))
                    .Select(x => new Part
                    {
                        Name = x.Name,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        SupplierId = x.SupplierId
                    })
                    .ToList();

                context.AddRange(parts);
            }


            var count = context.SaveChanges();

            return $"Successfully imported {count}";
        }

        //09
        public static string ImportSuppliers(CarDealerContext context, string inputXml) 
        {
            

            var xmlSerializer = new XmlSerializer(typeof(List<ImportSupplierDto>), new XmlRootAttribute("Suppliers"));

            using (var reader = new StringReader(inputXml))
            {
                var suppliersDto = xmlSerializer.Deserialize(reader) as List<ImportSupplierDto>;

                var suppliers = suppliersDto.Select(x => new Supplier
                {
                     Name = x.Name,
                     IsImporter = x.IsImporter
                })
                .ToList();

                context.AddRange(suppliers);
            }


            var count = context.SaveChanges();

            return $"Successfully imported {count}";
        }
    }
}