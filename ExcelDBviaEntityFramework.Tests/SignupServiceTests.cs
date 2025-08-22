using ExcelDBviaEntityFramework.Data.Stubs;
using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Services;
using FluentAssertions;

namespace ExcelDBviaEntityFramework.Tests
{
    [TestClass]
    public class SignupServiceTests
    {
        private SignupService _service;
        private SignupRepositoryStub _repo;

        [TestInitialize]
        public void Setup()
        {
            _repo = new SignupRepositoryStub();
            _service = new SignupService(_repo);
        }

        [TestMethod]
        public void GetSignup_ShouldReturnSignupWhenExists()
        {
            var result = _service.GetSignup("1");

            result.Should().NotBeNull();
            result.Id.Should().Be("1");
            result.Name.Should().Be("Brice");
        }

        [TestMethod]
        public void GetSignup_ShouldReturnNullIfNotFound()
        {
            var x = _service.GetSignup("0");

            x.Should().BeNull();
        }

        [TestMethod]
        public void GetSignupIncludingLogs_ShouldReturnSignupWithLogs()
        {
            var result = _service.GetSignupIncludingLogs("2");

            result.Should().NotBeNull();
            result.Id.Should().Be("2");
            result.Logs.Should().NotBeNullOrEmpty();
            result.Logs.All(l => l.SignupId == "2").Should().BeTrue();
        }

        [TestMethod]
        public void GetSignups_ShouldReturnAllSignups()
        {
            var result = _service.GetSignups();

            result.Should().NotBeNull();
            result.Count.Should().Be(3); // stub repo seeds 3 signups
        }

        [TestMethod]
        public void GetSignupsIncludingLogs_ShouldReturnSignupsWithLogs()
        {
            var result = _service.GetSignupsIncludingLogs();

            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.All(s => s.Logs != null).Should().BeTrue();
        }

        [TestMethod]
        public void AddSignup_ShouldAddSignup()
        {
            var insert = new SignupUpsert
            {
                Name = "Garth",
                PhoneNumber = "555-5554",
                PartySize = 2
            };

            var result = _service.AddSignup(insert);

            result.Should().NotBeNull();
            result.Name.Should().Be(insert.Name);
            result.PhoneNumber.Should().Be(insert.PhoneNumber);
            result.PartySize.Should().Be(insert.PartySize);
        }

        [TestMethod]
        public void UpdateSignup_ShouldUpdateSignup()
        {
            string id = "1";
            var signup = _repo.GetSignup(id);

            var update = new SignupUpsert
            {
                Name = $"{signup.Name} Updated",
                PhoneNumber = $"{signup.PhoneNumber} Updated",
                PartySize = signup.PartySize + 1,
            };

            var result = _service.UpdateSignup(id, update);

            result.Should().NotBeNull();
            result.Name.Should().Be(update.Name);
            result.PhoneNumber.Should().Be(update.PhoneNumber);
            result.PartySize.Should().Be(update.PartySize);
        }

        [TestMethod]
        public void UpdateSignup_ShouldReturnNullIfNotFound()
        {
            string id = "0"; // non-existing

            var update = new SignupUpsert
            {
                Name = "Some Name",
                PhoneNumber = "Some Phone",
                PartySize = 13,
            };

            var result = _service.UpdateSignup(id, update);

            result.Should().BeNull();
        }

        [TestMethod]
        public void DeleteSignup_ShouldReturnTrue()
        {
            var result = _service.DeleteSignup("1");
            result.Should().BeTrue();
        }

        [TestMethod]
        public void TestStuff_ShouldNotThrow()
        {
            Action act = () => _service.TestStuff();
            act.Should().NotThrow();
        }

        [TestMethod]
        public void CheckData_ShouldNotThrow()
        {
            Action act = () => _service.CheckData();
            act.Should().NotThrow();
        }
    }
}
