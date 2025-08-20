using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        Signup GetSignup(string id);
        List<Signup> GetSignups();
        Signup GetSignupIncludingLogs(string id);
        List<Signup> GetSignupsIncludingLogs();
        Signup AddSignup(SignupUpsert insert);
        Signup UpdateSignup(string id, SignupUpsert update);
        bool DeleteSignup(string id);
        void TestStuff();
        void CheckData(bool checkIdUniqueness = true);
    }
}