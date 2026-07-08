using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdlExporter.Mssql.Configuration
{
    public static class ScriptingOptionsExtensions
    {
        public static void Apply(this ScriptingOptions scriptingOptions, ScripterSettings settings)
        {
            scriptingOptions.ScriptDrops = settings.ScriptDrops;
            scriptingOptions.WithDependencies = settings.WithDependencies;
            scriptingOptions.Indexes = settings.Indexes;
            scriptingOptions.DriAllConstraints = settings.DriAllConstraints;
            scriptingOptions.DriAll = settings.DriAll;
            scriptingOptions.ScriptSchema = settings.ScriptSchema;
            scriptingOptions.ScriptData = settings.ScriptData;
            scriptingOptions.IncludeHeaders = settings.IncludeHeaders;
            scriptingOptions.Triggers = settings.Triggers;
            scriptingOptions.FullTextIndexes = settings.FullTextIndexes;
        }
    }
}
