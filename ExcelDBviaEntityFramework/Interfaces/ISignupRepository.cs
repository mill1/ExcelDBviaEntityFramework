using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupRepository
    {
        List<Signup> Get();
        Signup GetById(string id);
        List<Signup> GetIncludingLogs();
        Signup GetByIdIncludingLogs(string id);
        void Add(Signup insert);
        Signup Update(string id, SignupUpsert update);
        void Log(Log log);
        bool HasEmptyIntegers();
        bool HasDuplicates();
    }
}
