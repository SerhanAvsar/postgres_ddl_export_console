using PgDdlExporter.Config;
using PgDdlExporter.Exporters;
using PgDdlExporter.Helpers;

namespace PgDdlExporter.Services
{
    public class ExportManager
    {
        private readonly IEnumerable<IExporter> _exporters;
        private readonly ExportConfig _exportConfig;
        private readonly ILogger _logger;

        public ExportManager(IEnumerable<IExporter> exporters, ExportConfig exportConfig, ILogger logger)
        {
            _exporters = exporters;
            _exportConfig = exportConfig;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            var orderedExporters = _exporters.OrderBy(e => e.ExportOrder).ToList();

            foreach (var schema in _exportConfig.Schemas)
            {
                _logger.Info($"--- Schema export baþlýyor: {schema} ---");

                foreach (var exporter in orderedExporters)
                {
                    _logger.Info($"[{exporter.ObjectTypeName}] export ediliyor...");
                    await exporter.ExportAsync(schema, _exportConfig.OutputPath);
                }
            }

            _logger.Info("Export tamamlandý.");
        }
    }
}