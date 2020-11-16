using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using WebAPILogInRegistration.Models;

namespace WebAPILogInRegistration.WebAPILogInRegistration
{
    public class BloggingContextFactory : IDesignTimeDbContextFactory<AuthenticationContext>
    {
        public AuthenticationContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthenticationContext>();
            optionsBuilder.UseSqlServer("Server=(localDB)\\MSSQLLocalDB;Database=UserDB;Trusted_Connection=true;MultipleActiveResultSets=True;");

            return new AuthenticationContext(optionsBuilder.Options);
        }
    }
}
