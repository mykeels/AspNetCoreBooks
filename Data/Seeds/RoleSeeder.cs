using System;
using Bogus;
using Books.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Books.Data
{
    public class RoleSeeder: DatabaseSeeder
    {
        public RoleSeeder(IServiceProvider serviceProvider): base(serviceProvider) 
        {
        }

        public override async Task Run()
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityRole<Guid>> Run(string roleName)
        {
            var role = new IdentityRole<Guid>()
            {
                NormalizedName = roleName.ToUpper(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Name = roleName
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }
    }
}