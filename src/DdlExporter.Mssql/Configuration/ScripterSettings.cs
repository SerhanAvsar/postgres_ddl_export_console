using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdlExporter.Mssql.Configuration
{
    public class ScripterSettings
    {
        public static ScripterSettings Default { get; private set; } = new ScripterSettings()
        {
            ScriptDrops = false,
            WithDependencies = false,
            Indexes = true,
            DriAllConstraints = true,
            DriAll = false,
            ScriptSchema = true,
            ScriptData = false,
            IncludeHeaders = false,
            Triggers = true,
            FullTextIndexes = true,
            /*
                scrp.Options.ScriptDrops = false;  
                scrp.Options.WithDependencies = true;  
                scrp.Options.Indexes = true;   // To include indexes  
                scrp.Options.DriAllConstraints = true;   // to include referential constraints in the script 
            */
        };
        public bool ScriptDrops { get; set; }
        public bool WithDependencies { get; set; }
        public bool Indexes { get; set; }
        public bool DriAllConstraints { get; set; }
        public bool DriAll { get; set; }
        public bool ScriptSchema { get; set; }
        public bool ScriptData { get; set; }
        public bool IncludeHeaders { get; set; }
        public bool Triggers { get; set; }
        public bool FullTextIndexes { get; set; }
    }
}
