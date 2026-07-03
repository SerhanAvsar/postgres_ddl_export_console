namespace PgDdlExporter.Models
{
    public class TableInfo : DatabaseObject
    {
        public List<ColumnInfo> Columns { get; set; } = new();
    }
}