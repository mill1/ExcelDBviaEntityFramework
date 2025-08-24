using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Moq;

namespace ExcelDBviaEntityFramework.Tests
{
// TODO set laten genereren
    [TestClass]
    public class SignupServiceTests
    {
        private Mock<ISignupRepository> _signupRepoMock = null!;
        //private Mock<IDbContextFactory<ExcelDbContext>> _ctxFactoryMock = null!;
        private SignupService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _signupRepoMock = new Mock<ISignupRepository>();

            _service = new SignupService(_signupRepoMock.Object);
        }

        [TestMethod]
        public void AddSignup_Should_Add_Signup_And_Log()
        {
            // Arrange
            const string Name = "Brice";

            var dto = new SignupUpsert
            {
                Name = Name,
                PhoneNumber = "555-5551",
                PartySize = 3
            };

            // Act
            _service.Add(dto);

            // Assert
            _signupRepoMock.Verify(r => r.Add(It.Is<Signup>(s =>
                s.Name == Name)), Times.Once);

            _signupRepoMock.Verify(r => r.Log(It.Is<Log>(l =>
                l.Entry.Contains("Added signup"))), Times.Once);
        }
    }
}
