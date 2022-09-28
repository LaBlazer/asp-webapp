using ASP_Webapp.Models;
using Microsoft.EntityFrameworkCore;

namespace ASP_Webapp.Data
{
    public class CompanyContext : DbContext
    {
        public CompanyContext(DbContextOptions options) : base(options)  
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Employer> Employers { get; set; }

        internal void ClearDatabase()
        {
            var tableNames = Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Distinct()
            .ToList();

            foreach (var tableName in tableNames)
            {
                Database.ExecuteSqlRaw($"PRAGMA foreign_keys = 0;DELETE FROM {tableName}; DELETE FROM SQLITE_SEQUENCE WHERE name='{tableName}';PRAGMA foreign_keys = 1;");
            }
        }
    }
}
