using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using Books.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Books.Data
{
    public class RoleSeeder: DatabaseSeeder
    {
        public RoleSeeder(IServiceProvider serviceProvider): base(serviceProvider) 
        {
        }

        public new void Run()
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityRole<Guid>> Run(string roleName)
        {
            var dbRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (dbRole == null)
            {
                dbRole = new IdentityRole<Guid>()
                {
                    NormalizedName = roleName.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Name = roleName
                };
                _context.Roles.Add(dbRole);
                await _context.SaveChangesAsync();
                Console.WriteLine("Seeding Role " + dbRole.Name);
            }
            
            return dbRole;
        }
    }
}