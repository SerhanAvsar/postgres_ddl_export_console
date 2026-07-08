using Azure.Core;
using DdlExporter.Common;
using DdlExporter.Common.Configuration;
using DdlExporter.Common.Loggers;
using DdlExporter.Common.Writers;
using DdlExporter.Mssql.Configuration;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DdlExporter.Mssql
{
    public class MssqlScripter : IScripter
    {
        public const string DATABASE_TYPE = "MSSQL";

        private MssqlConfigurationSettings _settings;
        private readonly IWriter _writer;
        private readonly ILogger _logger;

        public MssqlScripter(IConfigurationReader configurationReader, IWriter writer, ILogger logger)
        {
            _settings = configurationReader.Read<MssqlConfigurationSettings>();
            _writer = writer;
            _logger = logger;
        }

        //public MssqlScripter(MssqlConfigurationSettings settings, IWriter writer)
        //{
        //    _settings = settings;
        //    _writer = writer;
        //}

        public void Execute()
        {
            _logger.Log("Execute");
            var connection = new Microsoft.SqlServer.Management.Common.ServerConnection(_settings.ServerHost, _settings.Username, _settings.Password);

            // Connect to the local, default instance of SQL Server.   
            Server srv = new Server(connection);
            Scripter scrp = new Scripter(srv);
            _logger.Log("Script begin");

            ScriptDatabases(srv, scrp);

            ScriptJobs(srv, scrp);

        }

        private void ScriptDatabases(Server srv, Scripter scrp)
        {
            var databaseName = _settings.DatabaseName;
            if (databaseName == null)
            {
                ScriptDatabases(scrp, srv);
            }
            else
            {
                Database db = srv.Databases[databaseName];
                ScriptDatabase(scrp, db);
            }
        }

        private void ScriptDatabases(Scripter scrp, Server srv)
        {
            foreach (Database db in srv.Databases)
            {
                if (db.IsSystemObject || !db.IsAccessible)
                    continue;
                ScriptDatabase(scrp, db);
            }
        }

        private void ScriptDatabase(Scripter scrp, Database db)
        {
            scrp.Options.Apply(_settings.ScripterSettings);

            if (_settings.ProcessorSettings.ScriptSchemas)
            {
                Scribe(scrp, db.Schemas, db.Name, "SCHEMA");
            }

            if (_settings.ProcessorSettings.ScriptTables)
            {
                if (_settings.ProcessorSettings.ScriptTriggers)
                    scrp.Options.Triggers = true;//trigger collection boş geliyor. bunun için table ile birlikte bağlı triggerlar yazılsın deniyor

                Scribe(scrp, db.Tables, db.Name, "TABLE");
                scrp.Options.Apply(_settings.ScripterSettings);
            }

            if (_settings.ProcessorSettings.ScriptViews)
            {
                Scribe(scrp, db.Views, db.Name, "VIEW");
            }

            if (_settings.ProcessorSettings.ScriptStoredProcedures)
            {
                Scribe(scrp, db.StoredProcedures, db.Name, "STORED_PROCEDURE");
            }

            if (_settings.ProcessorSettings.ScriptFunctions)
            {
                Scribe(scrp, db.UserDefinedFunctions, db.Name, "FUNCTIONS");
            }

            if (_settings.ProcessorSettings.ScriptPartitionInfos)
            {
                Scribe(scrp, db.PartitionFunctions, db.Name, "PARTITION_FUNCTIONS");
                Scribe(scrp, db.PartitionSchemes, db.Name, "PARTITION_SCHEMES");
            }

            if (_settings.ProcessorSettings.ScriptFulltextCatalogs)
            {
                Scribe(scrp, db.FullTextCatalogs, db.Name, "FULL_TEXT_CATALOGS");
            }

            if (_settings.ProcessorSettings.ScriptServiceBroker)
            {
                Scribe(scrp, db.ServiceBroker.MessageTypes, db.Name, "SERVICE_BROKER_MESSAGE_TYPES");
                Scribe(scrp, db.ServiceBroker.ServiceContracts, db.Name, "SERVICE_BROKER_CONTRACTS");
                Scribe(scrp, db.ServiceBroker.Queues, db.Name, "SERVICE_BROKER_QUEUES");
                Scribe(scrp, db.ServiceBroker.Services, db.Name, "SERVICE_BROKER_SERVICES");
                Scribe(scrp, db.ServiceBroker.Routes, db.Name, "SERVICE_BROKER_ROUTES");
            }

            if (_settings.ProcessorSettings.ScriptUserDefinedTypes)
            {
                Scribe(scrp, db.UserDefinedAggregates, db.Name, "USER_DEFINED_AGGREGATES");
                Scribe(scrp, db.UserDefinedDataTypes, db.Name, "USER_DEFINED_DATA_TYPES");
                Scribe(scrp, db.UserDefinedTableTypes, db.Name, "USER_DEFINED_TABLE_TYPES");
                Scribe(scrp, db.UserDefinedTypes, db.Name, "USER_DEFINED_TYPES");
            }
            //trigger collection boş geliyor. bunun için table ile birlikte bağlı triggerlar yazılsın deniyor
            //if (_settings.ProcessorSettings.ScriptTriggers)
            //{
            //    Scribe(scrp, db.Triggers);
            //}
        }

        private void ScriptJobs(Server srv, Scripter scrp)
        {
            if (_settings.ProcessorSettings.ScriptJobs)
            {
                foreach (SqlSmoObject job in srv.JobServer.Jobs)
                {
                    Scribe(scrp, job, "SQLServerAgent", "JOBS");
                }
            }
        }

        private void Scribe(Scripter scrp, SmoCollectionBase smoCollection, string groupName, string objectType)
        {
            foreach (SqlSmoObject smo in smoCollection)
            {
                Scribe(scrp, smo, groupName, objectType);
            }
        }

        private void Scribe(Scripter scrp, SqlSmoObject smo, string groupName, string objectType)
        {
            if (IsSystemObject(smo))
                return;
            try
            {
                _writer.Start(groupName, objectType, BuildObjectName(smo));
                System.Collections.Specialized.StringCollection sc = scrp.Script(new Urn[] { smo.Urn });
                foreach (string st in sc)
                {
                    //Console.WriteLine(st);
                    _writer.WriteLine(st);
                }
                _writer.Finish();
            }
            catch (Exception ex)
            {
                _logger.Log($"Error {objectType} {smo.ToString()}: {ex.Message}");
            }
        }

        private string BuildObjectName(SqlSmoObject smo)
        {
            var rawObjectName = smo.ToString();
            try
            {
                var regexSettings = _settings.ProcessorSettings?.ObjectNameReplaceRegexSettings;
                if (regexSettings != null)
                    return Regex.Replace(rawObjectName, regexSettings.Pattern, regexSettings.Replacement);
            }
            catch (Exception) { }
            return rawObjectName.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        private bool IsSystemObject(SqlSmoObject smo)
        {
            PropertyInfo prop = smo.GetType().GetProperty("IsSystemObject");
            if (prop == null)
                return false;

            object result = prop.GetValue(smo);
            return (bool)result;
        }
    }
}
