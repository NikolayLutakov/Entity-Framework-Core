using MiniORM;
using MIniORM.App.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MIniORM.App.Data
{
    class SoftUniDbContext : DbContext
    {
        public SoftUniDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<Employee> Employees { get; }

        public DbSet<Department> Departments { get; }
        public DbSet<Project> Projects { get; }

        public DbSet<EmployeeProject> EmployeesProjects { get; }

    }
}
