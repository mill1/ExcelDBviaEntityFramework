using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework.Tests.Fakes
{
    public class FakeExcelDbContext : ExcelDbContext
    {
        public FakeExcelDbContext() : base(new DbContextOptions<ExcelDbContext>()) { }

        public override DbSet<Signup> Signups { get; set; } = new TestDbSet<Signup>();
        public override DbSet<Log> Logs { get; set; } = new TestDbSet<Log>();

        public override int SaveChanges()
        {
            // No real DB — just pretend everything was saved.
            return 0;
        }
    }
}
