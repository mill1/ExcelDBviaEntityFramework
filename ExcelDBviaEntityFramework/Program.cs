using System;
using System.Data;
using System.Linq;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework
{
    public class Program
    {
        static void Main(string[] args)
        {
            // RunDemo();
            RunExcelviaEFCore();
        }

        private static void RunExcelviaEFCore()
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

        private static async void RunDemo()
        {
            // https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli

            using var db = new BloggingContext();

            // Note: This sample requires the database to be created before running.
            Console.WriteLine($"Database path: {db.DbPath}.");

            // Create
            Console.WriteLine("Inserting a new blog...");
            db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
            await db.SaveChangesAsync();

            var blog = await PrintBlog(db);

            // Update
            Console.WriteLine("Updating the blog and adding a post...");
            blog.Url = "https://devblogs.microsoft.com/dotnet";
            blog.Posts.Add(
                new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
            await db.SaveChangesAsync();

            blog = await PrintBlog(db);

            // Delete
            Console.WriteLine("Delete the blog...");
            db.Remove(blog);
            await db.SaveChangesAsync();

            await PrintBlog(db);
        }

        private static async Task<Blog> PrintBlog(BloggingContext db)
        {
            var blog = await db.Blogs
                .OrderBy(b => b.BlogId)
                .FirstOrDefaultAsync();

            Console.ForegroundColor = ConsoleColor.Green;

            if (blog == null)
                Console.WriteLine("No blogs found");
            else
                Console.WriteLine(blog);

            Console.ResetColor();

            return blog;
        }
    }
}
