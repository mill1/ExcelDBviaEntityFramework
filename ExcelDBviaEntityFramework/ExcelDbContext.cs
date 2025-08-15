using ExcelDBviaEntityFramework.Helpers;
using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;

namespace ExcelDBviaEntityFramework
{
    public class ExcelDbContext : DbContext
    {
        private const string ExcelFileName = "Signups.xlsx";

        public ExcelDbContext()
        {
            var fullPathExcel = FileHelper.ResolveExcelPath(ExcelFileName);

            if (FileHelper.IsExcelFileInUse(fullPathExcel))
            {
                throw new DBConcurrencyException($"Excel file is currently in use. Please close it and try again.\r\nPath: {fullPathExcel}");                
            }
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

            foreach (var entry in ChangeTracker.Entries<Signup>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        affectedRows += SaveModifications(entry);
                        break;
                    case EntityState.Added:
                        affectedRows += SaveAdditions(entry);
                        break;
                    case EntityState.Deleted:
                        affectedRows += SaveSoftDeletion(entry);
                        break;
                }

                entry.Reload();
            }

            return affectedRows;
        }

        // ---------- Core Actions ----------

        private int SaveAdditions(EntityEntry<Signup> entry)
        {
            if (string.IsNullOrEmpty((string?)entry.CurrentValues[nameof(Signup.Id_ý)]))
                entry.CurrentValues[nameof(Signup.Id_ý)] = Guid.NewGuid().ToString("N")[..8];

            entry.CurrentValues[nameof(Signup.Deleted_ý)] = false;

            var (columns, parameters) = BuildParametersFromProperties(entry, includeAll: true);
            string sql = $"INSERT INTO [Sheet1$] ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters.Select(p => p.Name))})";

            ExecuteCommand(sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveModifications(EntityEntry<Signup> entry)
        {
            var (setClauses, parameters) = BuildParametersFromProperties(entry, includeAll: false);
            if (!setClauses.Any())
                return 0;

            parameters.Add(("@id", entry.OriginalValues[nameof(Signup.Id_ý)]));
            string sql = $"UPDATE [Sheet1$] SET {string.Join(", ", setClauses)} WHERE [{nameof(Signup.Id_ý)}] = @id";

            ExecuteCommand(sql, parameters);
            entry.State = EntityState.Unchanged;
            return 1;
        }

        private int SaveSoftDeletion(EntityEntry<Signup> entry)
        {
            string sql = $"UPDATE [Sheet1$] SET [{nameof(Signup.Deleted_ý)}] = @deleted WHERE [{nameof(Signup.Id_ý)}] = @id";
            var parameters = new List<(string, object)>
        {
            ("@deleted", true),
            ("@id", entry.OriginalValues[nameof(Signup.Id_ý)])
        };

            ExecuteCommand(sql, parameters);
            entry.State = EntityState.Detached;
            return 1;
        }

        // ---------- Helpers ----------

        private (List<string> ColumnsOrSetClauses, List<(string Name, object Value)> Parameters)
            BuildParametersFromProperties(EntityEntry<Signup> entry, bool includeAll)
        {
            var resultList = new List<string>();
            var parameters = new List<(string Name, object Value)>();
            int index = 0;

            foreach (var prop in typeof(Signup).GetProperties())
            {
                if (!includeAll && prop.Name == nameof(Signup.Id_ý))
                    continue;

                if (!includeAll && !entry.Property(prop.Name).IsModified)
                    continue;

                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                string columnName = columnAttr?.Name ?? prop.Name;
                string paramName = $"@p{index++}";
                object value = entry.CurrentValues[prop.Name] ?? DBNull.Value;

                if (includeAll)
                    resultList.Add($"[{columnName}]");
                else
                    resultList.Add($"[{columnName}] = {paramName}");

                parameters.Add((paramName, value));
            }

            return (resultList, parameters);
        }

        private void ExecuteCommand(string sql, List<(string Name, object Value)> parameters)
        {
            using var cmd = Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            if (cmd.Connection?.State != ConnectionState.Open)
                cmd.Connection?.Open();

            cmd.ExecuteNonQuery();
        }
    }
}