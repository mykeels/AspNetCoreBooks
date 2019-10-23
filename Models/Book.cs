using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Books.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser<Guid> User { get; set; }
    }
}