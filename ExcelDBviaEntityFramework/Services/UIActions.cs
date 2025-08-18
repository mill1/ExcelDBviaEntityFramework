using ExcelDBviaEntityFramework.Console;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;

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
            _signupService.CheckData();

            var name = ConsoleHelper.GetUserInput("Name:");
            var phone = ConsoleHelper.GetUserInput("Phone:");
            int partySize = (int)GetValidInteger("Party size:");

            var signup = _signupService.AddSignup(CreateInsertDto(name, phone, partySize));

            ConsoleHelper.WriteLineColored($"Added signup: {signup}", ConsoleColor.Cyan);
        }

        public void UpdateSignup()
        {
            _signupService.CheckData();

            var id = ConsoleHelper.GetUserInput($"Id of signup to update:");
            var existing = _signupService.GetSignupByEFId(id);

            if (existing == null)
            {
                ConsoleHelper.WriteLineColored($"Signup with {nameof(Signup.Id)} '{id}' not found.", ConsoleColor.Cyan);
                return;
            }
            
            SignupUpsert update = GetUpdateDto(existing);

            if (update.Name == null && update.PhoneNumber == null && update.PartySize == null)
            {
                ConsoleHelper.WriteLineColored($"No changes were made.", ConsoleColor.Cyan);
                return;
            }

            var updated = _signupService.UpdateSignup(id, update);

            if (updated != null)
                ConsoleHelper.WriteLineColored($"Updated: {updated}", ConsoleColor.Green);
        }

        public void DeleteSignup()
        {
            _signupService.CheckData();

            var id = ConsoleHelper.GetUserInput("Id of signup to delete:");
            bool result = _signupService.DeleteSignup(id);
            var message = result ? "The signup is deleted" : $"Signup with id '{id}' not found.";

            ConsoleHelper.WriteLineColored($"{message}", ConsoleColor.Green);
        }

        public void ListSignups()
        {
            _signupService.CheckData(checkIdUniqueness: false);

            var signups = _signupService.GetSignups();

            var maxLength = signups.Max(s => $"{s.Id}{s.Name}{s.PhoneNumber}{s.PartySize}".Length) + 9;

            ConsoleHelper.WriteLineColored($"Current signups:", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored(new string('-', maxLength), ConsoleColor.Cyan);

            foreach (var signup in signups)
            {
                ConsoleHelper.WriteLineColored($"{signup}", ConsoleColor.Cyan);
            }
            ConsoleHelper.WriteLineColored(new string('-', maxLength), ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored($"Number of signups: {signups.Count}", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored($"Average party size: {signups.Average(s => s.PartySize):#.##}", ConsoleColor.Cyan);
            var largestParty = signups.OrderByDescending(s => s.PartySize).First();
            ConsoleHelper.WriteLineColored($"Largest: {largestParty.Name}, party of {largestParty.PartySize}", ConsoleColor.Cyan);
        }

        public void ShowHelp()
        {
            ConsoleHelper.WriteLineColored("Select an option by typing the associated letter and press Enter.", ConsoleColor.Cyan);
        }

        private static SignupUpsert GetUpdateDto(Signup existing)
        {
            var newName = ConsoleHelper.GetUserInput($"New name (leave empty to keep '{existing.Name}'):");
            var newPhone = ConsoleHelper.GetUserInput($"New phone (leave empty to keep '{existing.PhoneNumber}'):");
            var partySize = GetValidInteger($"New party size (leave empty to keep {existing.PartySize}):", allowNull: true);

            return CreateUpdateDto(newName, newPhone, partySize);
        }

        private static SignupUpsert CreateInsertDto(string newName, string newPhone, int? partySize)
        {
            return new SignupUpsert
            {
                Name = string.IsNullOrWhiteSpace(newName) ? null : newName,
                PhoneNumber = string.IsNullOrWhiteSpace(newPhone) ? null : newPhone,
                PartySize = partySize
            };
        }

        private static SignupUpsert CreateUpdateDto(string newName, string newPhone, int? partySize)
        {
            return new SignupUpsert
            {
                Name = string.IsNullOrWhiteSpace(newName) ? null : newName,
                PhoneNumber = string.IsNullOrWhiteSpace(newPhone) ? null : newPhone,
                PartySize = partySize
            };
        }

        private static int? GetValidInteger(string prompt, bool allowNull = false)
        {
            while (true)
            {
                string input = ConsoleHelper.GetUserInput(prompt);

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (allowNull)
                        return null;

                    ConsoleHelper.WriteLineColored("Input cannot be empty. Please try again.", ConsoleColor.Magenta);
                    continue;
                }

                if (int.TryParse(input, out int result))
                    return result;

                ConsoleHelper.WriteLineColored("Invalid number. Please try again.", ConsoleColor.Magenta);
            }
        }
    }
}
