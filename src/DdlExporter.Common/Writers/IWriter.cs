namespace DdlExporter.Common.Writers
{
    public interface IWriter
    {
        void Start(string databaseName, string objectType, string objectName);
        void WriteLine(string content);
        void Finish();
    }
}