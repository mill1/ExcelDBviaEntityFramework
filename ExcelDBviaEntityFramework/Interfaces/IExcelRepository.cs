namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IExcelRepository
    {
        int Execute(string sql, List<(string Name, object Value)> parameters);

        (List<string> ColumnsOrSetClauses, List<(string Name, object Value)> Parameters)
            BuildParameters<T>(T entity, bool includeAll, List<string>? modifiedProps = null);
    }
}