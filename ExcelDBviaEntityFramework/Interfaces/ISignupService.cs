using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        Signup GetSignupByEFId(string id);
        Signup AddSignup(SignupUpsert insert);
        Signup UpdateSignup(string id, SignupUpsert update);
        bool DeleteSignup(string id);
        List<Signup> GetSignups();
    }
}