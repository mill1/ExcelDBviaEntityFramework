
using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework.Services
{
    public class SignupService : ISignupService
    {
        private readonly ExcelDbContext _db;
        private readonly IDbContextFactory<ExcelDbContext> _dbContextFactory;

        public SignupService(ExcelDbContext db, IDbContextFactory<ExcelDbContext> dbContextFactory)
        {
            _db = db;
            _dbContextFactory = dbContextFactory;
        }

        public Signup GetSignupById(string id)
        {
            return _db.Signups.FirstOrDefault(s => s.Id_ý == id);
        }

        public Signup AddSignup(SignupUpsert upsert)
        {
            var newSignup = new Signup
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = upsert.Name,
                PhoneNumber = upsert.PhoneNumber,
                PartySize = (int)upsert.PartySize
            };

            _db.Signups.Add(newSignup);
            _db.SaveChanges();

            return newSignup;
        }

        public Signup UpdateSignup(string id, SignupUpsert upsert)
        {
            var signup = _db.Signups.FirstOrDefault(s => s.Id_ý == id);

            if (signup == null)
                return null;

            if (!string.IsNullOrWhiteSpace(upsert.Name))
                signup.Name = upsert.Name;

            if (!string.IsNullOrWhiteSpace(upsert.PhoneNumber))
                signup.PhoneNumber = upsert.PhoneNumber;

            if (upsert.PartySize.HasValue)
                signup.PartySize = upsert.PartySize.Value;

            _db.SaveChanges();
            return signup;
        }

        public bool DeleteSignup(string id)
        {
            bool deleted = false;
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signup = ctx.Signups.FirstOrDefault(s => s.Id_ý == id);

                if (signup == null)
                    return false;

                ctx.Signups.Remove(signup);
                ctx.SaveChanges();
                deleted = true;
            }

            if (deleted)
            {
                ExcelHelper.RemoveDeletedRows(id);
            }

            return deleted;
        }

        public List<Signup> GetSignups()
        {
            return [.. _db.Signups];
        }
    }
}