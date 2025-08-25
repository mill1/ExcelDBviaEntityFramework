using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Extensions;
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
                ConsoleFormatter.WriteError(e.ToString());
            }
            finally
            {
                ConsoleFormatter.ResetColor();
            }
        }

        private void ExecuteUserInterface()
        {
            DrawBanner();

            while (!_quit)
            {
                PrintMenuOptions();

                string option = ConsoleFormatter.GetUserInput();

                if (_menuOptions.TryGetValue(option, out var action))
                {
                    try
                    {
                        _uiActions.CheckData();                        
                        action();
                    }
                    catch (SignupException ex)
                    {
                        ConsoleFormatter.WriteWarning(ex.Message);
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        HandleOleDbException(ex);
                    }
                    catch (Exception ex)
                    {
                        ConsoleFormatter.WriteError(ConsoleFormatter.UnexpectedError(ex));
                    }
                }
                else
                {
                    ConsoleFormatter.WriteWarning(ConsoleFormatter.InvalidOption(option));
                }
            }
        }

        private static void HandleOleDbException(System.Data.OleDb.OleDbException ex)
        {
            const string errorMessageExcerpt = "The Microsoft Access database engine could not find the object '.Dual'";

            if (ex.Message.Contains(errorMessageExcerpt))
            {
                ConsoleFormatter.WriteError(ConsoleFormatter.DualObjectNotFound(errorMessageExcerpt));
                return;
            }
            ConsoleFormatter.WriteError(ConsoleFormatter.DatabaseError(ex));
        }

        private void PrintMenuOptions()
        {
            ConsoleFormatter.WriteMenuOption(new string('-', TotalWidth));

            foreach (var item in _menuItems)
            {
                ConsoleFormatter.WriteMenuOption($"- {item.Label} ({item.Key})");
            }

            ConsoleFormatter.WriteMenuOption(new string('-', TotalWidth));
        }

        private void DrawBanner()
        {
            var assemblyName = _assemblyService.GetAssemblyName();
            string appVersion = _assemblyService.GetAssemblyValue("Version", assemblyName);
            string blankLine = new string(' ', TotalWidth);

            ConsoleFormatter.WriteBanner(blankLine + "\r\n" + blankLine);
            ConsoleFormatter.WriteBanner($"{assemblyName.Name}".PadMiddle(TotalWidth, ' '));
            ConsoleFormatter.WriteBanner($"version: {appVersion}".PadMiddle(TotalWidth, ' '));
            ConsoleFormatter.WriteBanner(blankLine + "\r\n" + blankLine);
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
