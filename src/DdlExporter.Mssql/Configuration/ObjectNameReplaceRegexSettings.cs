using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdlExporter.Mssql.Configuration
{
    public class ObjectNameReplaceRegexSettings
    {
        public string Pattern { get; set; } = "[:*\\[\\]\\/]";
        public string Replacement { get; set; } = string.Empty;
    }
}
