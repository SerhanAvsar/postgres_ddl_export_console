using System.Text;
using PgDdlExporter.Models;

namespace PgDdlExporter.DdlBuilders
{
    // Not: Gerçek projede identity/serial, primary key gibi detaylar
    // ayrýca pg_get_constraintdef ile eklenmeli. Burada temel iskelet var.
    public class TableDdlBuilder : IDdlBuilder<TableInfo>
    {
        public string Build(TableInfo table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"-- Table: {table.QualifiedName}");
            sb.AppendLine($"CREATE TABLE {table.QualifiedName} (");

            var columnLines = table.Columns
                .OrderBy(c => c.OrdinalPosition)
                .Select(c => BuildColumnLine(c))
                .ToList();

            sb.AppendLine(string.Join(",\n", columnLines));
            sb.AppendLine(");");

            return sb.ToString();
        }

        private string BuildColumnLine(ColumnInfo column)
        {
            var nullability = column.IsNullable ? "NULL" : "NOT NULL";
            var defaultClause = string.IsNullOrEmpty(column.DefaultValue)
                ? string.Empty
                : $" DEFAULT {column.DefaultValue}";

            return $"    {column.Name} {column.DataType} {nullability}{defaultClause}";
        }
    }
}