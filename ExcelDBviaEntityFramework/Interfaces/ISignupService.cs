using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        Signup GetSignupByEFId(string id);
        Signup AddSignup(SignupInsert insert);
        Signup UpdateSignup(string id, SignupUpdate update);
        bool DeleteSignup(string id);
        List<Signup> GetSignups();
    }
}