namespace ExcelDBviaEntityFramework.UI
{
    public static class ConsoleHelper
    {
        public static void WriteLineColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }

        public static string GetUserInput(string prompt = null)
        {
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                WriteLineColored(prompt, ConsoleColor.Cyan);
            }

            Console.ForegroundColor = ConsoleColor.White;

            return Console.ReadLine()?.Trim() ?? string.Empty;
        }
    }
}
