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
        private readonly IExcelDataGatewayFactory _repoFactory;

        public ExcelDbContext(DbContextOptions<ExcelDbContext> options, IExcelDataGatewayFactory? repoFactory = null)
            : base(options)
        {
            _repoFactory = repoFactory ?? new ExcelDataGatewayFactory();
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
            var repo = _repoFactory.Create(Database.GetDbConnection());

            int affectedRows = 0;
            affectedRows = SaveChangesSignups(repo, affectedRows);
            SaveChangesLogs(repo);

            return affectedRows;
        }

        private int SaveChangesSignups(ExcelDataGateway repo, int affectedRows)
        {
            foreach (var entry in ChangeTracker.Entries<Signup>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        affectedRows += SaveAddition(entry, repo, Constants.SheetNameSignups, nameof(Signup.Id));
                        break;
                    case EntityState.Modified:
                        affectedRows += SaveModification(entry, repo);
                        break;
                    case EntityState.Deleted:
                        affectedRows += SaveSoftDeletion(entry, repo, Constants.SheetNameSignups, nameof(Signup.Id));
                        break;
                }

                entry.Reload();
            }

            return affectedRows;
        }

        private void SaveChangesLogs(ExcelDataGateway repo)
        {
            foreach (var entry in ChangeTracker.Entries<Log>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SaveAddition(entry, repo, Constants.SheetNameLogs, nameof(Log.Id));
                        break;
                    case EntityState.Modified:
                        throw new InvalidOperationException("Not allowed.");
                    case EntityState.Deleted:
                        SaveSoftDeletion(entry, repo, Constants.SheetNameLogs, nameof(Log.Id));
                        break;
                }

                entry.Reload();
            }
        }

        private int SaveAddition<TEntity>(EntityEntry<TEntity> entry, ExcelDataGateway repo, string sheetName, string keyPropertyName)
        where TEntity : class
        {
            // Ensure Id is present
            if (string.IsNullOrEmpty(entry.CurrentValues[keyPropertyName]?.ToString()))
                entry.CurrentValues[keyPropertyName] = Guid.NewGuid().ToString("N")[..8];

            // Always insert with Deleted = false
            entry.CurrentValues[Constants.ColumnNameDeleted] = false;

            // Build INSERT statement
            var (columns, parameters) = repo.BuildParameters(entry.Entity, includeAll: true);
            string sql = $"INSERT INTO [{sheetName}] " +
                         $"({string.Join(", ", columns)}) " +
                         $"VALUES ({string.Join(", ", parameters.Select(p => p.Name))})";

            repo.Execute(sql, parameters);

            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveModification(EntityEntry<Signup> entry, ExcelDataGateway repo)
        {
            var modifiedProps = entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name).ToList();
            var (setClauses, parameters) = repo.BuildParameters(entry.Entity, includeAll: false, modifiedProps: modifiedProps);

            if (!setClauses.Any()) return 0;

            parameters.Add(("@id", entry.OriginalValues[nameof(Signup.Id)]));
            string sql = $"UPDATE [{Constants.SheetNameSignups}] SET {string.Join(", ", setClauses)} WHERE [{nameof(Signup.Id)}] = @id";

            repo.Execute(sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveSoftDeletion<TEntity>(EntityEntry<TEntity> entry, ExcelDataGateway repo, string sheetName, string keyPropertyName)
        where TEntity : class
        {
            string sql = $"UPDATE [{sheetName}] SET [{Constants.ColumnNameDeleted}] = @deleted WHERE [{keyPropertyName}] = @id";
            var parameters = new List<(string, object)>
            {
                ("@deleted", true),
                ("@id", entry.OriginalValues[keyPropertyName])
            };

            repo.Execute(sql, parameters);
            entry.State = EntityState.Detached;

            return 1;
        }
    }
}