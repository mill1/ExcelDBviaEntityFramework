
using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework.Services
{
    public class SignupService : ISignupService
    {
        private readonly IDbContextFactory<ExcelDbContext> _dbContextFactory;

        public SignupService(IDbContextFactory<ExcelDbContext> dbContextFactory)
        {
            // Use a db context factory to make sure that the DbContext is disposed after the request.
            // Using a scoped service somehow keeps the Excel file locked, which prevents write actions by ClosedXML.
            _dbContextFactory = dbContextFactory;
        }

        public Signup GetSignupByEFId(string id)
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                // Use AsNoTracking for read-only queries to improve performance
                return ctx.Signups.AsNoTracking().FirstOrDefault(s => s.Id == id);
            }
        }

        public Signup AddSignup(SignupUpsert insert)
        {
            var newSignup = new Signup
            {                
                Deleted_ý = false,
                Id = GenerateId(),
                Name = insert.Name,
                PhoneNumber = insert.PhoneNumber,
                PartySize = (int)insert.PartySize
            };

            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                ctx.Signups.Add(newSignup);
                ctx.SaveChanges();
            }

            return newSignup;
        }

        public Signup UpdateSignup(string id, SignupUpsert update)
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signup = ctx.Signups.FirstOrDefault(s => s.Id == id);

                if (signup == null)
                    return null;

                if (!string.IsNullOrWhiteSpace(update.Name))
                    signup.Name = update.Name;

                if (!string.IsNullOrWhiteSpace(update.PhoneNumber))
                    signup.PhoneNumber = update.PhoneNumber;

                if (update.PartySize.HasValue)
                    signup.PartySize = update.PartySize.Value;

                ctx.SaveChanges();
            
                return signup;
            }
        }

        public bool DeleteSignup(string id)
        {
            bool deleted = false;

            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signup = ctx.Signups.FirstOrDefault(s => s.Id == id);

                if (signup == null)
                    return false;

                ctx.Signups.Remove(signup);
                ctx.SaveChanges();
                deleted = true;
            }

            if (deleted)
            {
                Thread.Sleep(350); // Ensure the file is not locked
                ExcelHelper.RemoveDeletedRow(id);
            }

            return deleted;
        }

        public List<Signup> GetSignups()
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                return [.. ctx.Signups];
            }          
        }

        private string GenerateId()
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var lastSignup = ctx.Signups.AsNoTracking().OrderByDescending(s => s.Id).FirstOrDefault();

                // If there are no signups, return "1" as the first ID
                if (lastSignup == null)
                    return "1";

                bool allIntegers = ctx.Signups
                    .AsEnumerable()
                    .All(s => int.TryParse(s.Id, out _));

                if (allIntegers)
                    return (int.Parse(lastSignup.Id) + 1).ToString();

                return $"{lastSignup.Id}b";
            }
        }

        public void CheckData(bool checkIdUniqueness=true)
        {
            var _filePath = FileHelper.ResolveExcelPath(Constants.ExcelFileName);
            FileHelper.EnsureFileNotLocked(_filePath);

            if (checkIdUniqueness)
            {
                using (var ctx = _dbContextFactory.CreateDbContext())
                {
                    var ids = ctx.Signups.Select(s => s.Id).ToList();
                    var duplicates = ids.GroupBy(id => id)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

                    if (duplicates.Any())
                    {
                        throw new SignupException($"Duplicate Signup ID's found: {string.Join(", ", duplicates)}. Fix this in Excel.");
                    }
                }
            }
        }
    }
}