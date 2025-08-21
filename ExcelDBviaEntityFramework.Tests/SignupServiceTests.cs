using ExcelDBviaEntityFramework.Data;
using ExcelDBviaEntityFramework.Models;
using ExcelDBviaEntityFramework.Services;
using ExcelDBviaEntityFramework.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;

[TestClass]
public class SignupServiceTests
{
    private FakeExcelDbContext _fakeCtx;
    private Mock<IDbContextFactory<ExcelDbContext>> _factoryMock;
    private SignupService _service;

    [TestInitialize]
    public void Setup()
    {
        // 1. Create fake in-memory context
        _fakeCtx = new FakeExcelDbContext();

        // 2. Setup factory to always return the fake context
        _factoryMock = new Mock<IDbContextFactory<ExcelDbContext>>();
        _factoryMock.Setup(f => f.CreateDbContext())
                    .Returns(_fakeCtx);

        // 3. Inject into SignupService
        _service = new SignupService(_factoryMock.Object);
    }

    [TestMethod]
    public void AddSignup_Should_Add_Signup_And_Log()
    {
        // Arrange
        var insert = new SignupUpsert
        {
            Name = "John Doe",
            PhoneNumber = "123456",
            PartySize = 3
        };

        // Act
        var result = _service.AddSignup(insert);

        // Assert
        Assert.AreEqual(1, _fakeCtx.Signups.Count(), "Signup not added");
        Assert.AreEqual(1, _fakeCtx.Logs.Count(), "Log not added");
        Assert.AreEqual("John Doe", result.Name);
    }

    [TestMethod]
    public void GetSignup_Should_Return_Signup()
    {
        // Arrange
        var signup = new Signup { Id = "1", Name = "Alice", PartySize = 2 };
        _fakeCtx.Signups.Add(signup);

        // Act
        var result = _service.GetSignup("1");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Alice", result.Name);
    }
}
