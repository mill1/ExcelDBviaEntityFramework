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
            var changedSignups = ChangeTracker.Entries<SignupEntry>()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added)
                .ToList();

            int affectedRows = 0;

            foreach (var entry in changedSignups)
            {
                var connection = Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                if (entry.State == EntityState.Modified)
                {
                    var setClauses = new List<string>();
                    var parameters = new List<(string Name, object Value)>();
                    int paramIndex = 0;

                    foreach (var prop in typeof(SignupEntry).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (prop.Name == nameof(SignupEntry.Id))
                            continue; // never update the primary key

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

                    if (setClauses.Count > 0)
                    {
                        var idParamName = $"@p{paramIndex++}";
                        parameters.Add((idParamName, entry.OriginalValues[nameof(SignupEntry.Id)] ?? DBNull.Value));

                        var sql = $"UPDATE [Sheet1$] SET {string.Join(", ", setClauses)} WHERE [Id] = {idParamName}";

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

                            affectedRows += updateCmd.ExecuteNonQuery();
                        }

                        entry.Reload();
                    }

                    entry.State = EntityState.Unchanged;
                }
                else if (entry.State == EntityState.Added)
                {
                    // Assign a new ID if not set
                    if (string.IsNullOrWhiteSpace(entry.CurrentValues[nameof(SignupEntry.Id)]?.ToString()))
                    {
                        entry.CurrentValues[nameof(SignupEntry.Id)] =
                            Guid.NewGuid().ToString("N").Substring(0, 8);
                    }

                    var columns = new List<string>();
                    var placeholders = new List<string>();
                    var parameters = new List<(string Name, object Value)>();
                    int paramIndex = 0;

                    foreach (var prop in typeof(SignupEntry).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var value = entry.CurrentValues[prop.Name];
                        var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                        var columnName = columnAttr?.Name ?? prop.Name;

                        string paramName = $"@p{paramIndex++}";
                        columns.Add($"[{columnName}]");
                        placeholders.Add(paramName);
                        parameters.Add((paramName, value ?? DBNull.Value));
                    }

                    var sql = $"INSERT INTO [Sheet1$] ({string.Join(", ", columns)}) VALUES ({string.Join(", ", placeholders)})";

                    using (var insertCmd = connection.CreateCommand())
                    {
                        insertCmd.CommandText = sql;
                        foreach (var (Name, Value) in parameters)
                        {
                            var param = insertCmd.CreateParameter();
                            param.ParameterName = Name;
                            param.Value = Value;
                            insertCmd.Parameters.Add(param);
                        }

                        affectedRows += insertCmd.ExecuteNonQuery();
                    }

                    entry.State = EntityState.Unchanged;
                }
            }

            return affectedRows;
        }
    }
}