using ExcelDBviaEntityFramework.Models;

// TODO
// refactor
// unit tests
// log sheet
// on close: delete all xl rows physically


namespace ExcelDBviaEntityFramework
{
    // https://www.bricelam.net/2024/03/12/ef-xlsx.html

    public class Program
    {
        static void Main(string[] args)
        {
            var fullPathExcel = FileHelper.ResolveExcelPath("Signups.xlsx");

            if (FileHelper.IsExcelFileInUse(fullPathExcel))
            {
                Console.WriteLine("Excel file is currently in use. Please close it and try again.");
                return;
            }

            Run();
        }

        private static void Run()
        {
            using var db = new SignupContext();

            // Add a new signup
            db.Signups.Add(new SignupEntry
            {
                Id_ý = Guid.NewGuid().ToString("N").Substring(0, 8),
                Deleted_ý = false,
                Name = "emiel",
                PhoneNumber = "555-1212",
                PartySize = 5
            });

            // Update an existing signup
            var firstSignup = db.Signups.First();
            firstSignup.PhoneNumber = "123-456-7005";

            var additions = new List<SignupEntry>
            {
                new SignupEntry
                {
                    Id_ý = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Deleted_ý = false,
                    Name = "robin 1",
                    PhoneNumber = "555-a",
                    PartySize = 5
                }
            };

            db.AddRange(additions);

            var deletes = db.Signups.Where(x => x.Name.Contains("David"));
            db.Signups.RemoveRange(deletes);

            db.SaveChanges(); // Inserts, updates AND deletes in one go

            // Re-read to confirm
            var updatedSignup = db.Signups.FirstOrDefault(s => s.Name == firstSignup.Name);
            Console.WriteLine($"After update: {updatedSignup.Name}, {updatedSignup.PhoneNumber}");
        }
    }
}
