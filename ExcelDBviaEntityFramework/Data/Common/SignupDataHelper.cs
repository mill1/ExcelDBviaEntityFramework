using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Data.Common
{
    public static class SignupDataHelper
    {
        public static Signup CreateDummySignup(string newId)
        {
            return new Signup
            {
                Deleted = false,
                Id = newId,
                Name = "Werner",
                PhoneNumber = "555-5554",
                PartySize = 1,
                Logs = null
            };
        }

        public static Log CreateLogEntry(string signupId, string entry)
        {
            return new Log
            {
                Deleted = false,
                Id = Guid.NewGuid().ToString("N")[..8],
                User = Environment.UserName,
                Timestamp = DateTime.UtcNow,
                SignupId = signupId,
                Entry = entry
            };
        }

        public static string GenerateId(List<Signup> signups)
        {

            var max = signups
                .Select(s => int.TryParse(s.Id, out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            return (max + 1).ToString();
        }
    }
}
