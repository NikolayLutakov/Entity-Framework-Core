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

            var departmentsDto = JsonConvert.DeserializeObject<List<ImportDepartmentDto>>(jsonString).ToList();          
           
            var departmentsToImport = new List<Department>();

            foreach (var departmentDto in departmentsDto)
            {
                if (!IsValid(departmentDto))
                {
                    result.AppendLine("Invalid Data");
                    continue;

                }
                var departmentToImport = new Department()
                {
                    Name = departmentDto.Name
                };

                var departmentCells = new List<Cell>();

                foreach (var cellDto in departmentDto.Cells)
                {
                    if (!IsValid(cellDto))
                    {
                        break;
                    }

                    var cellToImport = new Cell()
                    {
                        CellNumber = cellDto.CellNumber,
                        HasWindow = cellDto.HasWindow
                    };

                    departmentCells.Add(cellToImport);
                }

                if (departmentCells.Count == 0)
                {
                    result.AppendLine("Invalid Data");
                    continue;

                }

                departmentToImport.Cells = departmentCells;

                result.AppendLine($"Imported {departmentToImport.Name} with {departmentToImport.Cells.Count} cells");
                departmentsToImport.Add(departmentToImport);


            }

            context.AddRange(departmentsToImport);
            context.SaveChanges();

            return result.ToString().Trim();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var result = new StringBuilder();

            var prisoners = JsonConvert.DeserializeObject<List<ImportPrisonerDto>>(jsonString).ToList();

            foreach (var prisoner in prisoners)
            {
                if (!IsValid(prisoner))
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }


                DateTime incarcrerationDate;
                var inDate = DateTime.TryParseExact(prisoner.IncarcerationDate, "dd/MM/yyyy",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out incarcrerationDate);

                DateTime releaseDate;
                var outDate = DateTime.TryParseExact(prisoner.ReleaseDate, "dd/MM/yyyy",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

                if (!inDate)
                {
                    result.AppendLine("Invalid Data");
                    continue;
                }


                foreach (var mail in prisoner.Mails)
                {
                    if (!IsValid(mail))
                    {
                        result.AppendLine("Invalid Data");
                        break;
                    }
                }

                var prisonerToImport = new Prisoner
                {
                    FullName = prisoner.FullName,
                    Nickname = prisoner.Nickname,
                    Age = prisoner.Age,
                    IncarcerationDate = incarcrerationDate,
                    ReleaseDate = releaseDate,
                    Bail = prisoner.Bail,
                    CellId = prisoner.CellId,
                    Mails = prisoner.Mails.Select(x => new Mail
                    {
                        Description = x.Description,
                        Sender = x.Sender,
                        Address = x.Address
                    }).ToList()

                };

                context.Add(prisonerToImport);
                result.AppendLine($"Imported {prisonerToImport.FullName} {prisonerToImport.Age} years old");
                context.SaveChanges();
            }

            return result.ToString().Trim();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var result = new StringBuilder();

            var xmlSerializer = new XmlSerializer(typeof(List<ImportOfficerDto>), new XmlRootAttribute("Officers"));

            using (var reader = new StringReader(xmlString))
            {
                var officers = xmlSerializer.Deserialize(reader) as List<ImportOfficerDto>;

                foreach (var officer in officers)
                {
                    if (!IsValid(officer))
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    Position position;
                    var parsePosition = Enum.TryParse<Position>(officer.Position, out position);

                    Weapon weapon;
                    var parseWeapon = Enum.TryParse<Weapon>(officer.Weapon, out weapon);


                    if (!parsePosition || !parseWeapon)
                    {
                        result.AppendLine("Invalid Data");
                        continue;
                    }

                    var officerToAdd = new Officer
                    {
                        FullName = officer.Name,
                        Salary = officer.Money,
                        Position = position,
                        Weapon = weapon,
                        DepartmentId = officer.DepartmentId
                      
                    };

                    foreach (var prisoner in officer.Prisoners)
                    {
                        var prisonerToAdd = context.Prisoners.Find(prisoner.PrisonerId);

                        var officerPrissoner = new OfficerPrisoner
                        {
                            Prisoner = prisonerToAdd
                        };

                        officerToAdd.OfficerPrisoners.Add(officerPrissoner);
                    }

                    context.Add(officerToAdd);
                    result.AppendLine
                        ($"Imported {officerToAdd.FullName} ({officerToAdd.OfficerPrisoners.Count} prisoners)");
                    context.SaveChanges();
                }
            }

               

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






