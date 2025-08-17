using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Console;

namespace ExcelDBviaEntityFramework.Services
{
    public class UIActions
    {        
        private readonly ISignupService _signupService;
        private readonly string _filePath;

        public UIActions(ISignupService signupService)
        {
            _filePath = FileHelper.ResolveExcelPath(Constants.ExcelFileName);
            _signupService = signupService;
        }

        public void AddSignup()
        {
            FileHelper.EnsureFileNotLocked(_filePath);

            var name = ConsoleHelper.GetUserInput("Name:");
            var phone = ConsoleHelper.GetUserInput("Phone:");
            int partySize = (int)GetValidInteger("Party size:");

            var signup = _signupService.AddSignup(CreateInsertDto(name, phone, partySize));

            ConsoleHelper.WriteLineColored($"Added signup: {signup}", ConsoleColor.Cyan);
        }

        public void UpdateSignup()
        {
            FileHelper.EnsureFileNotLocked(_filePath);

            var id = ConsoleHelper.GetUserInput($"EF id ({nameof(Signup.Id_ý)}) of signup to update:");
            var existing = _signupService.GetSignupByEFId(id);

            if (existing == null)
            {
                ConsoleHelper.WriteLineColored($"Signup with {nameof(Signup.Id_ý)} '{id}' not found.", ConsoleColor.Cyan);
                return;
            }

            ConsoleHelper.WriteLineColored($"Current: {existing}", ConsoleColor.Cyan);
            SignupUpdate update = GetUpdateDto(existing);

            var updated = _signupService.UpdateSignup(id, update);

            if (updated != null)
                ConsoleHelper.WriteLineColored($"Updated: {updated}", ConsoleColor.Green);
        }

        public void DeleteSignup()
        {
            FileHelper.EnsureFileNotLocked(_filePath);

            var id = ConsoleHelper.GetUserInput("Id of signup to delete:");
            bool result = _signupService.DeleteSignup(id);
            var message = result ? "The signup is deleted" : $"Signup with id '{id}' not found.";

            ConsoleHelper.WriteLineColored($"{message}", ConsoleColor.Cyan);
        }

        public void ListSignups()
        {
            FileHelper.EnsureFileNotLocked(_filePath);

            var signups = _signupService.GetSignups();

            var maxLength = signups.Max(s => $"{s.Id}{s.Name}{s.PhoneNumber}{s.PartySize}".Length) + 27;

            ConsoleHelper.WriteLineColored($"Current signups:", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored(new string('-', maxLength), ConsoleColor.Cyan);

            foreach (var signup in signups)
            {
                ConsoleHelper.WriteLineColored($"{signup}", ConsoleColor.Cyan);
            }
            ConsoleHelper.WriteLineColored(new string('-', maxLength), ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColored($"Number of signups: {signups.Count}", ConsoleColor.Cyan);
        }

        public void ShowHelp()
        {
            ConsoleHelper.WriteLineColored("Select an option by typing the associated letter and press Enter.", ConsoleColor.Cyan);
        }

        private static SignupUpdate GetUpdateDto(Signup existing)
        {
            var newId = ConsoleHelper.GetUserInput($"New id (leave empty to keep '{existing.Id}'):");
            var newName = ConsoleHelper.GetUserInput($"New name (leave empty to keep '{existing.Name}'):");
            var newPhone = ConsoleHelper.GetUserInput($"New phone (leave empty to keep '{existing.PhoneNumber}'):");
            var partySize = GetValidInteger($"New party size (leave empty to keep {existing.PartySize}):", allowNull:true);

            return CreateUpdateDto(newId, newName, newPhone, partySize);
        }

        private static SignupInsert CreateInsertDto(string newName, string newPhone, int? partySize)
        {
            return new SignupInsert
            {
                Name = string.IsNullOrWhiteSpace(newName) ? null : newName,
                PhoneNumber = string.IsNullOrWhiteSpace(newPhone) ? null : newPhone,
                PartySize = partySize
            };
        }

        private static SignupUpdate CreateUpdateDto(string newId, string newName, string newPhone, int? partySize)
        {
            return new SignupUpdate
            {
                Id = string.IsNullOrWhiteSpace(newId) ? null : newId,
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
