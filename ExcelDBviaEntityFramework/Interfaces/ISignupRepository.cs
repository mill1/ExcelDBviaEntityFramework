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
        bool Delete(string id);
        void Log(Log log);
        void Log(List<Log> logs);
        bool HasEmptyIntegers();
        bool HasDuplicates();
    }
}
