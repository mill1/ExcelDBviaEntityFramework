using ExcelDBviaEntityFramework.Exceptions;
using ExcelDBviaEntityFramework.Interfaces;
using ExcelDBviaEntityFramework.Models;
using Moq;

namespace ExcelDBviaEntityFramework.Tests
{
    [TestClass]
    public class SignupServiceTests
    {
        private Mock<ISignupRepository> _signupRepoMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private SignupService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _signupRepoMock = new Mock<ISignupRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _service = new SignupService(_signupRepoMock.Object, _fileServiceMock.Object);
        }

        [TestMethod]
        public void Get_ShouldReturnSignups()
        {
            // Arrange
            var expected = new List<Signup> { new Signup { Id = "1", Name = "Test" } };
            _signupRepoMock.Setup(r => r.Get()).Returns(expected);

            // Act
            var result = _service.Get();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetById_ShouldReturnSignup()
        {
            // Arrange
            var expected = new Signup { Id = "1", Name = "Bob" };
            _signupRepoMock.Setup(r => r.GetById("1")).Returns(expected);

            // Act
            var result = _service.GetById("1");

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetById_ShouldReturnNull_WhenSignupNotFound()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.GetById(It.IsAny<string>())).Returns((Signup?)null);

            // Act
            var result = _service.GetById("0");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetIncludingLogs_ShouldReturnSignupsWithLogs()
        {
            // Arrange
            var expected = new List<Signup> { new Signup { Id = "1", Name = "Alice" } };
            _signupRepoMock.Setup(r => r.GetIncludingLogs()).Returns(expected);

            // Act
            var result = _service.GetIncludingLogs();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetByIdIncludingLogs_ShouldReturnSignupWithLogs()
        {
            // Arrange
            var expected = new Signup { Id = "1", Name = "Alice" };
            _signupRepoMock.Setup(r => r.GetByIdIncludingLogs("1")).Returns(expected);

            // Act
            var result = _service.GetByIdIncludingLogs("1");

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetByIdIncludingLogs_ShouldReturnNull_WhenSignupNotFound()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.GetByIdIncludingLogs(It.IsAny<string>())).Returns((Signup?)null);

            // Act
            var result = _service.GetByIdIncludingLogs("0");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddSignup_ShouldAddSignupAndLog()
        {
            // Arrange
            var insert = new SignupUpsert
            {
                Name = "Brice",
                PhoneNumber = "555-5551",
                PartySize = 3
            };

            // Act
            _service.Add(insert);

            // Assert
            _signupRepoMock.Verify(r => r.Add(It.Is<Signup>(s =>
                s.Name == "Brice" &&
                s.PhoneNumber == "555-5551" &&
                s.PartySize == 3)), Times.Once);

            _signupRepoMock.Verify(r => r.Log(It.Is<Log>(l =>
                l.Entry.Contains("Added signup"))), Times.Once);
        }

        [TestMethod]
        public void AddSignup_ShouldThrowInCaseOfMissingPartySize()
        {
            // Arrange
            var insert = new SignupUpsert { Name = "Missing PartySize" };

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => _service.Add(insert));
            Assert.AreEqual("Nullable object must have a value.", ex.Message);

            _signupRepoMock.Verify(r => r.Add(It.IsAny<Signup>()), Times.Never);
            _signupRepoMock.Verify(r => r.Log(It.IsAny<Log>()), Times.Never);
        }

        [TestMethod]
        public void AddSignup_ShouldNotLogInCaseOfException()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.Add(It.IsAny<Signup>())).Throws(new DivideByZeroException("Delen door nul is flauwekul"));

            // Act & Assert
            var ex = Assert.ThrowsException<DivideByZeroException>(() => _service.Add(new SignupUpsert { Name = "Zero", PartySize = 1 }));

