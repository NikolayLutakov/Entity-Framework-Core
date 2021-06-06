namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportProjectDto[]), new XmlRootAttribute("Projects"));

            var result = new StringBuilder();

            using (var reader = new StringReader(xmlString))
            {
                var xmlProjects = xmlSerializer.Deserialize(reader) as ImportProjectDto[];

                foreach (var xmlProject in xmlProjects)
                {
                    if (!IsValid(xmlProject))
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime projOpenDate;
                    var parseProjOpenDate = DateTime.TryParseExact(xmlProject.OpenDate, "dd/MM/yyyy",
                                                            CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                            out projOpenDate);



                    DateTime? projDueDate = null;
                    bool parseProjDueDate = false;

                    if (xmlProject.DueDate != null || xmlProject.DueDate == "")
                    {
                        DateTime projDueDate1;
                        parseProjDueDate = DateTime.TryParseExact(xmlProject.DueDate, "dd/MM/yyyy",
                                                            CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                            out projDueDate1);
                        if (parseProjDueDate)
                        {
                            projDueDate = projDueDate1;
                        }
                        
                    }

                    

                    if (!parseProjOpenDate)
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    

                    var proj = new Project
                    {
                        Name = xmlProject.Name,
                        OpenDate = projOpenDate,
                        DueDate = projDueDate

                    };

                    foreach (var xmlTask in xmlProject.Tasks)
                    {
                        if (!IsValid(xmlTask))
                        {
                            result.AppendLine(ErrorMessage);
                            continue;
                        }

                        DateTime taskOpenDate;
                        var parseTaskOpenDate = DateTime.TryParseExact(xmlTask.OpenDate, "dd/MM/yyyy",
                                                                CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                                out taskOpenDate);

                        DateTime taskDueDate;
                        var parseTaskDueDate = DateTime.TryParseExact(xmlTask.DueDate, "dd/MM/yyyy",
                                                                CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                                out taskDueDate);
                        if (!parseTaskOpenDate || !parseTaskDueDate || taskOpenDate < projOpenDate)
                        {
                            result.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (parseProjDueDate)
                        {
                            if (taskDueDate > projDueDate)
                            {
                                result.AppendLine(ErrorMessage);
                                continue;
                            }
                            
                        }


                        var task = new Task
                        {
                            Name = xmlTask.Name,
                            OpenDate = taskOpenDate,
                            DueDate = taskDueDate,
                            ExecutionType = (ExecutionType)xmlTask.ExecutionType,
                            LabelType = (LabelType)xmlTask.LabelType
                        };

                        proj.Tasks.Add(task);
                        
                        
                    }
                    context.Add(proj);
                    result.AppendLine(string.Format(SuccessfullyImportedProject, proj.Name, proj.Tasks.Count));
                }
            }

            context.SaveChanges();
            return result.ToString().Trim();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var result = new StringBuilder();

            var jsonEmployees = JsonConvert.DeserializeObject<List<ImportEmployeeDto>>(jsonString);

            foreach (var jsonEmployee in jsonEmployees)
            {
                if (!IsValid(jsonEmployee))
                {
                    result.AppendLine(ErrorMessage);
                    continue;
                }

                var tasksIds = jsonEmployee.Tasks.Select(int.Parse).Distinct();

                var employee = new Employee
                {
                    Username = jsonEmployee.Username,
                    Email = jsonEmployee.Email,
                    Phone = jsonEmployee.Phone
                };

                foreach (var jsonTaskId in tasksIds)
                {
                    var task = context.Tasks.Find(jsonTaskId);

                    if (task == null)
                    {
                        result.AppendLine(ErrorMessage);
                        continue;
                    }

                    var emplTask = new EmployeeTask
                    {
                        Task = task
                    };

                    employee.EmployeesTasks.Add(emplTask);
                }

                context.Add(employee);
                result.AppendLine(string.Format(SuccessfullyImportedEmployee, employee.Username, employee.EmployeesTasks.Count));
            }

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