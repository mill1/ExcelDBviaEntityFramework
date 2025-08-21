using ExcelDBviaEntityFramework.Data;
using System.Data.Common;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IExcelRepositoryFactory
    {
        ExcelRepository Create(DbConnection connection);
    }
}
