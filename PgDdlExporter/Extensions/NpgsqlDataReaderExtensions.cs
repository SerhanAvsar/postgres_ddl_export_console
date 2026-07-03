using Npgsql;

namespace PgDdlExporter.Extensions
{
    public static class NpgsqlDataReaderExtensions
    {
        public static string GetStringOrDefault(this NpgsqlDataReader reader, string column, string defaultValue = "")
        {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
        }

        public static string? GetStringOrNull(this NpgsqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public static bool GetBoolOrDefault(this NpgsqlDataReader reader, string column, bool defaultValue = false)
        {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetBoolean(ordinal);
        }

        public static int GetIntOrDefault(this NpgsqlDataReader reader, string column, int defaultValue = 0)
        {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
        }
    }
}