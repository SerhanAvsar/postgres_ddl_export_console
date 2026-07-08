using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdlExporter.Common.Configuration
{
    public class JsonConfigurationReader : IConfigurationReader
    {
        private readonly string jsonString;

        public JsonConfigurationReader(string jsonString)
        {
            this.jsonString = jsonString;
        }

        public T Read<T>() where T : ConfigurationSettings
        {
            var settings = JsonConvert.DeserializeObject<T>(jsonString);
            return settings;
        }
    }
}
