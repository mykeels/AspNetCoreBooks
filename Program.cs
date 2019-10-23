using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Books.Data;
using Books.Extensions;
using CommandLine;

namespace Books
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HandleEnvLoad(args);
            
            var host = CreateHostBuilder(args).Build();
            var env = host.Services.GetRequiredService<IHostEnvironment>();
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

            lifetime.ApplicationStarted.Register(() => {
                Console.WriteLine("Application started");
            });

            args = HandleDbSeed(args, host, env.IsTest());
            args = HandleDbMigration(args, host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static string[] HandleDbSeed(string[] args, IHost host, bool isTestEnv = false)
        {
            var isSeed = args.Any(x => x == "/seed");
            if (isSeed || isTestEnv || DbTypes.IsInMemory())
            {
                Console.WriteLine("Seeding");
                new DatabaseSeeder(host.Services).Run().Wait();
                if (isSeed) Environment.Exit(0);
            }

            return args.Except(new[] { "/seed" }).ToArray();
        }

        public static string[] HandleDbMigration(string[] args, IHost host)
        {
            var isMigration = args.Any(x => x == "/migrate");
            if (isMigration)
            {
                Console.WriteLine("Migrating");
                new DatabaseSeeder(host.Services).Migrate().Wait();
                Environment.Exit(0);
            }

            return args.Except(new[] { "/migrate" }).ToArray();
        }

        public static void HandleEnvLoad(string[] args)
        {
            Parser.Default.ParseArguments<CommandOptions>(args)
                .WithParsed<CommandOptions>(o =>
                {
                    if (!String.IsNullOrEmpty(o.Environment)) {
                        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", o.Environment);
                    }

                    if (!String.IsNullOrEmpty(o.Urls)) {
                        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", o.Urls);
                    }
                });

            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!String.Equals(envName, "test", StringComparison.InvariantCultureIgnoreCase))
            {
                System.Diagnostics.Debug.Assert(System.IO.File.Exists(".env"), ".env file is missing");
                DotNetEnv.Env.Load();
            }
        }
    }
}
