using ExcelDBviaEntityFramework.Services;

namespace ExcelDBviaEntityFramework.UI
{
    public class ConsoleUI
    {
        private const int LineWidth = 40;
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly SignupService _signupService;
        private readonly UIActions _actions;
        private bool _quit;

        private static class MenuOptions
        {
            public const string AddSignup = "a";
            public const string UpdateSignup = "u";
            public const string DeleteSignup = "d";
            public const string PrintSignups = "p";
            public const string Help = "h";
            public const string Quit = "q";
        }

        public ConsoleUI(SignupService signupService)
        {
            _signupService = signupService;
            _actions = new UIActions(signupService);

            _menuItems =
            [
                new("Add sign up", MenuOptions.AddSignup, _actions.AddSignup),
                new("Update sign up", MenuOptions.UpdateSignup, _actions.UpdateSignup),
                new("Delete sign up", MenuOptions.DeleteSignup, _actions.DeleteSignup),
                new("Print sign ups", MenuOptions.PrintSignups, _actions.ListSignups),
                new("Help", MenuOptions.Help, _actions.ShowHelp),
                new("Quit", MenuOptions.Quit, () => _quit = true)
            ];

            _menuOptions = _menuItems.ToDictionary(m => m.Key, m => m.Action, StringComparer.OrdinalIgnoreCase);
        }

        public void Run()
        {
            try
            {
                ExecuteUserInterface();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.ResetColor();

                // TODO fysieke delete van xl rows
            }
        }

        private void ExecuteUserInterface()
        {
            DrawBanner();

            while (!_quit)
            {
                PrintMenuOptions();

                // TODO: Console. calls verplaatsen naar centrale UIHelper class, ook aanpassen in UIActions
                Console.ForegroundColor = ConsoleColor.White;
                string option = Console.ReadLine()?.Trim() ?? string.Empty;

                if (_menuOptions.TryGetValue(option, out var action))
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"Invalid option: {option}");
                }
            }
        }

        private void PrintMenuOptions()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Make a choice:");

            foreach (var item in _menuItems)
            {
                Console.WriteLine($"- {item.Label} ({item.Key})");
            }
        }

        private static void DrawBanner()
        {
            var assName = GetAssemblyName();
            var borderChar = '#';
            string centered = assName.PadLeft((LineWidth - 2 + assName.Length) / 2).PadRight(LineWidth - 2);

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(new string(borderChar, LineWidth));
            Console.WriteLine($"{borderChar}{centered}{borderChar}");
            Console.WriteLine(new string(borderChar, LineWidth));
        }

        private static string GetAssemblyName()
        {
            return typeof(Program).Assembly.GetName().Name;
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
