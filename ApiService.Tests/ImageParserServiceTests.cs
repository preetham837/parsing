using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ApiService.Parse.Services;
using ApiService.Parse.Models;

namespace ApiService.Tests;

public class ImageParserServiceTests
{
    private readonly Mock<GroqClient> _mockGroqClient;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<ImageParserService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ImageParserService _imageParserService;

    public ImageParserServiceTests()
    {
        _mockGroqClient = new Mock<GroqClient>(Mock.Of<HttpClient>(), Mock.Of<IConfiguration>(), Mock.Of<ILogger<GroqClient>>());
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<ImageParserService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(x => x["ImageParserModel"]).Returns("llama-3.2-90b-vision-preview");
        
        _imageParserService = new ImageParserService(_mockGroqClient.Object, _mockHttpClient.Object, _mockLogger.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task ParseImageAsync_GoodQuality_ReturnsCompleteIdParsed()
    {
        // Arrange
        var imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
        var expectedJson = """
            {
              "full_name": "SMITH, JOHN MICHAEL",
              "date_of_birth": "1985-06-15",
              "address": {
                "street": "123 MAIN ST",
                "city": "SPRINGFIELD",
                "state": "IL",
                "country": "USA",
                "zip_code": "62701"
              },
              "document_number": "S123456789",
              "expiration_date": "2028-06-15",
              "issue_date": "2024-06-15",
              "license_class": "D",
              "endorsements": "",
              "restrictions": "CORRECTIVE LENSES",
              "sex": "M",
              "eye_color": "BRN",
              "height": "6'00\"",
              "detected_country": "USA",
              "detected_state": "IL",
              "barcode_present": true,
              "warnings": [],
              "confidences": {"name": 0.95, "date_of_birth": 0.98},
              "boxes": {}
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), imageBase64))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _imageParserService.ParseImageAsync(imageBase64);

        // Assert
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
    public async Task ParseImageAsync_LowQuality_AddsWarnings()
    {
        // Arrange
        var imageBase64 = "lowquality_base64_image";
        var expectedJson = """
            {
              "full_name": "SMITH, JOHN",
              "date_of_birth": "1985-6-15",
              "address": {
                "street": "123 MAIN ST",
                "city": "SPRINGFIELD",
                "state": "IL",
                "country": "USA",
                "zip_code": "62701"
              },
              "document_number": "S123456789",
              "expiration_date": "2028-6-15",
              "issue_date": "2024-6-15",
              "license_class": "D",
              "endorsements": "",
              "restrictions": "",
              "sex": "M",
              "eye_color": "BRN",
              "height": "6'00\"",
              "detected_country": "USA",
              "detected_state": "IL",
              "barcode_present": false,
              "warnings": [],
              "confidences": {},
              "boxes": {}
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), imageBase64))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _imageParserService.ParseImageAsync(imageBase64);

        // Assert
        Assert.Equal("SMITH, JOHN", result.FullName);
        Assert.Contains("Date of birth format may be uncertain", result.Warnings);
        Assert.Contains("Expiration date format may be uncertain", result.Warnings);
        Assert.Contains("Issue date format may be uncertain", result.Warnings);
        Assert.False(result.BarcodePresent);
    }

    [Fact]
    public async Task ParseImageAsync_UnknownFields_HandlesGracefully()
    {
        // Arrange
        var imageBase64 = "unknown_fields_image";
        var expectedJson = """
            {
              "full_name": "",
              "date_of_birth": "",
              "address": {
                "street": "",
                "city": "",
                "state": "",
                "country": "",
                "zip_code": ""
              },
              "document_number": "",
              "expiration_date": "",
              "issue_date": "",
              "license_class": "",
              "endorsements": "",
              "restrictions": "",
              "sex": "",
              "eye_color": "",
              "height": "",
              "detected_country": "",
              "detected_state": "",
              "barcode_present": false,
              "warnings": [],
              "confidences": {},
              "boxes": {}
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), imageBase64))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _imageParserService.ParseImageAsync(imageBase64);

        // Assert
        Assert.Equal("", result.FullName);
        Assert.Equal("", result.DateOfBirth);
        Assert.Equal("", result.Address.Street);
        Assert.Equal("", result.DocumentNumber);
        Assert.False(result.BarcodePresent);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task ParseImageAsync_MissingBarcode_SetsBarcodePresentToFalse()
    {
        // Arrange
        var imageBase64 = "no_barcode_image";
        var expectedJson = """
            {
              "full_name": "DOE, JANE",
              "date_of_birth": "1990-03-22",
              "address": {
                "street": "456 OAK AVE",
                "city": "CHICAGO",
                "state": "IL",
                "country": "USA",
                "zip_code": "60601"
              },
              "document_number": "D987654321",
              "expiration_date": "2029-03-22",
              "issue_date": "2025-03-22",
              "license_class": "D",
              "endorsements": "",
              "restrictions": "",
              "sex": "F",
              "eye_color": "BLU",
              "height": "5'06\"",
              "detected_country": "USA",
              "detected_state": "IL",
              "barcode_present": false,
              "warnings": [],
              "confidences": {},
              "boxes": {}
            }
            """;

        _mockGroqClient.Setup(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), imageBase64))
                      .ReturnsAsync(expectedJson);

        // Act
        var result = await _imageParserService.ParseImageAsync(imageBase64);

        // Assert
        Assert.Equal("DOE, JANE", result.FullName);
        Assert.False(result.BarcodePresent);
    }

    [Fact]
    public async Task ParseImageAsync_InvalidJsonResponse_RetriesAndHandles()
    {
        // Arrange
        var imageBase64 = "invalid_response_image";
        var invalidJson = "{ this is not valid json }";
        var validRetryJson = """
            {
              "full_name": "RETRY, SUCCESS",
              "date_of_birth": "1990-01-01",
              "address": {
                "street": "",
                "city": "",
                "state": "",
                "country": "",
                "zip_code": ""
              },
              "document_number": "",
              "expiration_date": "",
              "issue_date": "",
              "license_class": "",
              "endorsements": "",
              "restrictions": "",
              "sex": "",
              "eye_color": "",
              "height": "",
              "detected_country": "",
              "detected_state": "",
              "barcode_present": false,
              "warnings": [],
              "confidences": {},
              "boxes": {}
            }
            """;

        _mockGroqClient.SetupSequence(x => x.ChatCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), imageBase64))
                      .ReturnsAsync(invalidJson)
                      .ReturnsAsync(validRetryJson);

        // Act
        var result = await _imageParserService.ParseImageAsync(imageBase64);

        // Assert
        Assert.Equal("RETRY, SUCCESS", result.FullName);
        Assert.Equal("1990-01-01", result.DateOfBirth);
    }
}