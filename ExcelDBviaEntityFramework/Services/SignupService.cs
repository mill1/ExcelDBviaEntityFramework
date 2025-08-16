
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Services
{
    public class SignupService : ISignupService
    {
        private readonly ExcelDbContext _db;

        public SignupService(ExcelDbContext db)
        {
            _db = db;
        }

        public Signup GetSignupById(string id)
        {
            return _db.Signups.FirstOrDefault(s => s.Id_ý == id);
        }

        public Signup AddSignup(string name, string phone, int partySize)
        {
            var newSignup = new Signup
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = name,
                PhoneNumber = phone,
                PartySize = partySize
            };

            _db.Signups.Add(newSignup);
            _db.SaveChanges();

            return newSignup;
        }

        public Signup UpdateSignup(string id, string name, string phone, int partySize)
        {
            var signup = _db.Signups.FirstOrDefault(s => s.Id_ý == id);

            if (signup == null)
                return null;

            signup.Name = name;
            signup.PhoneNumber = phone;
            signup.PartySize = partySize;

            _db.SaveChanges();

            return signup;
        }

        public bool DeleteSignup(string id)
        {
            var signup = _db.Signups.Where(s => s.Id_ý == id).FirstOrDefault();

            if (signup == null)
                return false;

            _db.Signups.Remove(signup);
            _db.SaveChanges();

            return true;
        }

        public List<Signup> GetSignups()
        {
            return [.. _db.Signups];
        }
    }
}