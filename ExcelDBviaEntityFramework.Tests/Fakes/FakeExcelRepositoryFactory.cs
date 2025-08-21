using System.Data.Common;
using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Interfaces;

namespace ExcelDBviaEntityFramework.Tests.Fakes
{
    public class FakeExcelRepositoryFactory : IExcelRepositoryFactory
    {
        public List<string> ExecutedSql { get; } = new();
        public List<(string, object)> LastParameters { get; private set; }

        public IExcelRepository Create(DbConnection connection)
        {
            return new FakeExcelRepository(this);
        }

        ExcelRepository IExcelRepositoryFactory.Create(DbConnection connection)
        {
            throw new NotImplementedException();
        }

        private class FakeExcelRepository : IExcelRepository
        {
            private readonly FakeExcelRepositoryFactory _parent;

            public FakeExcelRepository(FakeExcelRepositoryFactory parent)
            {
                _parent = parent;
            }

            public void Execute(string sql, List<(string, object)> parameters)
            {
                _parent.ExecutedSql.Add(sql);
                _parent.LastParameters = parameters;
            }

            public (List<string>, List<(string, object)>) BuildParameters(
                object entity, bool includeAll, List<string> modifiedProps = null)
            {
                // Just enough to keep SaveChanges from crashing
                // TODO: kan list niet aan toch?
                return (new List<string> { "Id" }, new List<(string, object)> { ("@id", "fake") });
            }

            int IExcelRepository.Execute(string sql, List<(string Name, object Value)> parameters)
            {
                throw new NotImplementedException();
            }

            public (List<string> ColumnsOrSetClauses, List<(string Name, object Value)> Parameters) BuildParameters<T>(T entity, bool includeAll, List<string>? modifiedProps = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
