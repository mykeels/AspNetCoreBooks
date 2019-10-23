using System;
using CommandLine;

namespace Books
{
    public class CommandOptions
    {
        [Option("environment", Required = false, HelpText = "A replacement for ASPNETCORE_ENVIRONMENT")]
        public string Environment { get; set; }

        [Option("urls", Required = false, HelpText = "A replacement for ASPNETCORE_URLS")]
        public string Urls { get; set; }
    }
}
