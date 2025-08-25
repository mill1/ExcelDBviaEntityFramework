using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Extensions;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repository for managing <see cref="Signup"/> entities in an Excel-backed EF Core context.
public class SignupRepository : ISignupRepository
{
    private readonly IDbContextFactory<ExcelDbContext> _dbContextFactory;

    public SignupRepository(IDbContextFactory<ExcelDbContext> contextFactory)
    {
        // Use a db context factory to make sure that the DbContext is disposed after the request.
        // Using a scoped db service somehow keeps the Excel file locked, which prevents subsequent CRUD operations.
        _dbContextFactory = contextFactory;
    }

    public List<Signup> Get()
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        return [.. ctx.Signups];
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
        ctx.SaveChangesWithGateway();
    }

    public Signup Update(string id, SignupUpsert update)
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        var signup = ctx.Signups.Single(s => s.Id == id);

        signup.Name = update.Name;
        signup.PhoneNumber = update.PhoneNumber;
        signup.PartySize = (int)update.PartySize;
        ctx.SaveChangesWithGateway();

        return signup;
    }

    public bool Delete(string id)
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        var signup = ctx.Signups.FirstOrDefault(s => s.Id == id);

        if (signup == null)
            return false;

        ctx.Signups.Remove(signup);

        // Remove all logs related to this signup ('Cascade delete')
        var logs = ctx.Logs.Where(l => l.SignupId == id).ToList();
        ctx.Logs.RemoveRange(logs);
        ctx.SaveChangesWithGateway();

        return true;
    }

    public void Log(Log log)
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        ctx.Logs.Add(log);
        ctx.SaveChangesWithGateway();
    }

    public void Log(List<Log> logs)
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        ctx.Logs.AddRange(logs);
        ctx.SaveChangesWithGateway();
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