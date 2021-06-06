using MIniORM.App.Data;
using MIniORM.App.Data.Entities;
using System;
using System.Linq;

namespace MIniORM.App
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server = 192.168.0.116,1433; Database = MiniORM; User ID = nlyutakov; Password = A123456a;";

            SoftUniDbContext context = new SoftUniDbContext(connectionString);

            context.Employees.Add(new Employee
            {
                FirstName = "Gosho",
                LastName = "Inserted",
                DepartmentId = context.Departments.First().Id,
                IsEmployed = true
            });

            Employee employee = context.Employees.First();
            employee.FirstName = "Modified";

            context.SaveChanges();
        }
    }
}
