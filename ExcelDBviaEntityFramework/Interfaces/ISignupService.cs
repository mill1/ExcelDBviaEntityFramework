using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        List<Signup> Get();
        Signup GetById(string id);
        List<Signup> GetIncludingLogs();
        Signup GetByIdIncludingLogs(string id);
        void Add(SignupUpsert insert);
        Signup Update(Signup existing, SignupUpsert update);
        bool Delete(string id);
        void TestStuff();
        void CheckData();
    }
}