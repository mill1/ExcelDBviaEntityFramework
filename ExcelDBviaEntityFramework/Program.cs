using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework
{
    public class Program
    {
        static void Main(string[] args)
        {
            RunDemo();
        }

        private static async void RunDemo()
        {
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
