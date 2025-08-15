
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Services
{
    public class SignupService
    {
        public Signup GetSignupById(string id)
        {           
            using var db = new ExcelDbContext();

            return db.Signups.FirstOrDefault(s => s.Id_ý == id);
        }

        public Signup AddSignup(string name, string phone, int partySize)
        {
            using var db = new ExcelDbContext();

            var newSignup = new Signup
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = name,
                PhoneNumber = phone,
                PartySize = partySize
            };

            db.Signups.Add(newSignup);
            db.SaveChanges();

            return newSignup;
        }

        public Signup UpdateSignup(string id, string name, string phone, int partySize)
        {
            using var db = new ExcelDbContext();

            var signup = db.Signups.FirstOrDefault(s => s.Id_ý == id);

            if (signup == null)
                return null;

            signup.Name = name;
            signup.PhoneNumber = phone;
            signup.PartySize = partySize;

            db.SaveChanges();

            return signup;
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

        public List<Signup> GetSignups()
        {
            using var db = new ExcelDbContext();

            return db.Signups.ToList();
        }        
    }
}