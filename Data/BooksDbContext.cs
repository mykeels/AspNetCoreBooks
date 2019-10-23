using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Books.Models;

namespace Books.Data
{
    public class BooksDbContext : IdentityDbContext<
                                    IdentityUser<Guid>,
                                    IdentityRole<Guid>,
                                    Guid,
                                    IdentityUserClaim<Guid>,
                                    IdentityUserRole<Guid>,
                                    IdentityUserLogin<Guid>,
                                    IdentityRoleClaim<Guid>,
                                    IdentityUserToken<Guid>
                                  >
    {
        private readonly DbContextOptions<BooksDbContext> _options;

        public DbSet<Book> Books { get; set; }

        public DbContextOptions<BooksDbContext> Options {
            get {
                return _options;
            }
        }
        
        public BooksDbContext(DbContextOptions<BooksDbContext> options) : base(options)
        {
            _options = options;
        }
    }
}