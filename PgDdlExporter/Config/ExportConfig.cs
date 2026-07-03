namespace PgDdlExporter.Config
{
    public class ExportConfig
    {
        public string OutputPath { get; set; } = "ExportedDDL";
        public List<string> Schemas { get; set; } = new() { "public" };
        public bool OverwriteExisting { get; set; } = true;
    }
}