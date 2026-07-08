using DdlExporter.Common.Configuration;
using System.ComponentModel;

namespace DdlExporter.Mssql.Configuration
{
    public class MssqlConfigurationSettings : ConfigurationSettings
    {
        public string ServerHost { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ProcessorSettings ProcessorSettings { get; set; } = new ProcessorSettings();
        public ScripterSettings ScripterSettings { get; set; } = ScripterSettings.Default;
    }
}
