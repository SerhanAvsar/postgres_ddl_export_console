using PgDdlExporter.Helpers;

namespace PgDdlExporter.Services
{
    public class FileService
    {
        private readonly ILogger _logger;
        private readonly bool _overwriteExisting;

        public FileService(ILogger logger, bool overwriteExisting)
        {
            _logger = logger;
            _overwriteExisting = overwriteExisting;
        }

        public async Task WriteDdlFileAsync(string filePath, string content)
        {
            if (File.Exists(filePath) && !_overwriteExisting)
            {
                _logger.Warn($"Dosya zaten var, atlandý: {filePath}");
                return;
            }

            await File.WriteAllTextAsync(filePath, content);
            _logger.Info($"Yazýldý: {filePath}");
        }
    }
}