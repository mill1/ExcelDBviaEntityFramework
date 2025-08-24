using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Extensions;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

public class SignupRepository : ISignupRepository
{
    private readonly IDbContextFactory<ExcelDbContext> _dbContextFactory;

    public SignupRepository(IDbContextFactory<ExcelDbContext> contextFactory)
    {
        _dbContextFactory = contextFactory;
    }

    public List<Signup> Get()
    {
        using (var ctx = _dbContextFactory.CreateDbContext())
        {
            return [.. ctx.Signups];
        }
    }

    public Signup? GetById(string id)
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        return ctx.Signups.FirstOrDefault(s => s.Id == id);
    }

    public List<Signup> GetIncludingLogs()
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        var signups = ctx.Signups.AsNoTracking().ToList();
        var logs = ctx.Logs.AsNoTracking().ToList();

        return signups.IncludeLogs(logs);
    }

    public Signup GetByIdIncludingLogs(string id)
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        var signup = ctx.Signups.FirstOrDefault(s => s.Id == id);

        if (signup == null)
            return null;

        return signup.IncludeLogs(ctx.Logs.AsNoTracking().ToList());
    }

    public void Add(Signup insert)
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        ctx.Signups.Add(insert);

        ctx.SaveChanges();
    }

    public Signup Update(string id, SignupUpsert update)
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        var signup = ctx.Signups.Single(s => s.Id == id);

        signup.Name = update.Name;
        signup.PhoneNumber = update.PhoneNumber;
        signup.PartySize = (int)update.PartySize;

        ctx.SaveChanges();

        return signup;
    }

    public void Log(Log log)
    {
        using var ctx = _dbContextFactory.CreateDbContext();
        ctx.Logs.Add(log);

        ctx.SaveChanges();
    }

    public bool HasDuplicates()
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        var ids = ctx.Signups.Select(s => s.Id).ToList();
        var duplicates = ids.GroupBy(id => id)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

        return duplicates.Any();
    }

    public bool HasEmptyIntegers()
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        try
        {
            ctx.Signups.AsEnumerable().Any(x => string.IsNullOrEmpty(x.Id));
            return false;
        }
        catch (InvalidCastException)
        {
            return true;
        }
    }
}