using System;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using Books.Data;
using Books.Providers;
using Books.Services;
using Books.Extensions;

namespace Books
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public IConfiguration _configuration { get; }
        public IHostEnvironment _environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (DbTypes.IsInMemory())
            {
                services.AddDbContext<BooksDbContext>(options =>
                {
                    options.UseInMemoryDatabase("books");
                });
            }
            else if (DbTypes.IsMySql())
            {
                services.AddDbContext<BooksDbContext>(options =>
                {
                    options.UseMySql(connectionString);
                });
            }
            else if (DbTypes.IsSqlite())
            {
                services.AddDbContext<BooksDbContext>(options =>
                {
                    options.UseSqlite(connectionString);
                });
            }
            else
            {
                services.AddDbContext<BooksDbContext>(options =>
                {
                    options.UseSqlServer(connectionString);
                });
            }

            services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<BooksDbContext>()
                .AddTokenProvider<JwtTokenProvider>(TokenProviders.Default)
                .AddUserManager<UserManager<IdentityUser<Guid>>>()
                .AddRoleManager<RoleManager<IdentityRole<Guid>>>();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.Name = TokenProviders.Default;
                options.TokenLifespan = TimeSpan.FromDays(5);
            });

            services.AddTransient<TokenService>();

            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));


            services.AddControllers()
                .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    })
                .AddMvcOptions(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.RespectBrowserAcceptHeader = true;
                });

            services.AddLogging();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                var jwtSection = _configuration.GetSection("Jwt");
                Debug.Assert(jwtSection.Exists(), "Jwt section missing in appsettings");
                string secret = _configuration.GetSection("Jwt")["Secret"];
                Debug.Assert(!String.IsNullOrEmpty(secret), "Jwt secret missing in appsettings");
                string audience = _configuration.GetSection("Jwt")["Audience"];
                Debug.Assert(!String.IsNullOrEmpty(audience), "Jwt audience missing in appsettings");
                string authority = _configuration.GetSection("Jwt")["Authority"];
                var key = Encoding.UTF8.GetBytes(secret);

                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = _environment.IsTestOrDevelopment();
                x.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var message = _environment.IsTestOrDevelopment() ?
                                        context.Exception.ToString() :
                                        "An error occurred processing your authentication.";
                        var result = Newtonsoft.Json.JsonConvert.SerializeObject(new { message });
                        return HttpResponseWritingExtensions.WriteAsync(context.Response, result);
                    }
                };
                x.RequireHttpsMetadata = !_environment.IsTestOrDevelopment();
                x.SaveToken = true;
                x.Audience = audience;
                if (String.IsNullOrEmpty(authority))
                {
                    // use local validation
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                }
                else
                {
                    // use sso validation
                    x.Authority = authority;
                }
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Books API",
                    Description = "API for Books",
                    TermsOfService = new Uri("http://tempuri.org/terms"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Mykeels.",
                        Email = "mykehell123@gmail.com",
                        Url = new Uri("https://twitter.com/mykeels")
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
