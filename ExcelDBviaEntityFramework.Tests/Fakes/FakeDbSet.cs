using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace ExcelDBviaEntityFramework.Tests.Fakes
{
    public class TestDbSet<T> : DbSet<T>, IQueryable<T> where T : class
    {
        private readonly List<T> _data = new();

        public new T Add(T entity)  // hide EF Core’s Add
        {
            _data.Add(entity);
            return entity;
        }

        public new T Remove(T entity)
        {
            _data.Remove(entity);
            return entity;
        }

        public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _data.GetEnumerator();

        public Type ElementType => typeof(T);
        public Expression Expression => _data.AsQueryable().Expression;

        public List<T> LocalList => _data;

        public override IEntityType EntityType => throw new NotImplementedException();
    }
}
