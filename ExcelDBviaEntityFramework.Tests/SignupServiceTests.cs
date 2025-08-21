using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Services;
using ExcelDBviaEntityFramework.Tests.Fakes;
using FluentAssertions;

namespace ExcelDBviaEntityFramework.Tests
{
    [TestClass]
    public class SignupServiceTests
    {
        private SignupService _service;
        private FakeSignupRepository _repo;

        [TestInitialize]
        public void Setup()
        {
            _repo = new FakeSignupRepository();
            _service = new SignupService(_repo);
        }

        // TODO other tests

        [TestMethod]
        public void AddSignup_ShouldAddSignup()
        {
            // Arrange
            var insert = new SignupUpsert
            {
                Name = "Garth",
                PhoneNumber = "555-5554",
                PartySize = 2
            };

            // Act
            var result = _service.AddSignup(insert);

            // Assert
            result.Should().BeEquivalentTo(insert);
        }
    }
}
