using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ApiService.Parse.Services;
using ApiService.Parse.Models;

namespace ApiService.Tests;

public class TextParserServiceTests
{
    private readonly Mock<GroqClient> _mockGroqClient;
    private readonly Mock<ILogger<TextParserService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly TextParserService _textParserService;

    public TextParserServiceTests()
    {
        _mockGroqClient = new Mock<GroqClient>(Mock.Of<HttpClient>(), Mock.Of<IConfiguration>(), Mock.Of<ILogger<GroqClient>>());
        _mockLogger = new Mock<ILogger<TextParserService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["TextParserModel"]).Returns("llama-3.3-70b-versatile");
        
        _textParserService = new TextParserService(_mockGroqClient.Object, _mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ParseTextAsync_FullInformation_ReturnsCompletePerson()
    {
        // Arrange
        var inputText = "John Smith lives at 123 Main Street, Springfield, IL, USA 62701. His phone number is (555) 123-4567.";
        var expectedJson = """
            {
              "name": "John Smith",
              "street": "123 Main Street",
              "city": "Springfield",
              "state": "IL",
              "country": "USA",
              "zip_code": "62701",
              "phone_number": "(555) 123-4567"
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _textParserService.ParseTextAsync(inputText);

        // Assert
        Assert.Equal("John Smith", result.Name);
        Assert.Equal("123 Main Street", result.Street);
        Assert.Equal("Springfield", result.City);
        Assert.Equal("IL", result.State);
        Assert.Equal("USA", result.Country);
        Assert.Equal("62701", result.ZipCode);
        Assert.Equal("(555) 123-4567", result.PhoneNumber);
    }

    [Fact]
    public async Task ParseTextAsync_PartialInformation_ReturnsPersonWithEmptyFields()
    {
        // Arrange
        var inputText = "Jane Doe, Chicago";
        var expectedJson = """
            {
              "name": "Jane Doe",
              "street": "",
              "city": "Chicago",
              "state": "",
              "country": "",
              "zip_code": "",
              "phone_number": ""
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _textParserService.ParseTextAsync(inputText);

        // Assert
        Assert.Equal("Jane Doe", result.Name);
        Assert.Equal("Chicago", result.City);
        Assert.Equal("", result.Street);
        Assert.Equal("", result.State);
        Assert.Equal("", result.Country);
        Assert.Equal("", result.ZipCode);
        Assert.Equal("", result.PhoneNumber);
    }

    [Fact]
    public async Task ParseTextAsync_TyposInLabels_HandlesGracefully()
    {
        // Arrange
        var inputText = "Jhon Smyth, adress: 456 Oak Ave, Chicagoo, IL";
        var expectedJson = """
            {
              "name": "Jhon Smyth",
              "street": "456 Oak Ave",
              "city": "Chicagoo",
              "state": "IL",
              "country": "",
              "zip_code": "",
              "phone_number": ""
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _textParserService.ParseTextAsync(inputText);

        // Assert
        Assert.Equal("Jhon Smyth", result.Name);
        Assert.Equal("456 Oak Ave", result.Street);
        Assert.Equal("Chicagoo", result.City);
        Assert.Equal("IL", result.State);
    }

    [Fact]
    public async Task ParseTextAsync_NoPhone_RegexFallbackNotTriggered()
    {
        // Arrange
        var inputText = "Bob Johnson, 789 Pine St, New York, NY 10001";
        var expectedJson = """
            {
              "name": "Bob Johnson",
              "street": "789 Pine St",
              "city": "New York",
              "state": "NY",
              "country": "",
              "zip_code": "10001",
              "phone_number": ""
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _textParserService.ParseTextAsync(inputText);

        // Assert
        Assert.Equal("Bob Johnson", result.Name);
        Assert.Equal("", result.PhoneNumber);
        Assert.Equal("10001", result.ZipCode);
    }

    [Fact]
    public async Task ParseTextAsync_NoZip_RegexFallbackNotTriggered()
    {
        // Arrange
        var inputText = "Alice Cooper, 321 Maple Dr, Boston, MA, phone: 555-987-6543";
        var expectedJson = """
            {
              "name": "Alice Cooper",
              "street": "321 Maple Dr",
              "city": "Boston",
              "state": "MA",
              "country": "",
              "zip_code": "",
              "phone_number": "555-987-6543"
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _textParserService.ParseTextAsync(inputText);

        // Assert
        Assert.Equal("Alice Cooper", result.Name);
        Assert.Equal("555-987-6543", result.PhoneNumber);
        Assert.Equal("", result.ZipCode);
    }

    [Fact]
    public async Task ParseTextAsync_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var inputText = "Invalid input";
        var invalidJson = "{ invalid json }";

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), null))
                      .ReturnsAsync(invalidJson);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _textParserService.ParseTextAsync(inputText));
        
        Assert.Contains("Failed to parse response from AI model", exception.Message);
    }
}