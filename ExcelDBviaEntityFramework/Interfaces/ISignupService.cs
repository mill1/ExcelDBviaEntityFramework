using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        List<Signup> GetSignups();
        List<Signup> GetSignupsIncludingLogs();
        Signup GetSignupByEFId(string id);
        Signup AddSignup(SignupUpsert insert);
        Signup UpdateSignup(string id, SignupUpsert update);
        bool DeleteSignup(string id);
        void TestStuff();
        void CheckData(bool checkIdUniqueness = true);
    }
}