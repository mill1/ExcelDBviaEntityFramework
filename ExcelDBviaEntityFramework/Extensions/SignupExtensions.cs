using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Extensions
{
    public static class SignupExtensions
    {
        public static List<Signup> IncludeLogs(this List<Signup> signups, List<Log> logs)
        {
            var logsBySignup = logs.GroupBy(l => l.SignupId)
                                   .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var s in signups)
            {
                if (logsBySignup.TryGetValue(s.Id, out var signupLogs))
                    s.Logs = signupLogs;
            }

            return signups;
        }
    }
}