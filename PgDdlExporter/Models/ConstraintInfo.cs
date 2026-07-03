namespace PgDdlExporter.Models
{
    public enum ConstraintType
    {
        PrimaryKey,
        ForeignKey,
        Unique,
        Check,
        Other
    }

    public class ConstraintInfo : DatabaseObject
    {
        public string TableName { get; set; } = string.Empty;
        public ConstraintType Type { get; set; }
        public string Definition { get; set; } = string.Empty; // pg_get_constraintdef() çýktýsý
    }
}