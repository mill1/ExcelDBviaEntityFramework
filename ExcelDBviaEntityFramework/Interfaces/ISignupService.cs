using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface ISignupService
    {
        Signup AddSignup(string name, string phone, int partySize);
        bool DeleteSignup(string id);
        Signup GetSignupById(string id);
        List<Signup> GetSignups();
        Signup UpdateSignup(string id, string name, string phone, int partySize);
    }
}