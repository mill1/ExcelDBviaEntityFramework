using System.Data.Common;
using ExcelDBviaEntityFramework.Interfaces;

namespace ExcelDBviaEntityFramework.Data
{
    public class ExcelRepositoryFactory : IExcelRepositoryFactory
    {
        public ExcelRepository Create(DbConnection connection)
        {
            return new ExcelRepository(connection);
        }
    }
}
