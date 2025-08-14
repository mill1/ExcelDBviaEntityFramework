using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework
{
    // https://www.bricelam.net/2024/03/12/ef-xlsx.html

    public class Program
    {
        static void Main(string[] args)
        {
            RunExcelviaEFCore();
        }

        private static void RunExcelviaEFCore()
        {
            string id = Guid.NewGuid().ToString("N").Substring(0, 8);

            using var db = new SignupContext();

            // Add a new signup
            db.Signups.Add(new SignupEntry
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = "Charlie 3",
                PhoneNumber = "555-1212",
                PartySize = 5
            });

            // Update an existing signup
            var firstSignup = db.Signups.First();
            firstSignup.PhoneNumber = "123-456-7002";

            // Delete a signup
            var signupToDelete = db.Signups.FirstOrDefault(s => s.Id == "a2429330");
            if (signupToDelete != null)
            {
                db.Signups.Remove(signupToDelete);
            }

            db.SaveChanges(); // Inserts, updates AND DELETES in one go

            // Re-read to confirm
            var updatedSignup = db.Signups.FirstOrDefault(s => s.Name == firstSignup.Name);
            Console.WriteLine($"After update: {updatedSignup.Name}, {updatedSignup.PhoneNumber}");
        }
    }
}
