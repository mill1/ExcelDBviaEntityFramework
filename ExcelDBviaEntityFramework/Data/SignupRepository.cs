using ExcelDBviaEntityFramework.Data.Common;
using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Extensions;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework.Data
{
    public class SignupRepository : ISignupRepository
    {
        private readonly IDbContextFactory<ExcelDbContext> _dbContextFactory;

        public SignupRepository(IDbContextFactory<ExcelDbContext> dbContextFactory)
        {
            // Use a db context factory to make sure that the DbContext is disposed after the request.
            // Using a scoped db service somehow keeps the Excel file locked, which prevents subsequent CRUD operations.
            _dbContextFactory = dbContextFactory;
        }

        public List<Signup> GetSignups()
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                return [.. ctx.Signups];
            }
        }

        public Signup GetSignup(string id)
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                return ctx.Signups.AsNoTracking().FirstOrDefault(s => s.Id == id);
            }
        }

        public List<Signup> GetSignupsIncludingLogs()
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signups = ctx.Signups.AsNoTracking().ToList();
                var logs = ctx.Logs.AsNoTracking().ToList();

                return signups.IncludeLogs(logs);
            }
        }

        public Signup GetSignupIncludingLogs(string id)
        {
            var signup = GetSignup(id);

            if (signup == null)
                return null;

            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                return signup.IncludeLogs(ctx.Logs.AsNoTracking().ToList());
            }
        }

        public Signup AddSignup(SignupUpsert insert)
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signups = ctx.Signups.ToList(); // materialize into memory.

                var newSignup = new Signup
                {
                    Deleted = false,
                    Id = DataHelper.GenerateId(signups),
                    Name = insert.Name,
                    PhoneNumber = insert.PhoneNumber,
                    PartySize = (int)insert.PartySize
                };

                ctx.Signups.Add(newSignup);

                Log log = DataHelper.CreateLogEntry(newSignup.Id, $"Added signup: {insert}");
                ctx.Logs.Add(log);

                ctx.SaveChanges();

                return newSignup;
            }
        }

        public Signup UpdateSignup(string id, SignupUpsert update)
        {
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signup = ctx.Signups.Single(s => s.Id == id);

                if (!string.IsNullOrWhiteSpace(update.Name))
                    signup.Name = update.Name;

                if (!string.IsNullOrWhiteSpace(update.PhoneNumber))
                    signup.PhoneNumber = update.PhoneNumber;

                if (update.PartySize.HasValue)
                    signup.PartySize = update.PartySize.Value;

                Log log = DataHelper.CreateLogEntry(id, $"Updated signup: {update}");
                ctx.Logs.Add(log);

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

                // Not found, nothing to delete
                if (signup == null)
                    return false;

                ctx.Signups.Remove(signup);

                // Remove all logs related to this signup ('Cascade delete')
                var logs = ctx.Logs.Where(l => l.SignupId == id).ToList();
                ctx.Logs.RemoveRange(logs);

                ctx.SaveChanges();
                deleted = true;
            }

            return deleted;
        }

        public void TestStuff()
        {
            // Playground to test stuff.
            // Currently: bulk inserts to test cascading delete

            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                var signups = ctx.Signups.ToList();
                var newSignup = DataHelper.CreateDummySignup(DataHelper.GenerateId(signups));

                var logs = new List<Log>
                {
                    DataHelper.CreateLogEntry(newSignup.Id, $"Added signup: {newSignup}"),
                    DataHelper.CreateLogEntry(newSignup.Id, $"Updated signup: some update"),
                    DataHelper.CreateLogEntry(newSignup.Id, $"Updated signup: another update"),
                };

                ctx.Signups.Add(newSignup);
                ctx.Logs.AddRange(logs);

                ctx.SaveChanges();
            }
        }

        public void CheckData(bool checkIdUniqueness = true)
        {
            var _filePath = FileHelper.ResolveExcelPath(Constants.ExcelFileName);
            FileHelper.EnsureFileNotLocked(_filePath);

            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                // Always use client-side evaluation so EF doesn’t try to translate the LINQ query
                // to SQL, which causes the "could not find the object '.Dual'"-error
                if (!ctx.Signups.AsEnumerable().Any())
                    return;

                try
                {
                    ctx.Signups.AsEnumerable().Any(x => string.IsNullOrEmpty(x.Id));
                }
                catch (InvalidCastException)
                {
                    throw new SignupException($"Empty Signup ID(s) and/or party size(s) found! Fix this in Excel.");
                }

                if (checkIdUniqueness)
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