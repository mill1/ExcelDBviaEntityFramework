
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Services
{
    public class SignupService
    {
        public SignupEntry GetSignUpByName(string name)
        {
            using var db = new ExcelDbContext();

            return db.Signups.FirstOrDefault(s => s.Name == name);
        }

        public SignupEntry AddSignup(string name, string phone, int partySize)
        {
            using var db = new ExcelDbContext();

            var newSignUp = new SignupEntry
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = name,
                PhoneNumber = phone,
                PartySize = partySize
            };

            db.Signups.Add(newSignUp);
            db.SaveChanges();

            return newSignUp;
        }

        public bool DeleteSignup(string id)
        {
            using var db = new ExcelDbContext();

            var signup = db.Signups.Where(s => s.Id_ý == id).FirstOrDefault();    

            if (signup == null)
                return false;

            db.Signups.Remove(signup);
            db.SaveChanges();

            return true;
        }

        public List<SignupEntry> GetSignups()
        {
            using var db = new ExcelDbContext();

            return db.Signups.ToList();
        }

        public void CRUDSignUps(string nameOfEntriesToDelete)
        {
            using var db = new ExcelDbContext();

            // Read & Update
            var firstSignup = db.Signups.First();
            firstSignup.PhoneNumber = GenerateRandomPhoneNumber();

            // Add a new signup
            db.Signups.Add(new SignupEntry
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = "Emiel",
                PhoneNumber = GenerateRandomPhoneNumber(),
                PartySize = 2
            });

            // Additions
            var additions = new List<SignupEntry>
            {
                new SignupEntry
                {
                    Id_ý = Guid.NewGuid().ToString("N")[..8],
                    Deleted_ý = false,
                    Name = "Robin",
                    PhoneNumber = GenerateRandomPhoneNumber(),
                    PartySize = 6
                },
                new SignupEntry
                {
                    Id_ý = Guid.NewGuid().ToString("N")[..8],
                    Deleted_ý = false,
                    Name = "Laurens",
                    PhoneNumber = GenerateRandomPhoneNumber(),
                    PartySize = 1
                }
            };

            db.AddRange(additions);

            // Deletions
            var deletes = db.Signups.Where(x => x.Name.ToLower() == nameOfEntriesToDelete.ToLower());
            db.Signups.RemoveRange(deletes);

            db.SaveChanges(); // Inserts, updates AND deletes in one go
        }

        private static string GenerateRandomPhoneNumber()
        {
            return "555-" + new Random().Next(1, 10000).ToString("D4");
        }
    }
}