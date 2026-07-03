namespace PgDdlExporter.DdlBuilders
{
    public interface IDdlBuilder<in T>
    {
        string Build(T dbObject);
    }
}