using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Books.Data;
using Books.Models;

namespace Books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BooksDbContext _context;
        private readonly UserManager<IdentityUser<Guid>> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthController(
            BooksDbContext context,
            UserManager<IdentityUser<Guid>> userManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> GetIdentity()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) throw new KeyNotFoundException("User not found");

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(
                new
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Email = user.Email,
                    Roles = roles
                }
            );
        }

        /// <summary>
        /// Get token for user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<LoginResultModel>> CreateToken([FromBody]LoginInputModel model)
        {
            var user = await _context.Users
                                    .FirstOrDefaultAsync(u => u.UserName == model.Username);

            bool isCredentialsValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (isCredentialsValid)
            {
                var result = new LoginResultModel();

                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, model.Username)
                    };

                var accessTokenClaims = new List<Claim>(claims) {
                        new Claim("type", "login")
                    };

                var refreshTokenClaims = new List<Claim>(claims) {
                        new Claim("type", "refresh")
                    };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                string secret = _configuration.GetSection("Jwt")["Secret"];
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                string hostUrl = _httpContextAccessor.HttpContext.Request.Host.Value;

                var accessToken = new JwtSecurityToken(
                    issuer: hostUrl,
                    audience: hostUrl,
                    claims: accessTokenClaims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                var refreshToken = new JwtSecurityToken(
                    issuer: hostUrl,
                    audience: hostUrl,
                    claims: refreshTokenClaims,
                    expires: DateTime.Now.AddDays(7),
                    signingCredentials: credentials
                );

                return new LoginResultModel()
                {
                    RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken),
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    User = new UserOutputModel()
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        Id = user.Id
                    }
                };
            }
            return BadRequest(new
            {
                Message = "Could not verify credentials"
            });
        }
    }
}