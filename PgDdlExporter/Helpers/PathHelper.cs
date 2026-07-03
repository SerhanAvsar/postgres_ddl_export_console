namespace PgDdlExporter.Helpers
{
    public static class PathHelper
    {
        public static string GetObjectFolder(string basePath, string subFolder)
        {
            var path = Path.Combine(basePath, subFolder);
            Directory.CreateDirectory(path);
            return path;
        }

        public static string SanitizeFileName(string name) =>
            string.Join("_", name.Split(Path.GetInvalidFileNameChars()));

        public static string BuildFilePath(string folder, string schema, string objectName) =>
            Path.Combine(folder, $"{SanitizeFileName(schema)}.{SanitizeFileName(objectName)}.sql");
    }
}