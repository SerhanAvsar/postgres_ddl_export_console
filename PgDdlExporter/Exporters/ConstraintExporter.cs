using Npgsql;
using PgDdlExporter.DdlBuilders;
using PgDdlExporter.Helpers;
using PgDdlExporter.Queries;
using PgDdlExporter.Services;

namespace PgDdlExporter.Exporters
{
    public class ConstraintExporter : IExporter
    {
        private readonly TableQueries _tableQueries;
        private readonly ConstraintQueries _constraintQueries;
        private readonly ConstraintDdlBuilder _ddlBuilder;
        private readonly FileService _fileService;
        private readonly ILogger _logger;

        public string ObjectTypeName => "Constraints";
        public int ExportOrder => 4; // Table(3) -> Constraint(4) -> Index(5) -> ...

        public ConstraintExporter(
            NpgsqlConnection connection,
            ConstraintDdlBuilder ddlBuilder,
            FileService fileService,
            ILogger logger)
        {
            _tableQueries = new TableQueries(connection);
            _constraintQueries = new ConstraintQueries(connection);
            _ddlBuilder = ddlBuilder;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task ExportAsync(string schema, string outputBasePath)
        {
            var folder = PathHelper.GetObjectFolder(outputBasePath, $"04_{ObjectTypeName}");
            var tables = await _tableQueries.GetTablesAsync(schema);

            foreach (var table in tables)
            {
                var constraints = await _constraintQueries.GetConstraintsAsync(schema, table.Name);

                if (constraints.Count == 0)
                {
                    _logger.Info($"{table.QualifiedName} için constraint bulunamadý, atlandý.");
                    continue;
                }

                var ddl = _ddlBuilder.BuildMany(constraints);
                var filePath = PathHelper.BuildFilePath(folder, schema, table.Name);

                await _fileService.WriteDdlFileAsync(filePath, ddl);
                _logger.Info($"{constraints.Count} constraint export edildi: {table.QualifiedName}");
            }
        }
    }
}