using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelDBviaEntityFramework
{
    public class SignupContext : DbContext
    {
        public DbSet<SignupEntry> Signups { get; set; }

        // TODO rel. path resource

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseJet(
                """
            Provider = Microsoft.ACE.OLEDB.12.0;
            Data Source = [rel. path]\Signups.xlsx;
            Extended Properties = 'Excel 12.0 Xml';
            """);
    }
}
