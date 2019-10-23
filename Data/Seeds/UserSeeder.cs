using System;
using Bogus;
using Books.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

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
            var user = new IdentityUser<Guid>()
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = _faker.Phone.PhoneNumber("0#0########"),
                PhoneNumberConfirmed = true
            };
            Console.WriteLine("Seeding User " + user.UserName);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userRole = new IdentityUserRole<Guid>()
            {
                RoleId = role.Id,
                UserId = user.Id
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}