using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupRepository
    {
        List<Signup> GetSignups();
        Signup GetSignup(string id);
        List<Signup> GetSignupsIncludingLogs();
        Signup GetSignupIncludingLogs(string id);
        Signup AddSignup(SignupUpsert insert);
        Signup UpdateSignup(string id, SignupUpsert update);
        bool DeleteSignup(string id);
        void TestStuff();
        void CheckData(bool checkIdUniqueness = true);
    }
}
