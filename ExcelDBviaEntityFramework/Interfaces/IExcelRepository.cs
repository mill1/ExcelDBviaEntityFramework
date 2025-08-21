namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IExcelRepository
    {
        (List<string> ColumnsOrSetClauses, List<(string Name, object Value)> Parameters) BuildParameters<T>(T entity, bool includeAll, List<string>? modifiedProps = null);
        int Execute(string sql, List<(string Name, object Value)> parameters);
    }
}