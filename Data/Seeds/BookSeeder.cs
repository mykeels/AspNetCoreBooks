using System;
using Bogus;
using Books.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Books.Data
{
    public class BookSeeder: DatabaseSeeder
    {
        public BookSeeder(IServiceProvider serviceProvider): base(serviceProvider) 
        {
        }

        public override async Task Run()
        {
            throw new NotImplementedException();
        }

        public async Task<Book> Run(IdentityUser<Guid> user)
        {
            var book = new Book()
            {
                UserId = user.Id,
                Name = _faker.Commerce.Product()
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }
    }
}