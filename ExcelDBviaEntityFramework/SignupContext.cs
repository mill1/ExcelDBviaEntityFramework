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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SignupEntry>()
                .HasKey(s => s.Id);

            // Ignore "deleted" rows
            modelBuilder.Entity<SignupEntry>()
                .HasQueryFilter(s => !string.IsNullOrEmpty(s.Id));
        }

        public override int SaveChanges()
        {
            var affectedRows = 0;
            var connection = Database.GetDbConnection();

            // Make sure connection is open
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            // Process all tracked SignupEntry changes
            foreach (var entry in ChangeTracker.Entries<SignupEntry>().ToList())
            {
                // ---- UPDATE ----
                if (entry.State == EntityState.Modified)
                {
                    var setClauses = new List<string>();
                    var parameters = new List<(string Name, object Value)>();
                    int paramIndex = 0;

                    foreach (var prop in typeof(SignupEntry).GetProperties())
                    {
                        if (prop.Name == nameof(SignupEntry.Id))
                            continue;

                        if (!entry.Property(prop.Name).IsModified)
                            continue;

                        var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                        var columnName = columnAttr?.Name ?? prop.Name;

                        string paramName = $"@p{paramIndex++}";
                        setClauses.Add($"[{columnName}] = {paramName}");
                        parameters.Add((paramName, entry.CurrentValues[prop.Name] ?? DBNull.Value));
                    }

                    if (setClauses.Count > 0)
                    {
                        var sql = $"UPDATE [Sheet1$] SET {string.Join(", ", setClauses)} WHERE [Id] = @id";
                        using var cmd = connection.CreateCommand();
                        cmd.CommandText = sql;

                        foreach (var (Name, Value) in parameters)
                        {
                            var param = cmd.CreateParameter();
                            param.ParameterName = Name;
                            param.Value = Value;
                            cmd.Parameters.Add(param);
                        }

                        var idParam = cmd.CreateParameter();
                        idParam.ParameterName = "@id";
                        idParam.Value = entry.OriginalValues[nameof(SignupEntry.Id)];
                        cmd.Parameters.Add(idParam);

                        affectedRows += cmd.ExecuteNonQuery();
                    }

                    entry.State = EntityState.Unchanged;
                }

                // ---- INSERT ----
                else if (entry.State == EntityState.Added)
                {
                    if (string.IsNullOrEmpty((string?)entry.CurrentValues[nameof(SignupEntry.Id)]))
                        entry.CurrentValues[nameof(SignupEntry.Id)] = Guid.NewGuid().ToString("N").Substring(0, 8);

                    var columnNames = new List<string>();
                    var paramNames = new List<string>();
                    var parameters = new List<(string Name, object Value)>();
                    int paramIndex = 0;

                    foreach (var prop in typeof(SignupEntry).GetProperties())
                    {
                        var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                        var columnName = columnAttr?.Name ?? prop.Name;

                        columnNames.Add($"[{columnName}]");
                        string paramName = $"@p{paramIndex++}";
                        paramNames.Add(paramName);
                        parameters.Add((paramName, entry.CurrentValues[prop.Name] ?? DBNull.Value));
                    }

                    var sql = $"INSERT INTO [Sheet1$] ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";
                    using var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = sql;

                    foreach (var (Name, Value) in parameters)
                    {
                        var param = insertCmd.CreateParameter();
                        param.ParameterName = Name;
                        param.Value = Value;
                        insertCmd.Parameters.Add(param);
                    }

                    affectedRows += insertCmd.ExecuteNonQuery();
                    entry.State = EntityState.Unchanged;
                }

                // ---- SOFT DELETE ----
                else if (entry.State == EntityState.Deleted)
                {
                    var setClauses = new List<string>();
                    var parameters = new List<(string Name, object Value)>();
                    int paramIndex = 0;

                    foreach (var prop in typeof(SignupEntry).GetProperties())
                    {
                        if (prop.Name == nameof(SignupEntry.Id))
                            continue;

                        var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                        var columnName = columnAttr?.Name ?? prop.Name;

                        string paramName = $"@p{paramIndex++}";
                        setClauses.Add($"[{columnName}] = {paramName}");

                        if (prop.PropertyType == typeof(string))
                            parameters.Add((paramName, "")); // empty string for text
                        else if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(double) || prop.PropertyType == typeof(double))
                            parameters.Add((paramName, 0));  // 0 for numbers
                        else
                            parameters.Add((paramName, DBNull.Value));
                    }


                    var sql = $"UPDATE [Sheet1$] SET {string.Join(", ", setClauses)} WHERE [Id] = @id";
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = sql;

                    foreach (var (Name, Value) in parameters)
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = Name;
                        param.Value = Value;
                        cmd.Parameters.Add(param);
                    }

                    var idParam = cmd.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = entry.OriginalValues[nameof(SignupEntry.Id)];
                    cmd.Parameters.Add(idParam);

                    affectedRows += cmd.ExecuteNonQuery();
                    entry.State = EntityState.Detached;
                }
            }

            return affectedRows;
        }



        private string ResolvePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Excel files");
        }

        private string ResolvePath2()
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assemblyLocation == null)
                throw new InvalidOperationException("Could not determine assembly location.");
            return Path.Combine(assemblyLocation, "Data");

        }
    }
}