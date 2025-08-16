using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Services;
using System.Data;

namespace ExcelDBviaEntityFramework.UI
{
    public class ConsoleUI
    {        
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly ISignupService _signupService;
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

        public ConsoleUI(ISignupService signupService)
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
                ConsoleHelper.WriteLineColored(e.ToString(), ConsoleColor.Red);
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

                string option = ConsoleHelper.GetUserInput();

                if (_menuOptions.TryGetValue(option, out var action))
                {
                    try
                    {
                        action();
                    }
                    catch (DBConcurrencyException ex)
                    {
                        ConsoleHelper.WriteLineColored(ex.Message, ConsoleColor.Magenta);
                    }
                    catch (Exception ex)
                    {                        
                        ConsoleHelper.WriteLineColored($"Unexpected error: {ex.Message}", ConsoleColor.Red);
                    }
                }
                else
                {                    
                    ConsoleHelper.WriteLineColored($"Invalid option: {option}", ConsoleColor.Magenta);
                }
            }
        }

        private void PrintMenuOptions()
        {            
            ConsoleHelper.WriteLineColored("Make a choice:", ConsoleColor.Yellow);

            foreach (var item in _menuItems)
            {
                ConsoleHelper.WriteLineColored($"- {item.Label} ({item.Key})", ConsoleColor.Yellow);
            }
        }

        private static void DrawBanner()
        {
            const int LineWidth = 40;
            var assName = GetAssemblyName();
            var borderChar = '#';
            string centered = assName.PadLeft((LineWidth - 2 + assName.Length) / 2).PadRight(LineWidth - 2);            

            ConsoleHelper.WriteLineColored(new string(borderChar, LineWidth), ConsoleColor.Green);
            ConsoleHelper.WriteLineColored($"{borderChar}{centered}{borderChar}", ConsoleColor.Green);
            ConsoleHelper.WriteLineColored(new string(borderChar, LineWidth), ConsoleColor.Green);
        }

        private static string GetAssemblyName()
        {
            return typeof(Program).Assembly.GetName().Name;
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
