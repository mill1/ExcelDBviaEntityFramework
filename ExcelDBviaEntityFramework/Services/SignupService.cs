
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

        public Signup GetSignupByEFId(string id)
        {
            return _db.Signups.FirstOrDefault(s => s.Id_ý == id);
        }

        public Signup AddSignup(SignupInsert insert)
        {
            var newSignup = new Signup
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Id = GenerateId(),
                Name = insert.Name,
                PhoneNumber = insert.PhoneNumber,
                PartySize = (int)insert.PartySize
            };

            _db.Signups.Add(newSignup);
            _db.SaveChanges();

            return newSignup;
        }

        public Signup UpdateSignup(string id, SignupUpdate update)
        {
            var signup = _db.Signups.FirstOrDefault(s => s.Id_ý == id);

            if (signup == null)
                return null;

            if (!string.IsNullOrWhiteSpace(update.Id))
                signup.Id = update.Id;

            if (!string.IsNullOrWhiteSpace(update.Name))
                signup.Name = update.Name;

            if (!string.IsNullOrWhiteSpace(update.PhoneNumber))
                signup.PhoneNumber = update.PhoneNumber;

            if (update.PartySize.HasValue)
                signup.PartySize = update.PartySize.Value;

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
                Thread.Sleep(100); // Ensure the file is not locked
                ExcelHelper.RemoveDeletedRow(id);
            }

            return deleted;
        }

        public List<Signup> GetSignups()
        {
            return [.. _db.Signups];
        }

        private string GenerateId()
        {
            var lastSignup = _db.Signups.OrderByDescending(s => s.Id).FirstOrDefault();

            // If there are no signups, return "1" as the first ID
            if (lastSignup == null)
                return "1";

            bool allIntegers = _db.Signups
                .AsEnumerable()
                .All(s => int.TryParse(s.Id, out _));

            if (allIntegers)
                return (int.Parse(lastSignup.Id) + 1).ToString();

            return $"{lastSignup.Id}b";
        }
    }
}