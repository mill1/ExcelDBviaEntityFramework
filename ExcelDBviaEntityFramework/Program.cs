using ExcelDBviaEntityFramework.Models;

namespace ExcelDBviaEntityFramework
{
    class Program
    {
        static void Main()
        {
            // Example operations
            AddSignup("Alice", "555-1234", 2);
            AddSignup( "Bob", "555-9876", 4);
            PrintSignups();
        }

        static void AddSignup(string name, string phone, double partySize)
        {
            using var db = new ExcelDbContext();
            db.Signups.Add(new SignupEntry
            {
                Id_ý = Guid.NewGuid().ToString("N")[..8],
                Deleted_ý = false,
                Name = name,
                PhoneNumber = phone,
                PartySize = partySize
            });
            db.SaveChanges();
        }

        static void PrintSignups()
        {
            using var db = new ExcelDbContext();
            foreach (var s in db.Signups)
            {
                Console.WriteLine($"{s.Id_ý}: {s.Name} ({s.PartySize})");
            }
        }
    }
}
