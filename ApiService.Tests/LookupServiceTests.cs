using Microsoft.Extensions.Logging;
using Moq;
using ApiService.Parse.Services;

namespace ApiService.Tests;

public class LookupServiceTests
{
    private readonly Mock<ILogger<LookupService>> _mockLogger;
    private readonly LookupService _lookupService;

    public LookupServiceTests()
    {
        _mockLogger = new Mock<ILogger<LookupService>>();
        _lookupService = new LookupService(_mockLogger.Object);
    }

    [Fact]
    public void GetPersonById_ValidId_ReturnsExpectedPerson()
    {
        // Act
        var result = _lookupService.GetPersonById("person-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Smith", result.Name);
        Assert.Equal("123 Main Street", result.Street);
        Assert.Equal("Springfield", result.City);
        Assert.Equal("IL", result.State);
        Assert.Equal("USA", result.Country);
        Assert.Equal("62701", result.ZipCode);
        Assert.Equal("(555) 123-4567", result.PhoneNumber);
    }

    [Fact]
    public void GetPersonById_InvalidId_ReturnsNull()
    {
        // Act
        var result = _lookupService.GetPersonById("invalid-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPersonById_Person2_ReturnsJaneDoe()
    {
        // Act
        var result = _lookupService.GetPersonById("person-2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("456 Oak Avenue", result.Street);
        Assert.Equal("Chicago", result.City);
        Assert.Equal("IL", result.State);
        Assert.Equal("(555) 987-6543", result.PhoneNumber);
    }

    [Fact]
    public void GetIdParsedById_ValidId_ReturnsExpectedIdData()
    {
        // Act
        var result = _lookupService.GetIdParsedById("id-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SMITH, JOHN MICHAEL", result.FullName);
        Assert.Equal("1985-06-15", result.DateOfBirth);
        Assert.Equal("123 MAIN ST", result.Address.Street);
        Assert.Equal("SPRINGFIELD", result.Address.City);
        Assert.Equal("IL", result.Address.State);
        Assert.Equal("S123456789", result.DocumentNumber);
        Assert.True(result.BarcodePresent);
        Assert.Equal("USA", result.DetectedCountry);
        Assert.Equal("IL", result.DetectedState);
    }

    [Fact]
    public void GetIdParsedById_InvalidId_ReturnsNull()
    {
        // Act
        var result = _lookupService.GetIdParsedById("invalid-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetIdParsedById_Id2_ReturnsJaneDoeData()
    {
        // Act
        var result = _lookupService.GetIdParsedById("id-2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DOE, JANE ELIZABETH", result.FullName);
        Assert.Equal("1990-03-22", result.DateOfBirth);
        Assert.Equal("456 OAK AVE", result.Address.Street);
        Assert.Equal("CHICAGO", result.Address.City);
        Assert.Equal("D987654321", result.DocumentNumber);
        Assert.True(result.BarcodePresent);
    }
}