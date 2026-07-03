using Npgsql;
using PgDdlExporter.DdlBuilders;
using PgDdlExporter.Helpers;
using PgDdlExporter.Queries;
using PgDdlExporter.Services;

namespace PgDdlExporter.Exporters
{
	public class TableExporter : IExporter
	{
		private readonly TableQueries _tableQueries;
		private readonly ColumnQueries _columnQueries;
		private readonly TableDdlBuilder _ddlBuilder;
		private readonly FileService _fileService;
		private readonly ILogger _logger;

		public string ObjectTypeName => "Tables";
		public int ExportOrder => 3; // Schema(0) -> Type(1) -> Sequence(2) -> Table(3) -> ...

		public TableExporter(
			NpgsqlConnection connection,
			TableDdlBuilder ddlBuilder,
			FileService fileService,
			ILogger logger)
		{
			_tableQueries = new TableQueries(connection);
			_columnQueries = new ColumnQueries(connection);
			_ddlBuilder = ddlBuilder;
			_fileService = fileService;
			_logger = logger;
		}

		public async Task ExportAsync(string schema, string outputBasePath)
		{
			var folder = PathHelper.GetObjectFolder(outputBasePath, $"03_{ObjectTypeName}");
			var tables = await _tableQueries.GetTablesAsync(schema);

			_logger.Info($"{tables.Count} tablo bulundu (schema: {schema})");

			foreach (var table in tables)
			{
				table.Columns = await _columnQueries.GetColumnsAsync(schema, table.Name);

				var ddl = _ddlBuilder.Build(table);
				var filePath = PathHelper.BuildFilePath(folder, schema, table.Name);

				await _fileService.WriteDdlFileAsync(filePath, ddl);
			}
		}
	}
}