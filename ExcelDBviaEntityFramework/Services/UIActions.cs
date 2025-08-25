using ExcelDBviaEntityFramework.Console;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Services
{
    public class UIActions : IUIActions
    {
        private readonly ISignupService _signupService;

        public UIActions(ISignupService signupService)
        {
            _signupService = signupService;
        }

        public void AddSignup()
        {
            var name = ConsoleFormatter.GetUserInput("Name:");
            var phone = ConsoleFormatter.GetUserInput("Phone:");
            int partySize = (int)GetValidInteger("Party size:");

            _signupService.Add(MapToDto(name, phone, partySize));

            ConsoleFormatter.WriteSuccess($"Added signup");
        }

        public void UpdateSignup()
        {
            var id = ConsoleFormatter.GetUserInput($"Id of signup to update:");

            var existing = _signupService.GetById(id);

            if (existing == null)
            {
                ConsoleFormatter.WriteInfo($"Signup with {nameof(Signup.Id)} '{id}' not found.");
                return;
            }

            SignupUpsert update = GetUpdateDto(existing);

            if (update.Name == null && update.PhoneNumber == null && update.PartySize == null)
            {
                ConsoleFormatter.WriteInfo($"No changes were made.");
                return;
            }

            var updated = _signupService.Update(existing, update);

            if (updated == null)
                ConsoleFormatter.WriteWarning($"Signup set to null by other process");
            else
                ConsoleFormatter.WriteSuccess($"Updated: {updated}");
        }

        public void DeleteSignup()
        {
            var id = ConsoleFormatter.GetUserInput("Id of signup to delete:");
            bool result = _signupService.Delete(id);
            var message = result ? "The signup is deleted" : $"Signup with id '{id}' not found.";

            if (result)
                ConsoleFormatter.WriteSuccess(message);
            else
                ConsoleFormatter.WriteInfo(message);
        }

        public void ListSignups()
        {
            var signups = _signupService.Get();

            if (!signups.Any())
            {
                ConsoleFormatter.WriteInfo("No signups found.");
                return;
            }

            var maxLength = signups.Max(s => $"{s.Id}{s.Name}{s.PhoneNumber}{s.PartySize}".Length) + 9;
            var line = new string('*', maxLength);

            ConsoleFormatter.WriteInfo($"Current signups:");
            ConsoleFormatter.WriteInfo(line);

            foreach (var signup in signups)
            {
                ConsoleFormatter.WriteSuccess($"{signup}");
            }

            ConsoleFormatter.WriteInfo(line);
            ConsoleFormatter.WriteInfo($"Number of signups: {signups.Count}");
            ConsoleFormatter.WriteInfo($"Average party size: {signups.Average(s => s.PartySize):#.##}");
            var largestParty = signups.OrderByDescending(s => s.PartySize).First();
            ConsoleFormatter.WriteInfo($"Largest: {largestParty.Name}, party of {largestParty.PartySize}");
        }

        public void ListLogsPerSignup()
        {
            var id = ConsoleFormatter.GetUserInput("Id of the signup  (leave empty to show all):");

            if (string.IsNullOrWhiteSpace(id))
            {
                ListLogsOfAllSignups();
                return;
            }

            ListLogsOfSignup(id);
        }

        private void ListLogsOfSignup(string id)
        {
            var signup = _signupService.GetByIdIncludingLogs(id);

            if (signup == null)
            {
                ConsoleFormatter.WriteSuccess($"Signup with {nameof(Signup.Id)} '{id}' not found.");
                return;
            }

            ListLogsPerSignup(signup);
        }

        private void ListLogsOfAllSignups()
        {
            var signups = _signupService.GetIncludingLogs();

            if (!signups.Any())
            {
                ConsoleFormatter.WriteInfo("No signups found.");
                return;
            }

            ConsoleFormatter.WriteInfo("Signups with logs:");

            foreach (var signup in signups)
            {
                ListLogsPerSignup(signup);
            }
        }

        private static void ListLogsPerSignup(Signup signup)
        {
            ConsoleFormatter.WriteSuccess($"Id {signup.Id} ({signup.Name})");

            if (signup.Logs.Any())
            {
                foreach (var log in signup.Logs)
                    ConsoleFormatter.WriteInfo($"  {log}");
            }
            else
            {
                ConsoleFormatter.WriteInfo("  [No logs]");
            }
        }

        public void TestStuff()
        {
            _signupService.TestStuff();
            ConsoleFormatter.WriteInfo("Stuff has been tested");
        }

        public void CheckData()
        {
            _signupService.CheckData();
        }

        private static SignupUpsert GetUpdateDto(Signup existing)
        {
            var newName = ConsoleFormatter.GetUserInput($"New name (leave empty to keep '{existing.Name}'):");
            var newPhone = ConsoleFormatter.GetUserInput($"New phone (leave empty to keep '{existing.PhoneNumber}'):");
            var partySize = GetValidInteger($"New party size (leave empty to keep {existing.PartySize}):", allowNull: true);

            return MapToDto(newName, newPhone, partySize);
        }

        private static SignupUpsert MapToDto(string newName, string newPhone, int? partySize)
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
                string input = ConsoleFormatter.GetUserInput(prompt);

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (allowNull)
                        return null;

                    ConsoleFormatter.WriteWarning("Input cannot be empty. Please try again.");
                    continue;
                }

                if (int.TryParse(input, out int result))
                    return result;

                ConsoleFormatter.WriteWarning("Invalid number. Please try again.");
            }
        }
    }
}
