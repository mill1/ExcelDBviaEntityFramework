using ExcelDBviaEntityFramework.Services;

namespace ExcelDBviaEntityFramework.UI
{
    public class UIActions
    {        
        private readonly SignupService _signupService;

        public UIActions(SignupService signupService)
        {
            _signupService = signupService;
        }

        public void AddSignup()
        {
            var name = GetUserInput("Name:");
            var phone = GetUserInput("Phone:");
            int partySize = GetValidInteger("Party size:");

            var signup = _signupService.AddSignup(name, phone, partySize);

            WriteLineColored($"Added sign up: {signup}", ConsoleColor.Cyan);
        }

        public void UpdateSignup()
        {
            var id = GetUserInput("Id of sign up to update:");
            var existing = _signupService.GetSignupById(id);

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
                if (int.TryParse(partySizeInput, out int parsed))
                    partySize = parsed;
                else
                    WriteLineColored("Invalid number entered for party size. Keeping old value.", ConsoleColor.Magenta);
            }

            var updated = _signupService.UpdateSignup(
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

        public void DeleteSignup()
        {
            var id = GetUserInput("Id of sign up to delete:");

            bool result = _signupService.DeleteSignup(id);

            var message = result ? "The Sign up is deleted" : $"Sign up with id '{id}' could not be found.";

            WriteLineColored($"{message}", ConsoleColor.Cyan);
        }

        public void ListSignups()
        {
            var signups = _signupService.GetSignups();

            WriteLineColored($"Current sign ups:", ConsoleColor.Cyan);
            WriteLineColored(new string('-', 35), ConsoleColor.Cyan);

            foreach (var entry in signups)
            {
                WriteLineColored($"* {entry}", ConsoleColor.Cyan);
            }
            WriteLineColored(new string('-', 35), ConsoleColor.Cyan);
            WriteLineColored($"Number of sign ups: {signups.Count}", ConsoleColor.Cyan);
        }

        public void ShowHelp()
        {
            WriteLineColored("Select an option by typing the associated letter and press Enter.", ConsoleColor.Cyan);
        }

        private static string GetUserInput(string prompt)
        {
            WriteLineColored(prompt, ConsoleColor.Cyan);

            Console.ForegroundColor = ConsoleColor.White;

            return Console.ReadLine()?.Trim() ?? string.Empty;
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

        private static void WriteLineColored(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
    }
}
