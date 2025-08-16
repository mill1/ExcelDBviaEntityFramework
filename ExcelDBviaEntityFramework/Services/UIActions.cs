using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.UI;

namespace ExcelDBviaEntityFramework.Services
{
    public class UIActions
    {        
        private readonly ISignupService _signupService;

        public UIActions(ISignupService signupService)
        {
            _signupService = signupService;
        }

        public void AddSignup()
        {
            var name = ConsoleHelper.GetUserInput("Name:");
            var phone = ConsoleHelper.GetUserInput("Phone:");
            int partySize = GetValidInteger("Party size:");

            var signup = _signupService.AddSignup(name, phone, partySize);

            ConsoleHelper.WriteLineColored($"Added sign up: {signup}", ConsoleColor.Cyan);
        }

        public void UpdateSignup()
        {
            var id = ConsoleHelper.GetUserInput("Id of sign up to update:");
            var existing = _signupService.GetSignupById(id);

            if (existing == null)
            {
                ConsoleHelper.WriteLineColored($"Sign up with id '{id}' not found.", ConsoleColor.Magenta);
                return;
            }

            ConsoleHelper.WriteLineColored($"Current: {existing}", ConsoleColor.Cyan);

            var newName = ConsoleHelper.GetUserInput($"New name (leave empty to keep '{existing.Name}'):");
            var newPhone = ConsoleHelper.GetUserInput($"New phone (leave empty to keep '{existing.PhoneNumber}'):");
            string partySizeInput = ConsoleHelper.GetUserInput($"New party size (leave empty to keep {existing.PartySize}):");

            int partySize = existing.PartySize;
            if (!string.IsNullOrWhiteSpace(partySizeInput))
            {
                if (int.TryParse(partySizeInput, out int parsed))
                    partySize = parsed;
                else
                    ConsoleHelper.WriteLineColored("Invalid number entered for party size. Keeping old value.", ConsoleColor.Magenta);
            }

            var updated = _signupService.UpdateSignup(
                id,
                string.IsNullOrWhiteSpace(newName) ? existing.Name : newName,
                string.IsNullOrWhiteSpace(newPhone) ? existing.PhoneNumber : newPhone,
                partySize
            );

            if (updated != null)
                ConsoleHelper.WriteLineColored($"Sign up updated successfully:\r\n{updated}", ConsoleColor.Cyan);
            else
                ConsoleHelper.WriteLineColored("Update failed.", ConsoleColor.Magenta);
        }

        public void DeleteSignup()
        {
            var id = ConsoleHelper.GetUserInput("Id of sign up to delete:");

            bool result = _signupService.DeleteSignup(id);

            var message = result ? "The Sign up is deleted" : $"Sign up with id '{id}' could not be found.";

            ConsoleHelper.WriteLineColored($"{message}", ConsoleColor.Cyan);
        }

        public void ListSignups()
        {
            var signups = _signupService.GetSignups();

            var maxLength = signups.Max(s => $"{s.Name}{s.PhoneNumber}{s.PartySize}".Length) + 16;

            ConsoleHelper.WriteLineColored($"Current sign ups:", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored(new string('-', maxLength), ConsoleColor.Cyan);

            foreach (var signup in signups)
            {
                ConsoleHelper.WriteLineColored($"* {signup}", ConsoleColor.Cyan);
            }
            ConsoleHelper.WriteLineColored(new string('-', maxLength), ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored($"Number of sign ups: {signups.Count}", ConsoleColor.Cyan);
        }

        public void ShowHelp()
        {
            ConsoleHelper.WriteLineColored("Select an option by typing the associated letter and press Enter.", ConsoleColor.Cyan);
        }

        private static int GetValidInteger(string prompt)
        {
            while (true)
            {
                string input = ConsoleHelper.GetUserInput(prompt);
                if (int.TryParse(input, out int result))
                    return result;

                ConsoleHelper.WriteLineColored("Invalid number. Please try again.", ConsoleColor.Magenta);
            }
        }
    }
}