            _signupRepoMock.Verify(r => r.Log(It.IsAny<Log>()), Times.Never);
        }

        [TestMethod]
        public void Update_ShouldUpdateAndLog()
        {
            // Arrange
            var existing = new Signup { Id = "1", Name = "Old", PhoneNumber = "000", PartySize = 2 };
            var update = new SignupUpsert { Name = "New", PhoneNumber = "111", PartySize = 5 };
            var updated = new Signup { Id = "1", Name = "New", PhoneNumber = "111", PartySize = 5 };

            _signupRepoMock.Setup(r => r.Update(existing.Id, It.IsAny<SignupUpsert>())).Returns(updated);

            // Act
            var result = _service.Update(existing, update);

            // Assert
            Assert.AreEqual(updated, result);
            _signupRepoMock.Verify(r => r.Log(It.Is<Log>(l =>
                l.Entry.Contains("Updated signup"))), Times.Once);
        }

        [TestMethod]
        public void Update_ShouldThrow_WhenSignupNotFound()
        {
            // Arrange
            var nonExistingId = "0";
            var update = new SignupUpsert { Name = "Ghost", PhoneNumber = "000", PartySize = 0 };
            // Mock the repository to mimic 'ctx.Signups.Single(s => s.Id == id);' when trying to update a non-existing signup
            // This scenario is unlikely since existence checks are done before update operations, but we simulate it for testing purposes.
            _signupRepoMock.Setup(r => r.Update(nonExistingId, update)).Throws(new InvalidOperationException("Sequence contains no elements"));

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(() => _service.Update(new Signup { Id = nonExistingId }, update));
            Assert.AreEqual("Sequence contains no elements", ex.Message);

            _signupRepoMock.Verify(r => r.Log(It.IsAny<Log>()), Times.Never);
        }

        [TestMethod]
        public void Delete_ShouldReturnTrue_WhenRepositoryDeletes()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.Delete("1")).Returns(true);

            // Act
            var result = _service.Delete("1");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Delete_ShouldReturnFalse_WhenSignupNotFound()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.Delete("0")).Returns(false);

            // Act
            var result = _service.Delete("0");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CheckData_ShouldThrow_WhenExcelFileIsLocked()
        {
            // Arrange
            _fileServiceMock.Setup(f => f.ResolveExcelPath(It.IsAny<string>())).Returns("dummyPath.xlsx");
            _fileServiceMock.Setup(f => f.EnsureFileNotLocked(It.IsAny<string>())).Throws(new SignupException("Excel file is locked."));

            // Act & Assert
            var ex = Assert.ThrowsException<SignupException>(() => _service.CheckData());
            Assert.AreEqual("Excel file is locked.", ex.Message);
        }

        [TestMethod]
        public void CheckData_ShouldThrow_WhenHasEmptyIntegers()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.Get()).Returns(new List<Signup> { new Signup { Id = "1", PartySize = 2 } });
            _signupRepoMock.Setup(r => r.HasEmptyIntegers()).Returns(true);
            _fileServiceMock.Setup(f => f.ResolveExcelPath(It.IsAny<string>())).Returns("dummyPath.xlsx");
            _fileServiceMock.Setup(f => f.EnsureFileNotLocked(It.IsAny<string>())).Callback(() => { });

            // Act & Assert
            var ex = Assert.ThrowsException<SignupException>(() => _service.CheckData());
            Assert.AreEqual("Empty signup id(s) and/or party size(s) found! Fix this in Excel.", ex.Message);
        }

        [TestMethod]
        public void CheckData_ShouldThrow_WhenHasDuplicates()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.Get()).Returns(new List<Signup> { new Signup { Id = "1", PartySize = 2 } });
            _signupRepoMock.Setup(r => r.HasDuplicates()).Returns(true);
            _fileServiceMock.Setup(f => f.ResolveExcelPath(It.IsAny<string>())).Returns("dummyPath.xlsx");
            _fileServiceMock.Setup(f => f.EnsureFileNotLocked(It.IsAny<string>())).Callback(() => { });

            // Act & Assert
            var ex = Assert.ThrowsException<SignupException>(() => _service.CheckData());
            Assert.AreEqual("Duplicate signup id(s) found! Fix this in Excel.", ex.Message);
        }

        [TestMethod]
        public void CheckData_ShouldReturn_WhenNoIssues()
        {
            // Arrange
            _signupRepoMock.Setup(r => r.Get()).Returns(new List<Signup> { new Signup { Id = "1", PartySize = 2 } });
            _signupRepoMock.Setup(r => r.HasEmptyIntegers()).Returns(false);
            _signupRepoMock.Setup(r => r.HasDuplicates()).Returns(false);
            _fileServiceMock.Setup(f => f.ResolveExcelPath(It.IsAny<string>())).Returns("dummyPath.xlsx");
            _fileServiceMock.Setup(f => f.EnsureFileNotLocked(It.IsAny<string>())).Callback(() => { });

            // Act & Assert
            _service.CheckData(); // should not throw
        }
    }
}
