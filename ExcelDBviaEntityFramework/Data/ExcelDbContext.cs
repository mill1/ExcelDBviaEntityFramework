using ExcelDBviaEntityFramework.Data.Infrastructure;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace ExcelDBviaEntityFramework.Data
{
    /// <summary>
    /// EF Core DbContext that maps <see cref="Signup"/>  and <see cref="Log"/> entities to Excel sheets and overrides SaveChanges to persist changes using <see cref="ExcelDataGateway"/>
    /// </summary>
    public class ExcelDbContext : DbContext
    {
        private IExcelDataGateway? _excelDataGateway;

        public ExcelDbContext(DbContextOptions<ExcelDbContext> options)
            : base(options)
        {
        }

        public DbSet<Signup> Signups { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Signup>().ToTable(Constants.SheetNameSignups);
            modelBuilder.Entity<Signup>().HasKey(s => s.Id);
            modelBuilder.Entity<Signup>().HasQueryFilter(e => !e.Deleted);

            modelBuilder.Entity<Log>().ToTable(Constants.SheetNameLogs);
            modelBuilder.Entity<Log>().HasKey(s => s.Id);
            modelBuilder.Entity<Log>().HasQueryFilter(e => !e.Deleted);
        }

        public override int SaveChanges()
        {
            int affectedRows = SaveChangesSignups();
            SaveChangesLogs();

            return affectedRows;
        }

        public int SaveChanges(IExcelDataGateway? excelDataGateway)
        {
            _excelDataGateway = excelDataGateway;
            return SaveChanges();
        }

        private int SaveChangesSignups()
        {
            int affectedRows = 0;

            foreach (var entry in ChangeTracker.Entries<Signup>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        affectedRows += SaveAddition(entry, Constants.SheetNameSignups, nameof(Signup.Id));
                        break;
                    case EntityState.Modified:
                        affectedRows += SaveModification(entry);
                        break;
                    case EntityState.Deleted:
                        affectedRows += SaveSoftDeletion(entry, Constants.SheetNameSignups, nameof(Signup.Id));
                        break;
                }

                entry.Reload();
            }

            return affectedRows;
        }

        private void SaveChangesLogs()
        {
            foreach (var entry in ChangeTracker.Entries<Log>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SaveAddition(entry, Constants.SheetNameLogs, nameof(Log.Id));
                        break;
                    case EntityState.Modified:
                        throw new InvalidOperationException("Not allowed.");
                    case EntityState.Deleted:
                        SaveSoftDeletion(entry, Constants.SheetNameLogs, nameof(Log.Id));
                        break;
                }

                entry.Reload();
            }
        }

        private int SaveAddition<TEntity>(EntityEntry<TEntity> entry, string sheetName, string keyPropertyName)
        where TEntity : class
        {
            // Ensure Id is present
            if (string.IsNullOrEmpty(entry.CurrentValues[keyPropertyName]?.ToString()))
                entry.CurrentValues[keyPropertyName] = Guid.NewGuid().ToString("N")[..8];

            // Always insert with Deleted = false
            entry.CurrentValues[Constants.ColumnNameDeleted] = false;

            // Build INSERT statement
            var (columns, parameters) = _excelDataGateway.BuildParameters(entry.Entity, includeAll: true);
            string sql = $"INSERT INTO [{sheetName}] " +
                         $"({string.Join(", ", columns)}) " +
                         $"VALUES ({string.Join(", ", parameters.Select(p => p.Name))})";

            _excelDataGateway.Execute(sql, parameters);

            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveModification(EntityEntry<Signup> entry)
        {
            var modifiedProps = entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToList();
            var (setClauses, parameters) = _excelDataGateway.BuildParameters(entry.Entity, includeAll: false, modifiedProps: modifiedProps);

            if (!setClauses.Any()) return 0;

            parameters.Add(("@id", entry.OriginalValues[nameof(Signup.Id)]));
            string sql = $"UPDATE [{Constants.SheetNameSignups}] SET {string.Join(", ", setClauses)} WHERE [{nameof(Signup.Id)}] = @id";

            _excelDataGateway.Execute(sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveSoftDeletion<TEntity>(EntityEntry<TEntity> entry, string sheetName, string keyPropertyName)
        where TEntity : class
        {
            string sql = $"UPDATE [{sheetName}] SET [{Constants.ColumnNameDeleted}] = @deleted WHERE [{keyPropertyName}] = @id";
            var parameters = new List<(string, object)>
            {
                ("@deleted", true),
                ("@id", entry.OriginalValues[keyPropertyName])
            };

            _excelDataGateway.Execute(sql, parameters);
            entry.State = EntityState.Detached;

            return 1;
        }
    }
}