
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Services
{
    public class SignupService
    {
        public SignupEntry GetSignUpById(string id)
        {
            using var db = new ExcelDbContext();

            return db.SignUps.FirstOrDefault(s => s.Id_ý == id);
        }

        public SignupEntry GetSignUpByName(string name)
        {
            using var db = new ExcelDbContext();

            return db.SignUps.FirstOrDefault(s => s.Name == name);
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

            db.SignUps.Add(newSignUp);
            db.SaveChanges();

            return newSignUp;
        }

        public SignupEntry UpdateSignup(string id, string name, string phone, int partySize)
        {
            using var db = new ExcelDbContext();

            var signUp = db.SignUps.FirstOrDefault(s => s.Id_ý == id);

            if (signUp == null)
                return null;

            signUp.Name = name;
            signUp.PhoneNumber = phone;
            signUp.PartySize = partySize;

            db.SaveChanges();

            return db.SignUps.First(s => s.Id_ý == id);
        }

        public bool DeleteSignup(string id)
        {
            using var db = new ExcelDbContext();

            var signup = db.SignUps.Where(s => s.Id_ý == id).FirstOrDefault();    

            if (signup == null)
                return false;

            db.SignUps.Remove(signup);
            db.SaveChanges();

            return true;
        }

        public List<SignupEntry> GetSignups()
        {
            using var db = new ExcelDbContext();

            return db.SignUps.ToList();
        }        

        private static string GenerateRandomPhoneNumber()
        {
            return "555-" + new Random().Next(1, 10000).ToString("D4");
        }
    }
}