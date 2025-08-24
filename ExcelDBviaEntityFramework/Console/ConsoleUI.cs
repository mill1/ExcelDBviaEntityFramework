using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Extensions;
using ExcelDBviaEntityFramework.Interfaces;

namespace ExcelDBviaEntityFramework
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
                Console.WriteError(e.ToString());
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private void ExecuteUserInterface()
        {
            DrawBanner();

            while (!_quit)
            {
                PrintMenuOptions();

                string option = Console.GetUserInput();

                if (_menuOptions.TryGetValue(option, out var action))
                {
                    try
                    {
                        _uiActions.CheckData();                        
                        action();
                    }
                    catch (SignupException ex)
                    {
                        Console.WriteWarning(ex.Message);
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        HandleOleDbException(ex);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteError(Console.UnexpectedError(ex));
                    }
                }
                else
                {
                    Console.WriteWarning(Console.InvalidOption(option));
                }
            }
        }

        private static void HandleOleDbException(System.Data.OleDb.OleDbException ex)
        {
            const string errorMessageExcerpt = "The Microsoft Access database engine could not find the object '.Dual'";

            if (ex.Message.Contains(errorMessageExcerpt))
            {
                Console.WriteError(Console.DualObjectNotFound(errorMessageExcerpt));
                return;
            }
            Console.WriteError(Console.DatabaseError(ex));
        }

        private void PrintMenuOptions()
        {
            Console.WriteMenuOption(new string('-', TotalWidth));

            foreach (var item in _menuItems)
            {
                Console.WriteMenuOption($"- {item.Label} ({item.Key})");
            }

            Console.WriteMenuOption(new string('-', TotalWidth));
        }

        private void DrawBanner()
        {
            var assemblyName = _assemblyService.GetAssemblyName();
            string appVersion = _assemblyService.GetAssemblyValue("Version", assemblyName);
            string blankLine = new string(' ', TotalWidth);

            Console.WriteBanner(blankLine + "\r\n" + blankLine);
            Console.WriteBanner($"{assemblyName.Name}".PadMiddle(TotalWidth, ' '));
            Console.WriteBanner($"version: {appVersion}".PadMiddle(TotalWidth, ' '));
            Console.WriteBanner(blankLine + "\r\n" + blankLine);
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
