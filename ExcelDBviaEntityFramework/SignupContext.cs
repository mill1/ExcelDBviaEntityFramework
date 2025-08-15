using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace ExcelDBviaEntityFramework
{
    public class SignupContext : DbContext
    {
        public DbSet<SignupEntry> Signups { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseJet(
                $"""
            Provider = Microsoft.ACE.OLEDB.12.0;
            Data Source = {FileHelper.ResolveExcelPath("Signups.xlsx")};
            Extended Properties = 'Excel 12.0 Xml';
            """);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SignupEntry>()
                .HasKey(s => s.Id_ý);

            modelBuilder.Entity<SignupEntry>()
                .HasQueryFilter(e => !e.Deleted_ý);
        }

        public override int SaveChanges()
        {
            var affectedRows = 0;
            var connection = Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            foreach (var entry in ChangeTracker.Entries<SignupEntry>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        affectedRows += SaveModifications(connection, entry);
                        break;

                    case EntityState.Added:
                        affectedRows += SaveAdditions(connection, entry);
                        break;

                    case EntityState.Deleted:
                        affectedRows += SaveSoftDeletion(connection, entry);
                        break;
                }

                entry.Reload();
            }

            return affectedRows;
        }

        // ---------- Core Actions ----------

        private int SaveSoftDeletion(DbConnection connection, EntityEntry<SignupEntry> entry)
        {
            var sql = "UPDATE [Sheet1$] SET [Deleted_ý] = @deleted WHERE [Id_ý] = @id";
            var parameters = new List<(string, object)>
        {
            ("@deleted", true),
            ("@id", entry.OriginalValues[nameof(SignupEntry.Id_ý)])
        };

            ExecuteCommand(connection, sql, parameters);
            entry.State = EntityState.Detached;
            return 1;
        }

        private int SaveAdditions(DbConnection connection, EntityEntry<SignupEntry> entry)
        {
            if (string.IsNullOrEmpty((string?)entry.CurrentValues[nameof(SignupEntry.Id_ý)]))
                entry.CurrentValues[nameof(SignupEntry.Id_ý)] = Guid.NewGuid().ToString("N").Substring(0, 8);

            entry.CurrentValues[nameof(SignupEntry.Deleted_ý)] = false;

            var (columns, parameters) = BuildParametersFromProperties(entry, includeAll: true);
            var sql = $"INSERT INTO [Sheet1$] ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters.Select(p => p.Name))})";

            ExecuteCommand(connection, sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveModifications(DbConnection connection, EntityEntry<SignupEntry> entry)
        {
            var (setClauses, parameters) = BuildParametersFromProperties(entry, includeAll: false);
            if (!setClauses.Any())
                return 0;

            var sql = $"UPDATE [Sheet1$] SET {string.Join(", ", setClauses)} WHERE [Id_ý] = @id";
            parameters.Add(("@id", entry.OriginalValues[nameof(SignupEntry.Id_ý)]));

            ExecuteCommand(connection, sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        // ---------- Helpers ----------

        private (List<string> ColumnsOrSetClauses, List<(string Name, object Value)> Parameters)
            BuildParametersFromProperties(EntityEntry<SignupEntry> entry, bool includeAll)
        {
            var resultList = new List<string>();
            var parameters = new List<(string Name, object Value)>();
            int index = 0;

            foreach (var prop in typeof(SignupEntry).GetProperties())
            {
                if (!includeAll && prop.Name == nameof(SignupEntry.Id_ý))
                    continue;

                if (!includeAll && !entry.Property(prop.Name).IsModified)
                    continue;

                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                var columnName = columnAttr?.Name ?? prop.Name;
                var paramName = $"@p{index++}";
                var value = entry.CurrentValues[prop.Name] ?? DBNull.Value;

                if (includeAll)
                    resultList.Add($"[{columnName}]");
                else
                    resultList.Add($"[{columnName}] = {paramName}");

                parameters.Add((paramName, value));
            }

            return (resultList, parameters);
        }

        private void ExecuteCommand(DbConnection connection, string sql, List<(string Name, object Value)> parameters)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
        }
    }

}