using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework.Services
{
    /// <summary>
    /// Application service that orchestrates signup operations.  
    /// Wraps <see cref="ISignupRepository"/> to provide higher-level business logic,  
    /// such as coordinating data persistence with logging, validation, and cleanup.  
    /// Classic 'bamiluik' that exposes a clean API without exposing EF Core details.  
    /// </summary>
    public class SignupService : ISignupService
    {
        private readonly ISignupRepository _repo;

        public SignupService(ISignupRepository repository)
        {
            _repo = repository;
        }

        public Signup AddSignup(SignupUpsert insert)
        {
            return _repo.AddSignup(insert);
        }

        public Signup UpdateSignup(string id, SignupUpsert update)
        {
            return _repo.UpdateSignup(id, update);
        }

        public bool DeleteSignup(string id)
        {
            return _repo.DeleteSignup(id);
        }

        public Signup GetSignup(string id)
        {
            return _repo.GetSignup(id);
        }

        public Signup GetSignupIncludingLogs(string id)
        {
            return _repo.GetSignupIncludingLogs(id);
        }

        public List<Signup> GetSignups()
        {
            return _repo.GetSignups();
        }

        public List<Signup> GetSignupsIncludingLogs()
        {
            return _repo.GetSignupsIncludingLogs();
        }

        public void TestStuff()
        {
            // Playground to test stuff.
            _repo.TestStuff();
        }

        public void CheckData()
        {
            _repo.CheckData();
        }
    }   
}
