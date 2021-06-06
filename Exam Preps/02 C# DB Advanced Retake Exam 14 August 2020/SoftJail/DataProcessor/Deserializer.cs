namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Globalization;
    using Newtonsoft.Json.Converters;
    using System.Xml.Serialization;
    using System.IO;
    using SoftJail.Data.Models.Enums;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var result = new StringBuilder();

            var departmetsDeserialized = JsonConvert.DeserializeObject<List<Department>>(jsonString)
                .ToList();

            var departments = new List<Department>();
            

            foreach (var record in departmetsDeserialized)
            {
                var cells = new List<Cell>();

                if (!IsValid(record))
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }


                var departmentToAdd = departments.FirstOrDefault(x => x.Name == record.Name);

                if (departmentToAdd == null)
                {
                    departmentToAdd = new Department()
                    {
                        Name = record.Name
                    };
                }

                foreach (var cell in record.Cells)
                {
                    if (!IsValid(cell))
                    {
                        break;
                    }

                    var cellToAdd = cells.FirstOrDefault(x => x.CellNumber == cell.CellNumber);

                    if (cellToAdd == null)
                    {
                        cellToAdd = new Cell()
                        {
                            CellNumber = cell.CellNumber,
                            HasWindow = cell.HasWindow
                        };
                        cells.Add(cellToAdd);
                    }
                }
                if (cells.Count == 0)
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }
                departmentToAdd.Cells = cells;
                departments.Add(departmentToAdd);
                result.AppendLine($"Imported {departmentToAdd.Name} with {departmentToAdd.Cells.Count} cells");
            }

            context.AddRange(departments);
            context.SaveChanges();

            

            return result.ToString().Trim();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var result = new StringBuilder();

            var format = "dd/MM/yyyy"; // your datetime format
            var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };

            var prisonersDeserialized = JsonConvert.DeserializeObject<List<Prisoner>>(jsonString, dateTimeConverter).ToList();

           

            var prisoners = new List<Prisoner>();

            foreach (var record in prisonersDeserialized)
            {

                var mails = new List<Mail>();

                if (!IsValid(record))
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }
                

                foreach (var mail in record.Mails)
                {
                    if (!IsValid(mail))
                    {
                        break;
                    }

                    mails.Add(mail);
                }

                if (mails.Count == 0)
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }

                record.Mails = mails;

                prisoners.Add(record);

                result.AppendLine($"Imported {record.FullName} {record.Age} years old");

            }

            context.AddRange(prisoners);
            context.SaveChanges();
            
            return result.ToString().Trim();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var result = new StringBuilder();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportOfficerDto>), new XmlRootAttribute("Officers"));

            var officersToAdd = new List<Officer>();

            using (StringReader reader = new StringReader(xmlString))
            {
                var officers = xmlSerializer.Deserialize(reader) as List<ImportOfficerDto>;

                

                foreach (var record in officers)
                {
                    if (!IsValid(record))
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    Position position;
                    bool positionParse = Enum.TryParse<Position>(record.Position, out position);

                    Weapon weapon;
                    bool weaponParse = Enum.TryParse<Weapon>(record.Weapon, out weapon);

                    if (!positionParse)
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    if (!weaponParse)
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    var officerToAdd = new Officer()
                    {
                        FullName = record.Name,
                        Salary = record.Money,
                        Position = position,
                        Weapon = weapon,
                        DepartmentId = record.DepartmentId
                    };

                    var prisoners = new List<OfficerPrisoner>();
                    
                    foreach (var prisoner in record.Prisoners)
                    {

                        var prisonerExported = context.Prisoners.Find(prisoner.Id);

                        var prisonerToAdd = new OfficerPrisoner()
                        {
                            Officer = officerToAdd,
                            Prisoner = prisonerExported,
                            
                        };

                        prisoners.Add(prisonerToAdd);

                    }

                    officerToAdd.OfficerPrisoners = prisoners;
                    officersToAdd.Add(officerToAdd);
                   
                    result.AppendLine($"Imported {officerToAdd.FullName} ({officerToAdd.OfficerPrisoners.Count} prisoners)");
                }


            }
            
            context.AddRange(officersToAdd);
            context.SaveChanges();

                


                
            
            return result.ToString().Trim();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}