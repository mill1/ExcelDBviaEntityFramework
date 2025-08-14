using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework
{
    public class Program
    {
        static void Main(string[] args)
        {
            RunExcelviaEFCore();
        }

        private static void RunExcelviaEFCore()
        {
            using var db = new SignupContext();

            db.Database.OpenConnection();
            var connection = db.Database.GetDbConnection();

            // Read the first signup
            var firstSignup = db.Signups.FirstOrDefault();
            Console.WriteLine($"Before update: {firstSignup.Name}, {firstSignup.PhoneNumber}");

            firstSignup.PhoneNumber = "123-456-7893";
            db.SaveChanges(); // Now actually updates Excel

            // Re-read to confirm
            var updatedSignup = db.Signups.FirstOrDefault(s => s.Name == firstSignup.Name);
            Console.WriteLine($"After update: {updatedSignup.Name}, {updatedSignup.PhoneNumber}");
        }


        private static void RunExcelviaEFCoreOrig()
        {
            // https://www.bricelam.net/2024/03/12/ef-xlsx.html

            using var db = new SignupContext();

            db.Database.OpenConnection();
            var connection = db.Database.GetDbConnection();

            using var tables = connection.GetSchema("Tables");
            foreach (DataRow table in tables.Rows)
            {
                var tableName = (string)table["TABLE_NAME"];
                Console.WriteLine(tableName);

                var command = connection.CreateCommand();
                command.CommandType = CommandType.TableDirect;
                command.CommandText = tableName;
                using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

                using var columns = reader.GetSchemaTable();
                foreach (DataRow column in columns.Rows)
                {
                    Console.WriteLine($"    {column["DataType"]} {column["ColumnName"]}");
                }

                var partyCount = db.Signups.Count();
                Console.WriteLine($"Parties: {partyCount}");

                var averagePartySize = db.Signups.Average(s => s.PartySize);
                Console.WriteLine($"Average size: {averagePartySize}");

                var largestParty = db.Signups.OrderByDescending(s => s.PartySize).First();
                Console.WriteLine($"Largest: {largestParty.Name}, party of {largestParty.PartySize}");
            }
        }
    }
}
