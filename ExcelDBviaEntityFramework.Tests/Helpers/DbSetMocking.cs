using Microsoft.EntityFrameworkCore;
using Moq;

namespace ExcelDBviaEntityFramework.Tests.Helpers
{
    public static class DbSetMocking
    {
        public static Mock<DbSet<T>> CreateMockSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(e => data.Remove(e));

            return mockSet;
        }
    }
}
