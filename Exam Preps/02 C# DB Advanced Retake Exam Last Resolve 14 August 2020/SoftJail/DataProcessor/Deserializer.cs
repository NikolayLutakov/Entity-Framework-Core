namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var result = new StringBuilder();

            var jsonDepartments = JsonConvert.DeserializeObject<List<ImportDepartmentDto>>(jsonString);

            foreach (var jsonDepartment in jsonDepartments)
            {
                if (!IsValid(jsonDepartment) || jsonDepartment.Cells.Count == 0 || !jsonDepartment.Cells.All(IsValid))
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = jsonDepartment.Name,
                    Cells = jsonDepartment.Cells.Select(c => new Cell
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow
                    })
                    .ToList()
                    
                };

                context.Add(department);
                result.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");

            }
            context.SaveChanges();
            return result.ToString().Trim();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var result = new StringBuilder();

            var jsonPrisoners = JsonConvert.DeserializeObject<List<ImportPrisonerDto>>(jsonString);

            foreach (var jsonPrisoner in jsonPrisoners)
            {
                if (!IsValid(jsonPrisoner) || !jsonPrisoner.Mails.All(IsValid))
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }

                DateTime incarcerationDate;
                var parseIn = DateTime.TryParseExact(jsonPrisoner.IncarcerationDate,
                            "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out incarcerationDate);
                
                DateTime releaseDate;
                var parseOut = DateTime.TryParseExact(jsonPrisoner.ReleaseDate,
                            "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out releaseDate);

                if (!parseIn)
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }
                

                var prisoner = new Prisoner
                {
                    FullName = jsonPrisoner.FullName,
                    Nickname = jsonPrisoner.Nickname,
                    Age = jsonPrisoner.Age,
                    IncarcerationDate = incarcerationDate,
                    ReleaseDate = releaseDate,
                    Bail = jsonPrisoner.Bail,
                    CellId = jsonPrisoner.CellId,
                    Mails = jsonPrisoner.Mails.Select(m => new Mail 
                    {
                        Description = m.Description,
                        Sender = m.Sender,
                        Address = m.Address
                    })
                    .ToArray()
                };

                context.Add(prisoner);
                result.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.SaveChanges();
            return result.ToString().Trim();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var result = new StringBuilder();

            var xmlSerializer = new XmlSerializer(typeof(ImportOfficerDto[]), new XmlRootAttribute("Officers"));

            using (var reader = new StringReader(xmlString))
            {
                var xmlOfficers = xmlSerializer.Deserialize(reader) as ImportOfficerDto[];

                foreach (var xmlOfficer in xmlOfficers)
                {
                    if (!IsValid(xmlOfficer))
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    Position position;
                    var parsePos = Enum.TryParse<Position>(xmlOfficer.Position, out position);

                    Weapon weapon;
                    var parseWeapon = Enum.TryParse<Weapon>(xmlOfficer.Weapon, out weapon);

                    if (!parsePos || !parseWeapon)
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    var officer = new Officer
                    {
                        FullName = xmlOfficer.Name,
                        Salary = xmlOfficer.Money,
                        Position = position,
                        Weapon = weapon,
                        DepartmentId = xmlOfficer.DepartmentId,

                    };

                    foreach (var xmlPrisoner in xmlOfficer.Prisoners)
                    {
                        var prisonerOfficer = new OfficerPrisoner
                        {
                            PrisonerId = xmlPrisoner.PrisonerId
                        };

                        officer.OfficerPrisoners.Add(prisonerOfficer);
                    }

                    context.Add(officer);
                    result.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
                }
            }
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