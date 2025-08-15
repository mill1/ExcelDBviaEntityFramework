using ExcelDBviaEntityFramework.Services;

namespace ExcelDBviaEntityFramework
{
    public class Runner
    {
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly SignupService _signUpService;
        private bool _quit;
        private const int LineWidth = 40;

        private static class MenuOptions
        {
            public const string AddSignUp = "a";
            public const string DeleteSignUp = "d";
            public const string PrintSignUps = "p";
            public const string CRUDSignUps = "c";
            public const string Help = "h";
            public const string Quit = "q";
        }

        public Runner()
        {
            _signUpService = new SignupService();

            _menuItems =
            [
                new("Add sign up", MenuOptions.AddSignUp, AddSignUp),
                new("Delete sign up", MenuOptions.DeleteSignUp, DeleteSignUp),
                new("CRUD sign up", MenuOptions.CRUDSignUps, CRUDSignUps),
                new("Print sign ups", MenuOptions.PrintSignUps, PrintSignUps),
                new("Help", MenuOptions.Help, ShowHelp),
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
                WriteLineColored(e.ToString(), ConsoleColor.Red);
            }
            finally
            {
                Console.ResetColor();

                // TODO fysieke delete van xl rows
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

        private void AddSignUp()
        {
            var name = GetUserInput("Name:");
            var phone = GetUserInput("Phone:");
            int partySize = GetValidInteger("Party size:");

            var signUp = _signUpService.AddSignup(name, phone, partySize);

            WriteLineColored($"Added sign up: {signUp}", ConsoleColor.Cyan);
        }


        private void DeleteSignUp()
        {
            var id = GetUserInput("Id of sign up to delete:");

            bool result = _signUpService.DeleteSignup(id);

            var message = result ? "The Sign up is deleted" : $"Sign up with id {id} could not be found.";

            WriteLineColored($"{message}", ConsoleColor.Cyan);
        }

        private void CRUDSignUps()
        {
            var firstSignup = _signUpService.GetSignups().FirstOrDefault();

            if (firstSignup == null)
            {
                WriteLineColored("Database is empty. This functionality requires at least one entry.", ConsoleColor.Magenta);
                return;
            }

            WriteLineColored($"Testing all CRUD operations...", ConsoleColor.Cyan);            

            var nameOfEntriesToDelete = GetUserInput($"Enter the name of the sign up(s) to delete (not '{firstSignup.Name}'):");

            if (firstSignup.Name.ToLower() == nameOfEntriesToDelete.ToLower())
            {
                WriteLineColored($"The name can not be '{firstSignup.Name}'! You thick?", ConsoleColor.Magenta);
                return;
            }


            _signUpService.CRUDSignUps(nameOfEntriesToDelete);

            var updatedSignup = _signUpService.GetSignUpByName(firstSignup.Name);

            WriteLineColored($"Before update: {firstSignup}", ConsoleColor.Cyan);
            WriteLineColored($"After update: {updatedSignup}", ConsoleColor.Cyan);

            var signups = _signUpService.GetSignups();

            WriteLineColored($"Count: {signups.Count}", ConsoleColor.Cyan);
            WriteLineColored($"Avg party size: {signups.Average(se => se.PartySize)}", ConsoleColor.Cyan);
        }

        private void PrintSignUps()
        {
            var signUps = _signUpService.GetSignups();

            WriteLineColored($"Current sign ups:", ConsoleColor.Cyan);
            WriteLineColored(new string('-', LineWidth), ConsoleColor.Cyan);

            foreach (var entry in signUps)
            {
                WriteLineColored($"\t{entry}", ConsoleColor.Cyan);
            }
            WriteLineColored(new string('-', LineWidth), ConsoleColor.Cyan);
            WriteLineColored($"Number of sign ups: {signUps.Count}", ConsoleColor.Cyan);
        }

        private void ShowHelp()
        {
            DrawBanner(ConsoleColor.Cyan, $"{GetAssemblyName()} help", '-');
            WriteLineColored("Select an option by typing the associated letter and press Enter.", ConsoleColor.Cyan);
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
            string centered = text.PadLeft((LineWidth - 2 + text.Length) / 2).PadRight(LineWidth - 2);

            WriteLineColored(new string(borderChar, LineWidth), foregroundColor);
            WriteLineColored($"{borderChar}{centered}{borderChar}", foregroundColor);
            WriteLineColored(new string(borderChar, LineWidth), foregroundColor);
        }

        private void PrintMenuOptions()
        {
            WriteLineColored("Make a choice:", ConsoleColor.Yellow);
            foreach (var item in _menuItems)
            {
                WriteLineColored($"- {item.Label} ({item.Key})", ConsoleColor.Yellow);
            }
        }

        private static string GetAssemblyName()
        {
            return typeof(Program).Assembly.GetName().Name;
        }

        private record MenuItem(string Label, string Key, Action Action);
    }
}
