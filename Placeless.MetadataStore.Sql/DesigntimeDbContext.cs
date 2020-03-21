using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless.MetadataStore.Sql
{
    class DesigntimeDbFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var _dbContextOptions = SqlServerDbContextOptionsExtensions.UseSqlServer(
                new DbContextOptionsBuilder(),
                "Data Source=(localdb)\\MSSQLLocalDB;Catalog=Placeless2;Integrated Security=True",
                options => options.CommandTimeout(120)
                ).Options;

            return new ApplicationDbContext(_dbContextOptions);
        }
    }
}
