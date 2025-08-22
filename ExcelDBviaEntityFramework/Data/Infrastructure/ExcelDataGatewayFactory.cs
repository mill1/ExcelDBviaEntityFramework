using System.Data.Common;
using ExcelDBviaEntityFramework.Interfaces;

namespace ExcelDBviaEntityFramework.Data.Infrastructure
{
    /// <summary>
    /// Factory for creating <see cref="ExcelDataGateway"/> instances given a DbConnection.
    /// </summary>
    public class ExcelDataGatewayFactory : IExcelRepositoryFactory
    {
        public ExcelDataGateway Create(DbConnection connection)
        {
            return new ExcelDataGateway(connection);
        }
    }
}
