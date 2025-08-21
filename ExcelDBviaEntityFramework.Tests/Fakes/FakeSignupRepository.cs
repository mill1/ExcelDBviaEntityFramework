using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Tests.Fakes
{
    public class FakeSignupRepository : ISignupRepository
    {
        private readonly List<Signup> _signups;
        private readonly List<Log> _logs;

        public FakeSignupRepository()
        {
            _signups = CreateSignupList();
            _logs = CreateLogList(_signups);
        }

        public Signup AddSignup(SignupUpsert insert)
        {
            return new Signup
            {
                Deleted = false,
                Id = (_signups.Count + 1).ToString(),
                Name = insert.Name,
                PhoneNumber = insert.PhoneNumber,
                PartySize = (int)insert.PartySize,
                Logs = null,
            };
        }

        public Signup UpdateSignup(string id, SignupUpsert update)
        {
            return _signups.FirstOrDefault(s => s.Id == id);
        }

        public bool DeleteSignup(string id)
        {
            return true;
        }

        public Signup GetSignup(string id)
        {
            return _signups.First(s => s.Id == id);
        }

        public Signup GetSignupIncludingLogs(string id)
        {
            throw new NotImplementedException();
        }

        public List<Signup> GetSignups()
        {
            return _signups;
        }

        public List<Signup> GetSignupsIncludingLogs()
        {
            throw new NotImplementedException();
        }

        public void TestStuff()
        {
            throw new NotImplementedException();
        }

        public void CheckData(bool checkIdUniqueness = true)
        {
            throw new NotImplementedException();
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
                    Entry = $"Added signup: {signups.ElementAt(1)}"
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
