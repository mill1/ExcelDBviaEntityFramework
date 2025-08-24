using ExcelDBviaEntityFramework;
using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using System.Collections.Generic;

public class SignupService : ISignupService
{
    private readonly ISignupRepository _signupRepository;

    public SignupService(ISignupRepository signupRepository)
    {
        _signupRepository = signupRepository;
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
            PartySize = (int)insert.PartySize,
            Logs = new List<Log>()
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
        throw new NotImplementedException();
    }

    public void TestStuff()
    {
        throw new NotImplementedException();
    }



    public void CheckData()
    {
        var _filePath = FileHelper.ResolveExcelPath(Constants.ExcelFileName);
        FileHelper.EnsureFileNotLocked(_filePath);

        if (Get().Count == 0)
            return;

        if(_signupRepository.HasEmptyIntegers())
            throw new SignupException($"Empty signup id(s) and/or party size(s) found! Fix this in Excel.");

        if(_signupRepository.HasDuplicates())
            throw new SignupException($"Duplicate signup id(s) found! Fix this in Excel.");
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

        var max = signups
            .Select(s => int.TryParse(s.Id, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return (max + 1).ToString();
    }
}
