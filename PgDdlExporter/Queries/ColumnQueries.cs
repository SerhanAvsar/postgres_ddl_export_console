using Npgsql;
using PgDdlExporter.Extensions;
using PgDdlExporter.Models;

namespace PgDdlExporter.Queries
{
    public class ColumnQueries
    {
        private readonly NpgsqlConnection _connection;

        public ColumnQueries(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<ColumnInfo>> GetColumnsAsync(string schema, string tableName)
        {
            const string sql = @"
                SELECT column_name, data_type, is_nullable, column_default, ordinal_position
                FROM information_schema.columns
                WHERE table_schema = @schema AND table_name = @table
                ORDER BY ordinal_position;";

            var result = new List<ColumnInfo>();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("schema", schema);
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ColumnInfo
                {
                    Name = reader.GetStringOrDefault("column_name"),
                    DataType = reader.GetStringOrDefault("data_type"),
                    IsNullable = reader.GetStringOrDefault("is_nullable") == "YES",
                    DefaultValue = reader.GetStringOrNull("column_default"),
                    OrdinalPosition = reader.GetIntOrDefault("ordinal_position")
                });
            }

            return result;
        }
    }
}