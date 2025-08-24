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
            var name = Console.GetUserInput("Name:");
            var phone = Console.GetUserInput("Phone:");
            var inputPartySize = GetValidInteger("Party size:");
            int partySize =  inputPartySize.HasValue ? inputPartySize.Value : 0;

            _signupService.Add(MapToDto(name, phone, partySize));

            Console.WriteSuccess($"Added signup");
        }

        public void UpdateSignup()
        {
            var id = Console.GetUserInput($"Id of signup to update:");
            
            var existing = _signupService.GetById(id);

            if (existing == null)
            {
                Console.WriteInfo($"Signup with {nameof(Signup.Id)} '{id}' not found.");
                return;
            }

            SignupUpsert update = GetUpdateDto(existing);

            if (update.Name == null && update.PhoneNumber == null && update.PartySize == null)
            {
                Console.WriteInfo($"No changes were made.");
                return;
            }

            var updated = _signupService.Update(existing, update);

            if (updated == null)
                Console.WriteWarning($"Signup set to null by other process");
            else
                Console.WriteSuccess($"Updated: {updated}");
        }

        public void DeleteSignup()
        {
            var id = Console.GetUserInput("Id of signup to delete:");
            bool result = _signupService.Delete(id);
            var message = result ? "The signup is deleted" : $"Signup with id '{id}' not found.";

            if(result)
                Console.WriteSuccess(message);
            else
                Console.WriteInfo(message);            
        }

        public void ListSignups()
        {
            var signups = _signupService.Get();

            if (!signups.Any())
            {
                Console.WriteInfo("No signups found.");
                return;
            } 

            var maxLength = signups.Max(s => $"{s.Id}{s.Name}{s.PhoneNumber}{s.PartySize}".Length) + 9;
            var line = new string('*', maxLength);

            Console.WriteInfo($"Current signups:");
            Console.WriteInfo(line);

            foreach (var signup in signups)
            {
                Console.WriteSuccess($"{signup}");
            }

            Console.WriteInfo(line);
            Console.WriteInfo($"Number of signups: {signups.Count}");
            Console.WriteInfo($"Average party size: {signups.Average(s => s.PartySize):#.##}");
            var largestParty = signups.OrderByDescending(s => s.PartySize).First();
            Console.WriteInfo($"Largest: {largestParty.Name}, party of {largestParty.PartySize}");
        }

        public void ListLogsPerSignup()
        {
            var id = Console.GetUserInput("Id of the signup  (leave empty to show all):");

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
                Console.WriteSuccess($"Signup with {nameof(Signup.Id)} '{id}' not found.");
                return;
            }

            ListLogsPerSignup(signup);
        }

        private void ListLogsOfAllSignups()
        {
            var signups = _signupService.GetIncludingLogs();

            if (!signups.Any())
            {
                Console.WriteInfo("No signups found.");
                return;
            }

            Console.WriteInfo("Signups with logs:");

            foreach (var signup in signups)
            {
                ListLogsPerSignup(signup);
            }
        }

        private static void ListLogsPerSignup(Signup signup)
        {
            Console.WriteSuccess($"Id {signup.Id} ({signup.Name})");

            if (signup.Logs.Any())
            {
                foreach (var log in signup.Logs)
                    Console.WriteInfo($"  {log}");
            }
            else
            {
                Console.WriteInfo("  [No logs]");
            }
        }

        public void TestStuff()
        {
            _signupService.TestStuff();
            Console.WriteInfo("Stuff has been tested");
        }

        public void CheckData()
        {
            _signupService.CheckData();
        }

        private static SignupUpsert GetUpdateDto(Signup existing)
        {
            var newName = Console.GetUserInput($"New name (leave empty to keep '{existing.Name}'):");
            var newPhone = Console.GetUserInput($"New phone (leave empty to keep '{existing.PhoneNumber}'):");
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
                string input = Console.GetUserInput(prompt);

                if (string.IsNullOrWhiteSpace(input))
                {
                    if (allowNull)
                        return null;

                    Console.WriteWarning("Input cannot be empty. Please try again.");
                    continue;
                }

                if (int.TryParse(input, out int result))
                    return result;

                Console.WriteWarning("Invalid number. Please try again.");
            }
        }
    }
}
