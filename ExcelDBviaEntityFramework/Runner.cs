namespace ExcelDBviaEntityFramework
{
    public class Runner
    {
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private bool _quit;
        private const int LineWidth = 40;

        private static class MenuOptions
        {
            public const string Calculate = "c";
            public const string Help = "h";
            public const string Quit = "q";
        }

        public Runner()
        {

            _menuItems =
            [
                new("Calculate rectangle area", MenuOptions.Calculate, CalculateRectangleArea),
                new("Help", MenuOptions.Help, ShowHelp),
                new("Quit", MenuOptions.Quit, () => _quit = true)
            ];

            _menuOptions = _menuItems.ToDictionary(m => m.Key, m => m.Action, StringComparer.OrdinalIgnoreCase);
        }

        private void CalculateRectangleArea()
        {

            int length = GetValidInteger("Enter length:");

            WriteLineColored($"The length is {length}", ConsoleColor.Cyan);
        }

        public void Run()
        {
            try
            {
                ExecuteUserInterface();
            }
            catch (Exception e)
            {
                WriteLineColored(e.ToString(), ConsoleColor.Red);
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private void ExecuteUserInterface()
        {
            DrawBanner(ConsoleColor.Green, GetAssemblyName(), '#');

            while (!_quit)
            {
                PrintMenuOptions();
                string option = GetUserInput();

                if (_menuOptions.TryGetValue(option, out var action))
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        WriteLineColored($"An error occurred: {ex.Message}", ConsoleColor.Red);
                    }
                }
                else
                {
                    WriteLineColored($"Invalid option: {option}", ConsoleColor.Magenta);
                }
            }
        }

        private void ShowHelp()
        {
            DrawBanner(ConsoleColor.Cyan, $"{GetAssemblyName()} help", '-');
            Console.WriteLine("Select an option by typing the associated letter and press Enter.");
        }

        private static int GetValidInteger(string prompt)
        {
            while (true)
            {
                string input = GetUserInput(prompt);
                if (int.TryParse(input, out int result))
                    return result;

                WriteLineColored("Invalid number. Please try again.", ConsoleColor.Magenta);
            }
        }

        private static string GetUserInput(string prompt = null)
        {
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                WriteLineColored(prompt, ConsoleColor.Yellow);
            }

            Console.ForegroundColor = ConsoleColor.White;
            return Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        private static void WriteLineColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }

        private static void DrawBanner(ConsoleColor foregroundColor, string text, char borderChar)
        {
            Console.ForegroundColor = foregroundColor;

            string centered = text.PadLeft((LineWidth - 2 + text.Length) / 2).PadRight(LineWidth - 2);
            Console.WriteLine(new string(borderChar, LineWidth));
            Console.WriteLine($"{borderChar}{centered}{borderChar}");
            Console.WriteLine(new string(borderChar, LineWidth));
        }

        private void PrintMenuOptions()
        {
            WriteLineColored("Make a choice:", ConsoleColor.Yellow);
            foreach (var item in _menuItems)
            {
                Console.WriteLine($"- {item.Label} ({item.Key})");
            }
        }

        private static string GetAssemblyName()
        {
            return typeof(Program).Assembly.GetName().Name;
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
