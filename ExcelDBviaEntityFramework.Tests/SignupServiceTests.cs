using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ExcelDBviaEntityFramework.Tests
{
    [TestClass]
    public class SignupServiceTests
    {
        private IDbContextFactory<ExcelDbContext> _dbContextFactory;
        private SignupService _service;

        [TestInitialize]
        public void Setup()
        {
            // Use a fresh in-memory DB for each test
            var options = new DbContextOptionsBuilder<ExcelDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            _dbContextFactory = new PooledDbContextFactory<ExcelDbContext>(options);
            _service = new SignupService(_dbContextFactory);
        }

        [TestMethod]
        public void AddSignup_Should_Add_Signup_And_Log()
        {
            // Arrange
            var insert = new SignupUpsert
            {
                Name = "John Doe",
                PhoneNumber = "123-456",
                PartySize = 4
            };

            // Act
            var newSignup = _service.AddSignup(insert);

            // Assert
            using var ctx = _dbContextFactory.CreateDbContext();

            var signup = ctx.Signups.FirstOrDefault(s => s.Id == newSignup.Id);
            Assert.IsNotNull(signup);
            Assert.AreEqual("John Doe", signup.Name);

            var logs = ctx.Logs.Where(l => l.SignupId == newSignup.Id).ToList();
            Assert.AreEqual(1, logs.Count);
            StringAssert.Contains(logs[0].Entry, "Added signup");
        }

        [TestMethod]
        public void UpdateSignup_Should_Update_And_Log()
        {
            // Arrange
            var signup = new Signup { Id = "1", Name = "Alice", PhoneNumber = "000", PartySize = 2 };
            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                ctx.Signups.Add(signup);
                ctx.SaveChanges();
            }

            var update = new SignupUpsert { Name = "Alice Updated", PartySize = 5 };

            // Act
            var updated = _service.UpdateSignup("1", update);

            // Assert
            Assert.IsNotNull(updated);
            Assert.AreEqual("Alice Updated", updated.Name);
            Assert.AreEqual(5, updated.PartySize);

            using var ctx2 = _dbContextFactory.CreateDbContext();
            var logs = ctx2.Logs.Where(l => l.SignupId == "1").ToList();
            Assert.AreEqual(1, logs.Count);
            StringAssert.Contains(logs[0].Entry, "Updated signup");
        }

        [TestMethod]
        public void DeleteSignup_Should_Remove_Signup_And_Logs()
        {
            // Arrange
            var signup = new Signup { Id = "1", Name = "Bob", PhoneNumber = "555", PartySize = 3 };
            var logs = new List<Log>
            {
                new Log { Id = "log1", SignupId = "1", User = "Test", Entry = "Initial log", Timestamp = System.DateTime.UtcNow }
            };

            using (var ctx = _dbContextFactory.CreateDbContext())
            {
                ctx.Signups.Add(signup);
                ctx.Logs.AddRange(logs);
                ctx.SaveChanges();
            }

            // Act
            var result = _service.DeleteSignup("1");

            // Assert
            Assert.IsTrue(result);

            using var ctx2 = _dbContextFactory.CreateDbContext();
            Assert.AreEqual(0, ctx2.Signups.Count());
            Assert.AreEqual(0, ctx2.Logs.Count());
        }
    }
}
