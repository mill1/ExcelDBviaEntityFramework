using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        Signup AddSignup(SignupUpsert upsert);
        bool DeleteSignup(string id);
        Signup GetSignupById(string id);
        List<Signup> GetSignups();
        Signup UpdateSignup(string id, SignupUpsert upsert);
    }
}