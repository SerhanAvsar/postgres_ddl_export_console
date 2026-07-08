namespace DdlExporter.Mssql.Configuration
{
    public class ProcessorSettings
    {
        public bool ScriptSchemas { get; set; } = true;
        public bool ScriptTables { get; set; } = true;
        public bool ScriptViews { get; set; } = true;
        public bool ScriptStoredProcedures { get; set; } = true;
        public bool ScriptTriggers { get; set; } = true;
        public bool ScriptFunctions { get; set; } = true;
        public bool ScriptPartitionInfos { get; set; } = true;
        public bool ScriptJobs { get; set; } = true;
        public bool ScriptFulltextCatalogs { get; set; } = true;
        public bool ScriptServiceBroker { get; set; } = true;
        public bool ScriptUserDefinedTypes { get; set; } = true;
        public ObjectNameReplaceRegexSettings ObjectNameReplaceRegexSettings { get; set; } = new ObjectNameReplaceRegexSettings();
    }
}