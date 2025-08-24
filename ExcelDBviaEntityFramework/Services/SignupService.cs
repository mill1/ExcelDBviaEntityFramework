using ExcelDBviaEntityFramework;
using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

/// <summary>
/// Application service that orchestrates signup operations.  
/// </summary>
public class SignupService : ISignupService
{
    private readonly ISignupRepository _signupRepository;
    private readonly IFileService _fileService;

    public SignupService(ISignupRepository signupRepository, IFileService fileService)
    {
        _signupRepository = signupRepository;
        _fileService = fileService;
    }

    public Signup GetById(string id)
    {
        return _signupRepository.GetById(id);
    }

    public Signup GetByIdIncludingLogs(string id)
    {
        return _signupRepository.GetByIdIncludingLogs(id);
    }

    public List<Signup> Get()
    {
        return _signupRepository.Get();
    }

    public List<Signup> GetIncludingLogs()
    {
        return _signupRepository.GetIncludingLogs();
    }

    public void Add(SignupUpsert insert)
    {
        // Business logic ✅
        var newId = GenerateId(Get());

        var signup = new Signup
        {
            Deleted = false,
            Id = newId,
            Name = insert.Name,
            PhoneNumber = insert.PhoneNumber,
            PartySize = (int)insert.PartySize
        };

        // Persistence delegation ✅
        _signupRepository.Add(signup);
        _signupRepository.Log(CreateLog(newId, $"Added signup: {insert}"));
    }

    public Signup Update(Signup existing, SignupUpsert update)
    {
        if (string.IsNullOrWhiteSpace(update.Name))
            update.Name = existing.Name;

        if (string.IsNullOrWhiteSpace(update.PhoneNumber))
            update.PhoneNumber = existing.PhoneNumber;

        if (!update.PartySize.HasValue)
            update.PartySize = existing.PartySize;

        var signup = _signupRepository.Update(existing.Id, update);
        _signupRepository.Log(CreateLog(existing.Id, $"Updated signup: {update}"));

        return signup;
    }

    public bool Delete(string id)
    {
        return _signupRepository.Delete(id);
    }

    public void TestStuff()
    {
        // Playground to test stuff.
        // Currently: bulk inserts to test cascading delete

        var signup = CreateSignup(new SignupUpsert { Name = "Werner", PhoneNumber = "555-5554", PartySize = 1 });

        var logs = new List<Log>
        {
            CreateLog(signup.Id, $"Added signup: {signup}"),
            CreateLog(signup.Id, $"Updated signup: some update"),
            CreateLog(signup.Id, $"Updated signup: another update"),
        };

        _signupRepository.Add(signup);
        _signupRepository.Log(logs);
    }

    public void CheckData()
    {
        var _filePath = _fileService.ResolveExcelPath(Constants.ExcelFileName);
        _fileService.EnsureFileNotLocked(_filePath);

        if (Get().Count == 0)
            return;

        if(_signupRepository.HasEmptyIntegers())
            throw new SignupException("Empty signup id(s) and/or party size(s) found! Fix this in Excel.");

        if(_signupRepository.HasDuplicates())
            throw new SignupException("Duplicate signup id(s) found! Fix this in Excel.");
    }

    private Signup CreateSignup(SignupUpsert insert)
    {
        return new Signup
        {
            Deleted = false,
            Id = GenerateId(Get()),
            Name = insert.Name,
            PhoneNumber = insert.PhoneNumber,
            PartySize = (int)insert.PartySize
        };
    }

    private static Log CreateLog(string signupId, string entry)
    {
        return new Log
        {
            Deleted = false,
            Id = Guid.NewGuid().ToString("N")[..8],
            User = Environment.UserName,
            Timestamp = DateTime.Now,
            SignupId = signupId,
            Entry = entry
        };
    }

    private static string GenerateId(List<Signup> signups)
    {
        if(signups == null || signups.Count == 0)
            return "1";

        var max = signups
            .Select(s => int.TryParse(s.Id, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return (max + 1).ToString();
    }
}
