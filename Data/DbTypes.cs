using System;

namespace Books.Data
{
    public class DbTypes
    {
        public const string DB_TYPE = "DB_TYPE";
        public const string MY_SQL = "MySql";
        public const string MS_SQL = "MsSql";
        public const string POSTGRES = "Postgres";
        public const string PSQL = "Psql";
        public const string IN_MEMORY = "InMemory";
        public const string SQLITE = "Sqlite";

        public static string Name {
            get {
                return Environment.GetEnvironmentVariable(DB_TYPE);
            }
        }

        public static bool IsMySql() {
            
            return String.Equals(Name, MY_SQL, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSqlServer() {
            
            return String.Equals(Name, MS_SQL, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsSqlite() {
            
            return String.Equals(Name, SQLITE, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsInMemory() {
            
            return String.Equals(Name, IN_MEMORY, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsPostgres() {
            
            return String.Equals(Name, POSTGRES, StringComparison.InvariantCultureIgnoreCase) || 
                    String.Equals(Name, PSQL, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}