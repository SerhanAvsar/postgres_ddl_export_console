using DdlExporter.Common.Configuration;
using DdlExporter.Common.Loggers;
using DdlExporter.Common.Writers;
using DdlExporter.Mssql;
using DdlExporter.Mssql.Configuration;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mssql_ddl_export_console
{
    internal class Program
    {
        private string[] args;
        private string databaseType;
        private string outputDir;
        private string settingsFile;

        public Program(string[] args)
        {
            this.args = args;
        }

        static void Main(string[] args)
        {
            try
            {
                var program = new Program(args);
                program.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Beklenmedik bir hata oluştu: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void Run()
        {
            if (ParseArgs() == false)
            {
                ShowHelp();
                return;
            }

            //var settingsFile = "testdb_Trevoo.json";//args[0];
            var settingsJson = File.ReadAllText(settingsFile);
            var logger = new ConsoleLogger();
            var scripter = ScripterBuilder.Get(this.databaseType/*MssqlScripter.DATABASE_TYPE*/)
                .AddConfigurationReader(new JsonConfigurationReader(settingsJson))
                .AddWriter(new FileWriter(outputDir/*"c:\\export"*/, logger))
                .AddLogger(logger)
                .Build();
            scripter.Execute();
            return;
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage: -db:MSSQL -od:<export directory> -s:<settings file>");
        }

        private bool ParseArgs()
        {
            if (args.Length != 3)
            {
                return false;
            }
            foreach (var arg in args)
            {
                if (arg.StartsWith("-db:"))
                    databaseType = arg.Replace("-db:", string.Empty).Trim();
                if (arg.StartsWith("-od:"))
                    outputDir = arg.Replace("-od:", string.Empty).Trim();
                if (arg.StartsWith("-s:"))
                    settingsFile = arg.Replace("-s:", string.Empty).Trim();
            }
            return true;
        }
        //private static void printDdl(Scripter scrp, SqlSmoObject tb)
        //{
        //    System.Collections.Specialized.StringCollection sc = scrp.Script(new Urn[] { tb.Urn });
        //    foreach (string st in sc)
        //    {
        //        Console.WriteLine(st);
        //    }
        //}
    }
}

