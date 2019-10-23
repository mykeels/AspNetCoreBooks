using Bogus;
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.Models;
using Books.Extensions;
using Books.Services;

namespace Books.Data
{
    public class DatabaseSeeder
    {
        protected readonly IServiceProvider _provider;
        protected readonly IHostEnvironment _environment;
        protected readonly Faker _faker;
        protected readonly BooksDbContext _context;
        protected readonly UserManager<IdentityUser<Guid>> _userManager;
        protected readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public DatabaseSeeder(IServiceProvider serviceProvider) {
            _provider = serviceProvider;

            var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            _faker = new Faker("en");
            _environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            _context = scope.ServiceProvider.GetService<BooksDbContext>();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();
            _roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole<Guid>>>();
        }

        public async Task Migrate()
        {
            if (!DbTypes.IsInMemory() && !_environment.IsTest()) {
                if (DbTypes.IsSqlServer() || DbTypes.IsSqlite()) {
                    await _context.Database.EnsureCreatedAsync();
                }
                else await _context.Database.MigrateAsync();
            }
        }

        public virtual async Task Run() 
        {
            await this.Migrate();

            var roleNames = new List<string>()
            {
                "admin",
                "manager"
            };

            foreach (var roleName in roleNames)
            {
                var role = await new RoleSeeder(_provider).Run(roleName);

                var user = await new UserSeeder(_provider).Run(role);

                var book = await new BookSeeder(_provider).Run(user);
            }

            await _context.SaveChangesAsync();
        }
    }
}