namespace ExcelDBviaEntityFramework.Services
{
    public class UIService
    {
        private readonly Dictionary<string, Action> _menuOptions;
        private readonly List<MenuItem> _menuItems;
        private readonly SignupService _signUpService;
        private bool _quit;
        private const int LineWidth = 40;

        private static class MenuOptions
        {
            public const string AddSignUp = "a";
            public const string UpdateSignUp = "u";
            public const string DeleteSignUp = "d";
            public const string PrintSignUps = "p";
            public const string Help = "h";
            public const string Quit = "q";
        }

        public UIService()
        {
            _signUpService = new SignupService();

            _menuItems =
            [
                new("Add sign up", MenuOptions.AddSignUp, AddSignUp),
                new("Update sign up", MenuOptions.UpdateSignUp, UpdateSignUp),
                new("Delete sign up", MenuOptions.DeleteSignUp, DeleteSignUp),
                new("Print sign ups", MenuOptions.PrintSignUps, PrintSignUps),
                new("Help", MenuOptions.Help, ShowHelp),
                new("Quit", MenuOptions.Quit, () => _quit = true)
            ];

            _menuOptions = _menuItems.ToDictionary(m => m.Key, m => m.Action, StringComparer.OrdinalIgnoreCase);
        }

        public void RunUI()
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

        private void UpdateSignUp()
        {
            var id = GetUserInput("Id of sign up to update:");
            var existing = _signUpService.GetSignUpById(id);

            if (existing == null)
            {
                WriteLineColored($"Sign up with id '{id}' not found.", ConsoleColor.Magenta);
                return;
            }

            WriteLineColored($"Current: {existing}", ConsoleColor.Cyan);

            var newName = GetUserInput($"New name (leave empty to keep '{existing.Name}'):");
            var newPhone = GetUserInput($"New phone (leave empty to keep '{existing.PhoneNumber}'):");
            string partySizeInput = GetUserInput($"New party size (leave empty to keep {existing.PartySize}):");

            int partySize = existing.PartySize;
            if (!string.IsNullOrWhiteSpace(partySizeInput))
            {
                if (int.TryParse(partySizeInput, out int parsed)) partySize = parsed;
                else
                {
                    WriteLineColored("Invalid number entered for party size. Keeping old value.", ConsoleColor.Magenta);
                }
            }

            var updated = _signUpService.UpdateSignup(
                id,
                string.IsNullOrWhiteSpace(newName) ? existing.Name : newName,
                string.IsNullOrWhiteSpace(newPhone) ? existing.PhoneNumber : newPhone,
                partySize
            );

            if (updated != null)
                WriteLineColored($"Sign up updated successfully:\r\n{updated}", ConsoleColor.Cyan);
            else
                WriteLineColored("Update failed.", ConsoleColor.Magenta);
        }

        private void DeleteSignUp()
        {
            var id = GetUserInput("Id of sign up to delete:");

            bool result = _signUpService.DeleteSignup(id);

            var message = result ? "The Sign up is deleted" : $"Sign up with id '{id}' could not be found.";

            WriteLineColored($"{message}", ConsoleColor.Cyan);
        }

        private void PrintSignUps()
        {
            var signUps = _signUpService.GetSignups();

            WriteLineColored($"Current sign ups:", ConsoleColor.Cyan);
            WriteLineColored(new string('-', LineWidth), ConsoleColor.Cyan);

            foreach (var entry in signUps)
            {
                WriteLineColored($"* {entry}", ConsoleColor.Cyan);
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
                WriteLineColored(prompt, ConsoleColor.Cyan);
            }

            Console.ForegroundColor = ConsoleColor.White;

            return Console.ReadLine()?.Trim() ?? string.Empty;
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
