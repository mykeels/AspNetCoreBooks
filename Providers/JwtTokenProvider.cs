using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Books.Models;
using Books.Services;

namespace Books.Providers
{
    public class JwtTokenProvider: IUserTwoFactorTokenProvider<IdentityUser<Guid>>
    {
        private readonly TokenService _tokenService;

        public JwtTokenProvider(TokenService tokenService, IOptions<DataProtectionTokenProviderOptions> options)
        {
            _tokenService = tokenService;

            Options = options?.Value ?? new DataProtectionTokenProviderOptions();
        }

        public string Name { get { return Options.Name; } }

        protected DataProtectionTokenProviderOptions Options { get; private set; }

        public virtual Task<bool> CanGenerateTwoFactorTokenAsync(
            UserManager<IdentityUser<Guid>> manager, 
            IdentityUser<Guid> user)
        {
            return Task.FromResult(false);
        }

        public virtual Task<string> GenerateAsync(
            string purpose, 
            UserManager<IdentityUser<Guid>> manager, 
            IdentityUser<Guid> user)
        {
            return Task.Run(() => {
                return _tokenService.CreateToken(new List<Claim>() {
                    new Claim(Claims.Type, purpose),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                }, DateTime.UtcNow.Add(Options.TokenLifespan), user.SecurityStamp);
            });
        }

        public virtual Task<bool> ValidateAsync(
            string purpose, 
            string token, 
            UserManager<IdentityUser<Guid>> manager, 
            IdentityUser<Guid> user)
        {
            return Task.Run(() => {
                try {
                    var claims = _tokenService.ValidateToken(token, user.SecurityStamp);
                    string userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
                    string type = claims.FindFirstValue(Claims.Type);
                    return !String.IsNullOrEmpty(userId) && 
                            !String.IsNullOrEmpty(type) && 
                            type == purpose;
                }
                catch {
                    return false;
                }
            });
        }
    }
}