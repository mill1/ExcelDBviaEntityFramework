using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace ExcelDBviaEntityFramework
{
    public class ExcelDbContext : DbContext
    {
        private const string ExcelFileName = "Signups.xlsx";

        public ExcelDbContext()
        {
            var fullPathExcel = FileHelper.ResolveExcelPath(ExcelFileName);
            if (FileHelper.IsExcelFileInUse(fullPathExcel))
                throw new DBConcurrencyException($"Excel file is currently in use. Please close it.\r\nPath: {fullPathExcel}");
        }

        public DbSet<Signup> Signups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseJet($"""
            Provider=Microsoft.ACE.OLEDB.12.0;
            Data Source={FileHelper.ResolveExcelPath(ExcelFileName)};
            Extended Properties='Excel 12.0 Xml;HDR=YES';
        """);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Signup>().ToTable(Constants.SheetName);
            modelBuilder.Entity<Signup>().HasKey(s => s.Id_ý);
            modelBuilder.Entity<Signup>().HasQueryFilter(e => !e.Deleted_ý);
        }

        public override int SaveChanges()
        {
            int affectedRows = 0;
            var repo = new ExcelRepository(Database.GetDbConnection());

            foreach (var entry in ChangeTracker.Entries<Signup>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        affectedRows += SaveAddition(entry, repo);
                        break;
                    case EntityState.Modified:
                        affectedRows += SaveModification(entry, repo);
                        break;
                    case EntityState.Deleted:
                        affectedRows += SaveSoftDeletion(entry, repo);
                        break;
                }
                entry.Reload();
            }

            return affectedRows;
        }

        private int SaveAddition(EntityEntry<Signup> entry, ExcelRepository repo)
        {
            if (string.IsNullOrEmpty(entry.CurrentValues[nameof(Signup.Id_ý)]?.ToString()))
                entry.CurrentValues[nameof(Signup.Id_ý)] = Guid.NewGuid().ToString("N")[..8];

            entry.CurrentValues[nameof(Signup.Deleted_ý)] = false;

            var (columns, parameters) = repo.BuildParameters(entry.Entity, includeAll: true);
            string sql = $"INSERT INTO [{Constants.SheetName}] ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters.Select(p => p.Name))})";
            repo.Execute(sql, parameters);

            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveModification(EntityEntry<Signup> entry, ExcelRepository repo)
        {
            var modifiedProps = entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToList();
            var (setClauses, parameters) = repo.BuildParameters(entry.Entity, includeAll: false, modifiedProps: modifiedProps);

            if (!setClauses.Any()) return 0;

            parameters.Add(("@id", entry.OriginalValues[nameof(Signup.Id_ý)]));
            string sql = $"UPDATE [{Constants.SheetName}] SET {string.Join(", ", setClauses)} WHERE [{nameof(Signup.Id_ý)}] = @id";

            repo.Execute(sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveSoftDeletion(EntityEntry<Signup> entry, ExcelRepository repo)
        {
            string sql = $"UPDATE [{Constants.SheetName}] SET [Deleted_ý] = @deleted WHERE [{nameof(Signup.Id_ý)}] = @id";
            var parameters = new List<(string, object)>
        {
            ("@deleted", true),
            ("@id", entry.OriginalValues[nameof(Signup.Id_ý)])
        };

            repo.Execute(sql, parameters);
            entry.State = EntityState.Detached;
            return 1;
        }
    }
}