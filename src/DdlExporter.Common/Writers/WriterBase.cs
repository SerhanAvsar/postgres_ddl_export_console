using DdlExporter.Common.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdlExporter.Common.Writers
{
    public abstract class WriterBase : IWriter
    {
        protected ILogger Logger { get; }

        public WriterBase(ILogger logger)
        {
            Logger = logger;
        }

        public abstract void Start(string databaseName, string objectType, string objectName);
        public abstract void WriteLine(string content);
        public abstract void Finish();
    }
}
