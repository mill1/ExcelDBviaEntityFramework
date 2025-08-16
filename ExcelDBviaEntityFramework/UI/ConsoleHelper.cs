namespace ExcelDBviaEntityFramework.UI
{
    public static class ConsoleHelpers
    {
        public static void WriteLineColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
    }
}
