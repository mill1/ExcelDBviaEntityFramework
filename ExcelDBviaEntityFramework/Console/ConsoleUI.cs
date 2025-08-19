using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Services;

namespace ExcelDBviaEntityFramework.Console
{
    public class ConsoleUI
    {
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly UIActions _actions;
        private bool _quit;

        private static class MenuOptions
        {
            public const string AddSignup = "a";
            public const string UpdateSignup = "u";
            public const string DeleteSignup = "d";
            public const string PrintSignups = "p";
            public const string TestStuff = "t";
            public const string Help = "h";
            public const string Quit = "q";
        }

        public ConsoleUI(ISignupService signupService)
        {
            _actions = new UIActions(signupService);

            _menuItems =
            [
                new("Add signup", MenuOptions.AddSignup, _actions.AddSignup),
                new("Update signup", MenuOptions.UpdateSignup, _actions.UpdateSignup),
                new("Delete signup", MenuOptions.DeleteSignup, _actions.DeleteSignup),
                new("Print signups", MenuOptions.PrintSignups, _actions.ListSignups),
                new("Test stuff", MenuOptions.TestStuff, _actions.TestStuff),

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
                        var errorMessageExcerpt = "The Microsoft Access database engine could not find the object '.Dual'";

                        if (ex.Message.Contains(errorMessageExcerpt))
                        {
                            ConsoleHelper.WriteLineColored(GetDualObjectNotFoundErrorMessage(errorMessageExcerpt), ConsoleColor.Red);
                            return;
                        }
                        ConsoleHelper.WriteLineColored(GetDatabaseErrorMessage(ex), ConsoleColor.Red);
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

        private static string GetDatabaseErrorMessage(System.Data.OleDb.OleDbException ex)
        {
            var sheetName = Constants.SheetNameSignups.Replace("$", string.Empty);

            return $"""
                Error connecting to the Excel data. 
                Exception: {ex.Message}
                Requirements w.r. to the Excel file database:
                - The file name should be {Constants.ExcelFileName}
                - The file should contain a sheet named {sheetName}
                - The first row of {sheetName} should contain headers
                - Column {Constants.SignupsColumnIndexDeleted} should be named {nameof(Signup.Deleted_ý)}
                - Column {Constants.SignupsColumnIndexId} should be named {nameof(Signup.Id)}
                """;
        }

        private static string GetDualObjectNotFoundErrorMessage(string errorMessageExcerpt)
        {
            return $"""                                
                The Jet provider has issued a .Dual probe when EF asked it for metadata w.r. to the query.
                Since Excel is not a relational database this causes next error:
                {errorMessageExcerpt}
                There are two workarounds for this:
                1. Force EF to skip relational translation paths by using no-tracking raw SQL instead of LINQ:
                    return ctx.Signups
                        .FromSqlRaw($"SELECT * FROM [{Constants.SheetNameSignups}]")
                        .AsNoTracking()
                        .ToList();
                2. Switch to client-side evaluation
                   If you must keep LINQ, call .AsEnumerable() before filtering so EF doesn’t try to translate.
                   For instance use the `AsEnumerable()` method on the DbSet, e.g.:
                    return ctx.Signups
                        .AsNoTracking()
                        .AsEnumerable()   // 🚀 forces client-side LINQ
                        .Where(s => !string.IsNullOrEmpty(s.Id));
                """;
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
