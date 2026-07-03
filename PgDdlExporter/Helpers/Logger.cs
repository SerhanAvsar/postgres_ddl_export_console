namespace PgDdlExporter.Helpers
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception? ex = null);
    }

    public class ConsoleLogger : ILogger
    {
        public void Info(string message) => Write("INFO ", message, ConsoleColor.Gray);
        public void Warn(string message) => Write("WARN ", message, ConsoleColor.Yellow);
        public void Error(string message, Exception? ex = null)
        {
            Write("ERROR", message, ConsoleColor.Red);
            if (ex != null)
                Write("ERROR", ex.ToString(), ConsoleColor.Red);
        }

        private void Write(string level, string message, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
            Console.ForegroundColor = prev;
        }
    }
}