namespace PgDdlExporter.Exporters
{
    public interface IExporter
    {
        string ObjectTypeName { get; }
        int ExportOrder { get; }
        Task ExportAsync(string schema, string outputBasePath);
    }
}