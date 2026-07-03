namespace PgDdlExporter.Config
{
    public class DatabaseConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int CommandTimeoutSeconds { get; set; } = 30;

        public string ToConnectionString() =>
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};CommandTimeout={CommandTimeoutSeconds}";
    }
}