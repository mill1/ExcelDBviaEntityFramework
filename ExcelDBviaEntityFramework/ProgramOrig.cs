using ExcelDBviaEntityFramework.Models;

// TODO
// unit tests
// log sheet???
// on close: delete all xl rows physically


namespace ExcelDBviaEntityFramework
{
    // https://www.bricelam.net/2024/03/12/ef-xlsx.html

    public class ProgramOrig
    {
        static void MainOrig(string[] args)
        {
            var fullPathExcel = FileHelper.ResolveExcelPath("Signups.xlsx");

            if (FileHelper.IsExcelFileInUse(fullPathExcel))
            {
                Console.WriteLine("Excel file is currently in use. Please close it and try again.");
                return;
            }

            new Runner().Run();
        }

        private static void Run()
        {
            using var db = new ExcelDbContext();

            // Add a new signup
            db.Signups.Add(new SignupEntry
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = "Emiel",
                PhoneNumber = "555-1212",
                PartySize = 2
            });

            // Update
            var firstSignup = db.Signups.First();
            Console.WriteLine($"Before update: {firstSignup.Name}, {firstSignup.PhoneNumber}");
            
            firstSignup.PhoneNumber = "555-" + new Random().Next(1, 10000).ToString("D4");

            // Additions
            var additions = new List<SignupEntry>
            {
                new SignupEntry
                {
                    Id_ý = Guid.NewGuid().ToString("N")[..8],
                    Deleted_ý = false,
                    Name = "Robin",
                    PhoneNumber = "555-1313",
                    PartySize = 6
                }
            };

            db.AddRange(additions);

            // Deletions
            var deletes = db.Signups.Where(x => x.Name.Contains("David"));
            db.Signups.RemoveRange(deletes);

            db.SaveChanges(); // Inserts, updates AND deletes in one go

            var updatedSignup = db.Signups.FirstOrDefault(s => s.Name == firstSignup.Name);
            Console.WriteLine($"After update: {updatedSignup?.Name}, {updatedSignup?.PhoneNumber}");
            Console.WriteLine($"Count: {db.Signups.Count()}");
            Console.WriteLine($"Avg party size: {db.Signups.Average(se => se.PartySize)}");
        }
    }
}
