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
    public class BookSeeder: DatabaseSeeder
    {
        public BookSeeder(IServiceProvider serviceProvider): base(serviceProvider) 
        {
        }

        public new void Run()
        {
            throw new NotImplementedException();
        }

        public async Task<Book> Run(IdentityUser<Guid> user)
        {
            string bookName = _faker.Commerce.Product();
            var dbBook = await _context.Books.FirstOrDefaultAsync(b => b.Name == bookName);
            if (dbBook == null)
            {
                dbBook = new Book()
                {
                    UserId = user.Id,
                    Name = bookName
                };
                _context.Books.Add(dbBook);
                await _context.SaveChangesAsync();
                Console.WriteLine("Seeding Book " + dbBook.Name);
            }
            return dbBook;
        }
    }
}