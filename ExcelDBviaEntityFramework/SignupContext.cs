using ExcelDBviaEntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
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
                Data Source = {ResolvePath()}\Signups.xlsx;
                Extended Properties = 'Excel 12.0 Xml';
                """);

        private string ResolvePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Excel files");
        }

        public override int SaveChanges()
        {
            var modifiedSignups = ChangeTracker.Entries<SignupEntry>()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in modifiedSignups)
            {
                // Build SET clause dynamically
                var setClauses = new List<string>();
                var parameters = new List<(string Name, object Value)>();
                int paramIndex = 0;

                foreach (var prop in typeof(SignupEntry).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var originalValue = entry.OriginalValues[prop.Name];
                    var currentValue = entry.CurrentValues[prop.Name];

                    if (!Equals(originalValue, currentValue))
                    {
                        var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                        var columnName = columnAttr?.Name ?? prop.Name;

                        string paramName = $"@p{paramIndex++}";
                        setClauses.Add($"[{columnName}] = {paramName}");
                        parameters.Add((paramName, currentValue ?? DBNull.Value));
                    }
                }

                if (setClauses.Count == 0)
                {
                    entry.State = EntityState.Unchanged;
                    continue;
                }

                // Use Name as the key
                var nameParamName = $"@p{paramIndex++}";
                parameters.Add((nameParamName, entry.OriginalValues[nameof(SignupEntry.Name)] ?? DBNull.Value));

                var sql = $"UPDATE [Sheet1$] SET {string.Join(", ", setClauses)} WHERE [Name] = {nameParamName}";

                var connection = Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var updateCmd = connection.CreateCommand())
                {
                    updateCmd.CommandText = sql;

                    foreach (var (Name, Value) in parameters)
                    {
                        var param = updateCmd.CreateParameter();
                        param.ParameterName = Name;
                        param.Value = Value;
                        updateCmd.Parameters.Add(param);
                    }

                    updateCmd.ExecuteNonQuery();
                }

                // Now reload the entity from Excel so in-memory object matches the file
                entry.Reload();
            }

            return modifiedSignups.Count;
        }
    }
}