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
    public class UserSeeder: DatabaseSeeder
    {
        public UserSeeder(IServiceProvider serviceProvider): base(serviceProvider) 
        {
        }

        public new void Run()
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityUser<Guid>> Run(IdentityRole<Guid> role)
        {
            string email = _faker.Internet.Email(provider: "mailinator.com").ToLower();
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (dbUser == null)
            {
                dbUser = new IdentityUser<Guid>()
                {
                    UserName = email,
                    NormalizedUserName = email.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    PhoneNumber = _faker.Phone.PhoneNumber("0#0########"),
                    PhoneNumberConfirmed = true
                };
                _context.Users.Add(dbUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("Seeding User " + dbUser.UserName);

                var userRole = new IdentityUserRole<Guid>()
                {
                    RoleId = role.Id,
                    UserId = dbUser.Id
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            return dbUser;
        }
    }
}