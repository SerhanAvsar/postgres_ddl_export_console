using Npgsql;
using PgDdlExporter.Extensions;
using PgDdlExporter.Models;

namespace PgDdlExporter.Queries
{
    public class ConstraintQueries
    {
        private readonly NpgsqlConnection _connection;

        public ConstraintQueries(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<ConstraintInfo>> GetConstraintsAsync(string schema, string tableName)
        {
            // contype: p=primary key, f=foreign key, u=unique, c=check, x=exclusion
            const string sql = @"
                SELECT
                    con.conname AS constraint_name,
                    con.contype AS constraint_type,
                    n.nspname AS schema_name,
                    cl.relname AS table_name,
                    r.rolname AS owner,
                    pg_get_constraintdef(con.oid) AS definition
                FROM pg_constraint con
                JOIN pg_class cl ON cl.oid = con.conrelid
                JOIN pg_namespace n ON n.oid = cl.relnamespace
                JOIN pg_roles r ON r.oid = cl.relowner
                WHERE n.nspname = @schema
                  AND cl.relname = @table
                ORDER BY con.contype, con.conname;";

            var result = new List<ConstraintInfo>();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            cmd.Parameters.AddWithValue("schema", schema);
            cmd.Parameters.AddWithValue("table", tableName);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new ConstraintInfo
                {
                    Name = reader.GetStringOrDefault("constraint_name"),
                    SchemaName = reader.GetStringOrDefault("schema_name"),
                    TableName = reader.GetStringOrDefault("table_name"),
                    Owner = reader.GetStringOrDefault("owner"),
                    Definition = reader.GetStringOrDefault("definition"),
                    Type = MapConstraintType(reader.GetStringOrDefault("constraint_type"))
                });
            }

            return result;
        }

        private static ConstraintType MapConstraintType(string contype) => contype switch
        {
            "p" => ConstraintType.PrimaryKey,
            "f" => ConstraintType.ForeignKey,
            "u" => ConstraintType.Unique,
            "c" => ConstraintType.Check,
            _ => ConstraintType.Other
        };
    }
}