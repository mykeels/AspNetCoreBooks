using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;

namespace Books.Extensions 
{
    public static class EnvironmentExtensions 
    {
        public static string Test = "Test";
        public static string Production = "Production";
        public static string Development = "Development";
        public static string Sqlite = "Sqlite";

        #region "IHostEnvironment"
        public static bool IsEnvironment(this IHostEnvironment env, string name)
        {
            return String.Equals(env.EnvironmentName, name, StringComparison.CurrentCultureIgnoreCase);
        }
        public static bool IsTest(this IHostEnvironment env)
        {
            return env.IsEnvironment(Test);
        }
        public static bool IsSqlite(this IHostEnvironment env)
        {
            return env.IsEnvironment(Sqlite);
        }
        public static bool IsTestOrDevelopment(this IHostEnvironment env)
        {
            return env.IsAnyEnvironment(Development, Test, Sqlite);
        }
        public static bool IsAnyEnvironment(this IHostEnvironment env, params string[] envs)
        {
            return envs.ToList().Any(_env => env.IsEnvironment(_env));
        }
        #endregion
    }
}