using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Extensions;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;

namespace ExcelDBviaEntityFramework.Console
{
    public class ConsoleUI
    {
        private const int TotalWidth = 40;
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly IUIActions _uiActions;
        private readonly IAssemblyService _assemblyService;
        private bool _quit;

        private static class MenuOptions
        {
            public const string AddSignup = "a";
            public const string UpdateSignup = "u";
            public const string DeleteSignup = "d";
            public const string PrintSignups = "p";
            public const string LogsPerSignup = "l";
            public const string TestStuff = "t";
            public const string Quit = "q";
        }

        public ConsoleUI(IUIActions uiActions, IAssemblyService assemblyService)
        {
            _uiActions = uiActions;
            _assemblyService = assemblyService;
            _menuItems =
            [
                new("Add signup", MenuOptions.AddSignup, _uiActions.AddSignup),
                new("Update signup", MenuOptions.UpdateSignup, _uiActions.UpdateSignup),
                new("Delete signup", MenuOptions.DeleteSignup, _uiActions.DeleteSignup),
                new("Print signups", MenuOptions.PrintSignups, _uiActions.ListSignups),
                new("Print Logs per signup", MenuOptions.LogsPerSignup, _uiActions.ListLogsPerSignup),
                new("Test stuff", MenuOptions.TestStuff, _uiActions.TestStuff),
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
                System.Console.ResetColor();
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
                    catch (SignupException ex)
                    {
                        ConsoleHelper.WriteLineColored(ex.Message, ConsoleColor.Magenta);
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        HandleOleDbException(ex);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteLineColored(ErrorMessageFormatter.UnexpectedError(ex), ConsoleColor.Red);
                    }
                }
                else
                {
                    ConsoleHelper.WriteLineColored(ErrorMessageFormatter.InvalidOption(option), ConsoleColor.Magenta);
                }
            }
        }

        private static void HandleOleDbException(System.Data.OleDb.OleDbException ex)
        {
            const string errorMessageExcerpt = "The Microsoft Access database engine could not find the object '.Dual'";

            if (ex.Message.Contains(errorMessageExcerpt))
            {
                ConsoleHelper.WriteLineColored(ErrorMessageFormatter.DualObjectNotFound(errorMessageExcerpt), ConsoleColor.Red);
                return;
            }
            ConsoleHelper.WriteLineColored(ErrorMessageFormatter.DatabaseError(ex), ConsoleColor.Red);
        }

        private void PrintMenuOptions()
        {
            ConsoleHelper.WriteLineColored(new string('-', TotalWidth), ConsoleColor.Yellow);

            foreach (var item in _menuItems)
            {
                ConsoleHelper.WriteLineColored($"- {item.Label} ({item.Key})", ConsoleColor.Yellow);
            }

            ConsoleHelper.WriteLineColored(new string('-', TotalWidth), ConsoleColor.Yellow);
        }

        private void DrawBanner()
        {
            var assemblyName = _assemblyService.GetAssemblyName();
            string appVersion = _assemblyService.GetAssemblyValue("Version", assemblyName);
            string blankLine = new string(' ', TotalWidth);

            ConsoleHelper.WriteLineColored(blankLine + "\r\n" + blankLine, ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleHelper.WriteLineColored($"{assemblyName.Name}".PadMiddle(TotalWidth, ' '), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleHelper.WriteLineColored($"version: {appVersion}".PadMiddle(TotalWidth, ' '), ConsoleColor.White, ConsoleColor.DarkBlue);
            ConsoleHelper.WriteLineColored(blankLine + "\r\n" + blankLine, ConsoleColor.White, ConsoleColor.DarkBlue);
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
