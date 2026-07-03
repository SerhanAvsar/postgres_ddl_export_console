namespace PgDdlExporter.Models
{
	public abstract class DatabaseObject
	{
		public string SchemaName { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Owner { get; set; } = string.Empty;

		public string QualifiedName => $"{SchemaName}.{Name}";
	}
}