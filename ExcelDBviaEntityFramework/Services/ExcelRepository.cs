using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data;
using System.Reflection;

namespace ExcelDBviaEntityFramework.Services
{
    public class ExcelRepository
    {
        private readonly DbConnection _connection;

        public ExcelRepository(DbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public int Execute(string sql, List<(string Name, object Value)> parameters)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value ?? DBNull.Value;
                cmd.Parameters.Add(p);
            }

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            return cmd.ExecuteNonQuery();
        }

        public (List<string> ColumnsOrSetClauses, List<(string Name, object Value)> Parameters) BuildParameters<T>(T entity, bool includeAll, List<string>? modifiedProps = null)
        {
            var columns = new List<string>();
            var parameters = new List<(string Name, object Value)>();
            int index = 0;

            foreach (var prop in typeof(T).GetProperties())
            {
                // skip Id_ý when updating
                if (!includeAll && prop.Name == "Id_ý")
                    continue;

                if (!includeAll && modifiedProps != null && !modifiedProps.Contains(prop.Name))
                    continue;

                var value = prop.GetValue(entity) ?? DBNull.Value;
                string paramName = $"@p{index++}";

                var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
                string columnName = columnAttr?.Name ?? prop.Name;

                if (includeAll)
                    columns.Add($"[{columnName}]");
                else
                    columns.Add($"[{columnName}] = {paramName}");

                parameters.Add((paramName, value));
            }

            return (columns, parameters);
        }
    }

}
