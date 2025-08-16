namespace ExcelDBviaEntityFramework.Console
{
    public static class ConsoleHelper
    {
        public static void WriteLineColored(string message, ConsoleColor color)
        {
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(message);
        }

        public static string GetUserInput(string prompt = null)
        {
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                WriteLineColored(prompt, ConsoleColor.Cyan);
            }

            System.Console.ForegroundColor = ConsoleColor.White;

            return System.Console.ReadLine()?.Trim() ?? string.Empty;
        }
    }
}
