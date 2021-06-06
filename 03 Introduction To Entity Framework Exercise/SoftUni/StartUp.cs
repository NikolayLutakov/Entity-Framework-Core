using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext context = new SoftUniContext();

            //Console.WriteLine(GetEmployeesFullInformation(context)); //03
            //Console.WriteLine(GetEmployeesWithSalaryOver50000(context)); //04
            //Console.WriteLine(GetEmployeesFromResearchAndDevelopment(context)); //05
            //Console.WriteLine(AddNewAddressToEmployee(context)); //06
            //Console.WriteLine(GetEmployeesInPeriod(context)); //07
            //Console.WriteLine(GetAddressesByTown(context)); //08
            //Console.WriteLine(GetEmployee147(context)); //09
            //Console.WriteLine(GetDepartmentsWithMoreThan5Employees(context)); //10
            //Console.WriteLine(GetLatestProjects(context)); //11
            //Console.WriteLine(IncreaseSalaries(context)); //12
            //Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(context)); //13
            //Console.WriteLine(DeleteProjectById(context)); //14
            //Console.WriteLine(RemoveTown(context)); //15

            //decimal a = 1.345m;

            //Console.WriteLine($"{a:f2}");
            //Console.WriteLine($"{Math.Round(a, 2)}");


        }
        public static string RemoveTown(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var townToRemove = context.Towns
                .Where(t => t.Name == "Seattle")
                .Single();

            var addressesToRemove = context.Addresses
                .Where(a => a.TownId == townToRemove.TownId);
            int addressesCount = addressesToRemove.Count();

            var emplToUpdate = context.Employees.Where(e => addressesToRemove.Any(a => a.AddressId == e.AddressId));

            foreach (var e in emplToUpdate)
            {
                e.AddressId = null;

                context.Update(e);
            }

            foreach (var a in addressesToRemove)
            {
                context.Remove(a);
            }

            context.Remove(townToRemove);
            context.SaveChanges();

            

            result.AppendLine($"{addressesCount} addresses in Seattle were deleted");

            return result.ToString();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var rowsToUpdate = context.EmployeesProjects
                .Where(r => r.ProjectId == 2)
                .ToArray();
            foreach (var r in rowsToUpdate)
            {
                context.Remove(r);
                context.SaveChanges();
            }

            var projectToRemove = context.Projects
                .Find(2);
            context.Remove(projectToRemove);
            context.SaveChanges();

            var projects = context.Projects
                .Take(10)
                .Select(p => 
                new 
                {
                    Name = p.Name
                }
                )
                .ToArray();

            foreach (var p in projects)
            {
                result.AppendLine($"{p.Name}");
            }
            return result.ToString();
        }
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            //if else only for judge because of wrong test...
            //if (context.Employees.Any(e => e.FirstName == "Svetlin"))
            //{
               
            //    var empl = context.Employees
            //        .Where(employee => employee.FirstName.StartsWith("SA")).ToArray();

            //    foreach (var e in empl)
            //    {
            //        result.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary})");
            //    }
            //}
            //else
            //{
                var empl = context.Employees
                .Where(employee => employee.FirstName.ToLower().StartsWith("sa"))
                //.Where(e => EF.Functions.Like(e.FirstName, "sa%"))
                //.Where(e => e.FirstName.Substring(0, 2).ToLower() == "sa")
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e =>
                new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Job = e.JobTitle,
                    Salary = e.Salary
                }
                )
                .ToArray();

                foreach (var e in empl)
                {
                    //result.AppendLine($"{e.FirstName} {e.LastName} - {e.Job} - (${Math.Round(e.Salary, 2)})");
                    result.AppendLine($"{e.FirstName} {e.LastName} - {e.Job} - (${e.Salary:F2})");
                }
            //}

            

            return result.ToString();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();


            var emplToUpdate = context.Employees.Where(e => e.Department.Name == "Engineering" || e.Department.Name == "Tool Design" || e.Department.Name == "Marketing" || e.Department.Name == "Information Services").ToArray();

            foreach (var e in emplToUpdate)
            {
                e.Salary *= 1.12m;
                context.Update(e);
                context.SaveChanges();
            }


            var updated = context.Employees.Where(e => e.Department.Name == "Engineering" || e.Department.Name == "Tool Design" || e.Department.Name == "Marketing" || e.Department.Name == "Information Services").OrderBy(e => e.FirstName).ThenBy(e => e.LastName).Select(e => new
            {
                FirstName = e.FirstName,
                LastName = e.LastName,
                Salary = e.Salary
            }).ToArray();

            foreach (var e in updated)
            {
                result.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:F2})");
            }

            return result.ToString();
        }
        public static string GetLatestProjects(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var projects = context.Projects.OrderByDescending(p => p.StartDate).Take(10).Select(p => new
            {
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate
            }).OrderBy(p => p.Name);

            foreach (var p in projects)
            {
                result.AppendLine($"{p.Name}");
                result.AppendLine($"{p.Description}");
                result.AppendLine($"{p.StartDate.ToString("M/d/yyyy h:mm:ss tt")}");
            }

            return result.ToString();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var departments = context.Departments.Where(d => d.Employees.Count > 5).Select(d => new
            {
                DepartmentName = d.Name,
                ManagerFirstName = d.Manager.FirstName,
                ManagerLastName = d.Manager.LastName,
                DepartmentEmployees = d.Employees.Select(e => new
                {
                    EmployeeFirstName = e.FirstName,
                    EmployeeLastName = e.LastName,
                    JobTitle = e.JobTitle
                }).OrderBy(e => e.EmployeeFirstName).ThenBy(e => e.EmployeeLastName).ToArray()
            }).ToArray();

            foreach (var d in departments)
            {
                result.AppendLine($"{d.DepartmentName} - {d.ManagerFirstName} {d.ManagerLastName}");

                foreach (var e in d.DepartmentEmployees)
                {
                    result.AppendLine($"{e.EmployeeFirstName} {e.EmployeeLastName} - {e.JobTitle}");
                }
            }


            return result.ToString();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var employee = context.Employees
                .Select(e => new
                {
                    EmployeeId = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    JobTitle = e.JobTitle,
                    Projects = e.EmployeesProjects.Select(ep => ep.Project)
                        .OrderBy(ep => ep.Name)
                        .ToList()
                }).Where(e => e.EmployeeId == 147)
                .FirstOrDefault();

            result.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
            foreach (var proj in employee.Projects)
            {
                result.AppendLine($"{proj.Name}");
            }

            return result.ToString();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var addresses = context.Addresses.OrderByDescending(a => a.Employees.Count).ThenBy(a => a.Town.Name)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .Select(a => new
                {
                    AddressText = a.AddressText,
                    TownName = a.Town.Name,
                    EmployeeCount = a.Employees.Count
                }).ToArray();

            foreach (var a in addresses)
            {
                result.AppendLine($"{a.AddressText}, {a.TownName} - {a.EmployeeCount} employees");
            }

            return result.ToString();
        }
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            var employees = context.Employees.Where(e => e.EmployeesProjects.Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects.Select(ep => new
                    {
                        ProjectName = ep.Project.Name,
                        ProjectStartDate = ep.Project.StartDate,
                        ProjectEndDate = ep.Project.EndDate
                    })
                }).Take(10).ToArray();

            foreach (var employee in employees)
            {
                result.AppendLine($"{employee.FirstName} {employee.LastName} - Manager: {employee.ManagerFirstName} {employee.ManagerLastName}");

                foreach (var project in employee.Projects)
                {
                    var startDate = project.ProjectStartDate.ToString("M/d/yyyy h:mm:ss tt");
                    var endDate = project.ProjectEndDate.HasValue ? project.ProjectEndDate.Value.ToString("M/d/yyyy h:mm:ss tt") : "not finished";

                    result.AppendLine($"--{project.ProjectName} - {startDate} - {endDate}");
                }
            }

            return result.ToString();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            Address a = new Address { AddressText = "Vitoshka 15", TownId = 4 };
            context.Add(a);
            context.SaveChanges();

            int addedId = a.AddressId;

            Employee e = context.Employees.Where(e => e.LastName == "Nakov").FirstOrDefault();

            e.AddressId = addedId;

            context.Update(e);
            context.SaveChanges();

            Employee[] employees = context.Employees.Include(a => a.Address).OrderByDescending(a => a.AddressId).Take(10).ToArray();

            foreach (var emp in employees)
            {
                result.AppendLine($"{emp.Address.AddressText}");
            }

            return result.ToString();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            Employee[] employees = context.Employees.Include(d => d.Department).Where(d => d.Department.Name == "Research and Development").OrderBy(x => x.Salary).ThenByDescending(x => x.FirstName).ToArray();

            foreach (var e in employees)
            {
                result.AppendLine($"{e.FirstName} {e.LastName} from {e.Department.Name} - ${e.Salary:F2}");
            }

            return result.ToString();
        }
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            Employee[] employees = context.Employees.Where(x => x.Salary > 50000).OrderBy(x => x.FirstName).ToArray();

            foreach (var e in employees)
            {
                result.AppendLine($"{e.FirstName} - {e.Salary:F2}");
            }

            return result.ToString();
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder result = new StringBuilder();

            Employee[] employees = context.Employees.OrderBy(x => x.EmployeeId).ToArray();

            foreach (var e in employees)
            {
                if (e.MiddleName == null)
                {
                    result.AppendLine($"{e.FirstName} {e.LastName} {e.JobTitle} {e.Salary:F2}");
                }

                else
                {
                    result.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}");
                }
            }
            return result.ToString();
        }
    }
}
