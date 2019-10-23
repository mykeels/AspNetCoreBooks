using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Books.Services
{
    public class TokenService {

        private readonly IHttpContextAccessor _context;
        private readonly IConfiguration _configuration;

        private string _secret {
            get {
                return _configuration.GetSection("Jwt")["Secret"];
            }
        }
        private string _issuer {
            get {
                return _configuration.GetSection("Jwt")["Issuer"] ?? _context.HttpContext.Request.Host.Value;
            }
        }
        private string _audience {
            get {
                return _configuration.GetSection("Jwt")["Audience"] ?? _context.HttpContext.Request.Host.Value;
            }
        }
        private string[] _audiences {
            get {
                return (_configuration.GetSection("Jwt")["Audiences"] ?? "")
                                    .Split(new string[] { "," }, StringSplitOptions.None)
                                    .Concat(
                                        (new string[] { _audience }).AsEnumerable()
                                    ).ToArray();
            }
        }

        public TokenService(IHttpContextAccessor context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        public string CreateToken(List<Claim> claims) => CreateToken(claims, DateTime.Now.AddDays(1));

        public string CreateToken(List<Claim> claims, DateTime expires, string secret = null) {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret ?? _secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token, string secret = null) {
            var validationParameters = new TokenValidationParameters()
            {
                ValidIssuer = _issuer,
                ValidAudiences = _audiences,
                IssuerSigningKeys = new[] {
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret ?? _secret)
                    )
                },
                ValidateIssuer = false
            };
            SecurityToken validatedToken;
            ClaimsPrincipal UserPrincipal;
            var handler = new JwtSecurityTokenHandler();

            UserPrincipal = handler.ValidateToken(token, validationParameters, out validatedToken);

            return UserPrincipal;
        }
    }

    public class Tokens {
        public const string EmailConfirmation = "EmailConfirmation";
    }

    public class TokenProviders
    {
        public const string Default = "Default";
    }
}