using ExcelDBviaEntityFramework.Data.Infrastructure;
using System.Data.Common;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IExcelRepositoryFactory
    {
        ExcelDataGateway Create(DbConnection connection);
    }
}
