using DdlExporter.Common;
using DdlExporter.Common.Configuration;
using DdlExporter.Common.Loggers;
using DdlExporter.Common.Writers;
using DdlExporter.Postgresql.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DdlExporter.Postgresql
{
    public class PostgresqlScripter : IScripter
    {
        public const string DATABASE_TYPE = "POSTGRESQL";

        private readonly PostgresqlConfigurationSettings _settings;
        private readonly IWriter _writer;
        private readonly ILogger _logger;

        private static readonly Regex HeaderRegex = new Regex(
            @"^--\s+Name:\s*(?<name>[^;]+);\s*Type:\s*(?<type>[^;]+);\s*Schema:\s*(?<schema>[^;]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public PostgresqlScripter(IConfigurationReader configurationReader, IWriter writer, ILogger logger)
        {
            _settings = configurationReader.Read<PostgresqlConfigurationSettings>();
            _writer = writer;
            _logger = logger;
        }

        public void Execute()
        {
            _logger.Log("PostgreSQL DDL Dýþa Aktarma Ýþlemi Baþladý...");

            var pgDumpExe = string.IsNullOrWhiteSpace(_settings.PgDumpPath) ? "pg_dump" : _settings.PgDumpPath;
            var arguments = $"--host=\"{_settings.ServerHost}\" --port={_settings.Port} --username=\"{_settings.Username}\" --dbname=\"{_settings.DatabaseName}\" --schema-only --schema=\"{_settings.Schema}\"";

            _logger.Log($"pg_dump çalýþtýrýlýyor: {pgDumpExe} {arguments}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = pgDumpExe,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            if (!string.IsNullOrEmpty(_settings.Password))
            {
                processStartInfo.EnvironmentVariables["PGPASSWORD"] = _settings.Password;
            }

            using (var process = new Process { StartInfo = processStartInfo })
            {
                var errorBuilder = new StringBuilder();

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"pg_dump baþlatýlamadý. '{pgDumpExe}' aracýnýn sistem PATH deðiþkeninde tanýmlý olduðundan veya PgDumpPath deðerinin doðru ayarlandýðýndan emin olun. Hata: {ex.Message}", ex);
                }

                process.BeginErrorReadLine();

                using (var reader = process.StandardOutput)
                {
                    ParseAndWriteDump(reader);
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var errors = errorBuilder.ToString();
                    _logger.Log($"pg_dump hatasý (Exit Code: {process.ExitCode}): {errors}");
                    throw new ApplicationException($"pg_dump çalýþtýrýlýrken bir hata oluþtu. Exit Code: {process.ExitCode}. Hata Detayý: {errors}");
                }
            }

            _logger.Log("PostgreSQL DDL Dýþa Aktarma Ýþlemi Tamamlandý.");
        }

        private void ParseAndWriteDump(StreamReader reader)
        {
            string currentObjectName = "PRE_SCRIPT";
            string currentObjectType = "SETUP";
            var currentBlockLines = new List<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var match = HeaderRegex.Match(line);
                if (match.Success)
                {
                    FlushBlock(currentObjectType, currentObjectName, currentBlockLines);

                    currentObjectName = match.Groups["name"].Value.Trim();
                    currentObjectType = match.Groups["type"].Value.Trim();
                }
                else
                {
                    currentBlockLines.Add(line);
                }
            }

            FlushBlock(currentObjectType, currentObjectName, currentBlockLines);
        }

        private void FlushBlock(string type, string name, List<string> lines)
        {
            bool hasContent = false;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0 && !trimmed.StartsWith("--"))
                {
                    hasContent = true;
                    break;
                }
            }

            if (!hasContent)
            {
                lines.Clear();
                return;
            }

            _writer.Start(_settings.DatabaseName, type, name);
            foreach (var line in lines)
            {
                _writer.WriteLine(line);
            }
            _writer.Finish();

            _logger.Log($"Aktarýldý: [{type}] {name}");
            lines.Clear();
        }
    }
}