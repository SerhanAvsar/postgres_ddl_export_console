using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PgDdlExporter.Config;
using PgDdlExporter.DdlBuilders;
using PgDdlExporter.Exporters;
using PgDdlExporter.Helpers;
using PgDdlExporter.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var dbConfig = configuration.GetSection("Database").Get<DatabaseConfig>()
    ?? throw new InvalidOperationException("Database config bulunamadı.");
var exportConfig = configuration.GetSection("Export").Get<ExportConfig>()
    ?? throw new InvalidOperationException("Export config bulunamadı.");

var services = new ServiceCollection();

services.AddSingleton<ILogger, ConsoleLogger>();
services.AddSingleton(dbConfig);
services.AddSingleton(exportConfig);

services.AddSingleton(sp =>
{
    var conn = new NpgsqlConnection(dbConfig.ToConnectionString());
    conn.Open();
    return conn;
});

services.AddSingleton(sp => new FileService(sp.GetRequiredService<ILogger>(), exportConfig.OverwriteExisting));

services.AddSingleton<TableDdlBuilder>();
services.AddSingleton<IExporter, TableExporter>();
services.AddSingleton<ConstraintDdlBuilder>();
services.AddSingleton<IExporter, ConstraintExporter>();
// services.AddSingleton<IExporter, ViewExporter>();

services.AddSingleton<ExportManager>();

var provider = services.BuildServiceProvider();

try
{
    var manager = provider.GetRequiredService<ExportManager>();
    await manager.RunAsync();
}
catch (Exception ex)
{
    var logger = provider.GetRequiredService<ILogger>();
    logger.Error("Export sırasında hata oluştu.", ex);
}
finally
{
    var conn = provider.GetRequiredService<NpgsqlConnection>();
    await conn.DisposeAsync();
}