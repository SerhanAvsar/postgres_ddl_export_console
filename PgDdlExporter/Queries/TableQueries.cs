using Npgsql;
using PgDdlExporter.Extensions;
using PgDdlExporter.Models;

namespace PgDdlExporter.Queries
{
    public class TableQueries
    {
        private readonly NpgsqlConnection _connection;

        public TableQueries(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<TableInfo>> GetTablesAsync(string schema)
        {
            const string sql = @"
                SELECT c.relname AS table_name, n.nspname AS schema_name, r.rolname AS owner
                FROM pg_class c
                JOIN pg_namespace n ON n.oid = c.relnamespace
                JOIN pg_roles r ON r.oid = c.relowner
                WHERE c.relkind = 'r' AND n.nspname = @schema
                ORDER BY c.relname;";

            var result = new List<TableInfo>();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("schema", schema);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new TableInfo
                {
                    Name = reader.GetStringOrDefault("table_name"),
                    SchemaName = reader.GetStringOrDefault("schema_name"),
                    Owner = reader.GetStringOrDefault("owner")
                });
            }

            return result;
        }
    }
}