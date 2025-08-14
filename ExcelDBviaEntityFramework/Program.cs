using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework
{
    // https://www.bricelam.net/2024/03/12/ef-xlsx.html

    public class Program
    {
        static void Main(string[] args)
        {
            var fullPathExcel = FileResolver.ResolveExcelPath("Signups.xlsx");

            if (FileResolver.IsExcelFileInUse(fullPathExcel))
            {
                Console.WriteLine("Excel file is currently in use. Please close it and try again.");
                return;
            }

            RunExcelviaEFCore();
        }

        private static void RunExcelviaEFCore()
        {
            using var db = new SignupContext();

            // Add a new signup
            db.Signups.Add(new SignupEntry
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = "emiel",
                PhoneNumber = "555-1212",
                PartySize = 5
            });

            // Update an existing signup
            var firstSignup = db.Signups.First();
            firstSignup.PhoneNumber = "123-456-7003";

            // Delete a signup(s)
            //var signupToDelete = db.Signups.FirstOrDefault(s => s.Id == "a2429330");
            //if (signupToDelete != null)
            //    db.Signups.Remove(signupToDelete);

            var additions = new List<SignupEntry>
            {
    new SignupEntry
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Name = "robin 1",
                    PhoneNumber = "555-a",
                    PartySize = 5
                },
    new SignupEntry
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Name = "robin 2",
                    PhoneNumber = "555-b",
                    PartySize = 5
                }
            };

            db.AddRange(additions);

            var deletes = db.Signups.Where(x => x.Name.Contains("Charlie"));
            db.Signups.RemoveRange(deletes);

            db.SaveChanges(); // Inserts, updates AND DELETES in one go

            // Re-read to confirm
            var updatedSignup = db.Signups.FirstOrDefault(s => s.Name == firstSignup.Name);
            Console.WriteLine($"After update: {updatedSignup.Name}, {updatedSignup.PhoneNumber}");
        }
    }
}
