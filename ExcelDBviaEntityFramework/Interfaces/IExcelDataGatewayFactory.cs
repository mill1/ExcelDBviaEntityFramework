using ExcelDBviaEntityFramework.Data.Infrastructure;
using System.Data.Common;

namespace ExcelDBviaEntityFramework.Interfaces
{
    public interface IExcelDataGatewayFactory
    {
        ExcelDataGateway Create(DbConnection connection);
    }
}
