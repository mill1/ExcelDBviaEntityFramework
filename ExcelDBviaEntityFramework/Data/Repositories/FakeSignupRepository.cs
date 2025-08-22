using ExcelDBviaEntityFramework.Data.Common;
using ExcelDBviaEntityFramework.Extensions;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Data.Repositories
{
    /// <summary>
    /// An in-memory fake implementation of <see cref="ISignupRepository"/> for unit tests. Seeds dummy signups/logs and supports CRUD.
    /// </summary>
    public class FakeSignupRepository : ISignupRepository
    {
        private readonly List<Signup> _signups;
        private readonly List<Log> _logs;

        public FakeSignupRepository()
        {
            _signups = CreateSignupList();
            _logs = CreateLogList(_signups);
        }

        public List<Signup> GetSignups()
        {
            return _signups;
        }

        public Signup GetSignup(string id)
        {
            return _signups.FirstOrDefault(s => s.Id == id);
        }

        public List<Signup> GetSignupsIncludingLogs()
        {
            return _signups.IncludeLogs(_logs);
        }

        public Signup GetSignupIncludingLogs(string id)
        {
            var signup = GetSignup(id);

            if (signup == null)
                return null;

            return signup.IncludeLogs(_logs);
        }

        public Signup AddSignup(SignupUpsert insert)
        {
            var newSignup =  new Signup
            {
                Deleted = false,
                Id = SignupDataHelper.GenerateId(_signups),
                Name = insert.Name,
                PhoneNumber = insert.PhoneNumber,
                PartySize = (int)insert.PartySize,
                Logs = null,
            };

            _signups.Add(newSignup);

            Log log = SignupDataHelper.CreateLogEntry(newSignup.Id, $"Added signup: {insert}");
            _logs.Add(log);

            return newSignup;
        }

        public Signup UpdateSignup(string id, SignupUpsert update)
        {
            // Update the signup in the list
            var index = _signups.FindIndex(s => s.Id == id);

            if (index < 0) // -1 == not found
            {
                throw new KeyNotFoundException($"Signup with ID {id} not found.");
            }

            var existingSignup = _signups[index];

            var signup = new Signup
            {
                Deleted = false,
                Id = id,
                Name = update.Name ?? existingSignup.Name,
                PhoneNumber = update.PhoneNumber ?? existingSignup.PhoneNumber,
                PartySize = update.PartySize.HasValue ? (int)update.PartySize : existingSignup.PartySize,
                Logs = null
            };

            _signups[index] = signup;

            Log log = SignupDataHelper.CreateLogEntry(id, $"Updated signup: {update}");
            _logs.Add(log);

            return signup;
        }

        public bool DeleteSignup(string id)
        {
            bool deleted = false;

            var signup = _signups.FirstOrDefault(s => s.Id == id);

            // Not found, nothing to delete
            if (signup == null)
                return false;

            _signups.Remove(signup);

            // Remove all logs related to this signup ('Cascade delete')
            var logsBySignup = _logs.Where(l => l.SignupId == id).ToList();

            if (logsBySignup.Any())
            {
                foreach (var log in logsBySignup)
                {
                    _logs.Remove(log);
                }
                deleted = true;
            }

            return deleted;
        }       

        public void TestStuff()
        {
            var newSignup = SignupDataHelper.CreateDummySignup(SignupDataHelper.GenerateId(_signups));

            var logs = new List<Log>
                {
                    SignupDataHelper.CreateLogEntry(newSignup.Id, $"Added signup: {newSignup}"),
                    SignupDataHelper.CreateLogEntry(newSignup.Id, $"Updated signup: some update"),
                    SignupDataHelper.CreateLogEntry(newSignup.Id, $"Updated signup: another update"),
                };

            _signups.Add(newSignup);
            _logs.AddRange(logs);
        }

        public void CheckData(bool checkIdUniqueness = true)
        {
            // checked
        }

        private static List<Signup> CreateSignupList()
        {
            return new List<Signup>
            {
                new Signup
                {
                    Deleted = false,
                    Id = "1",
                    Name = "Brice",
                    PhoneNumber = "555-5551",
                    PartySize = 3,
                    Logs = null,
                },
                new Signup
                {
                    Deleted = false,
                    Id = "2",
                    Name = "Ryan",
                    PhoneNumber = "555-5552",
                    PartySize = 4,
                    Logs = null,
                },
                new Signup
                {
                    Deleted = false,
                    Id = "3",
                    Name = "David",
                    PhoneNumber = "555-5553",
                    PartySize = 2,
                    Logs = null,
                }
            };
        }

        private static List<Log> CreateLogList(List<Signup> signups)
        {
            return new List<Log>
            {
                new Log
                {
                    Deleted = false,
                    Id = "ea3bdcd5",
                    User = "John.Doe",
                    Timestamp = DateTime.Parse("19-8-2025 13:21:48"),
                    SignupId = signups.ElementAt(0).Id,
                    Entry = $"Added signup: {signups.ElementAt(0)}"
                },
                new Log
                {
                    Deleted = false,
                    Id = "334273x0",
                    User = "Michael.Smith",
                    Timestamp = DateTime.Parse("19-8-2025 15:25:22"),
                    SignupId = signups.ElementAt(1).Id,
                    Entry = $"Added signup: {signups.ElementAt(1)}"
                },
                 new Log
                {
                    Deleted = false,
                    Id = "8187ebee",
                    User = "Michael.Smith",
                    Timestamp = DateTime.Parse("20-8-2025 10:32:29"),
                    SignupId = signups.ElementAt(1).Id,
                    Entry = "Updated signup: Name: [unchanged], Phone: 555-5552, Party Size: [unchanged]"
                },
                  new Log
                {
                    Deleted = false,
                    Id = "25y48527",
                    User = "John.Doe",
                    Timestamp = DateTime.Parse("20-8-2025 11:34:06"),
                    SignupId = signups.ElementAt(2).Id,
                    Entry = $"Added signup: {signups.ElementAt(2)}"
                }
            }; 
        }
    }
}
