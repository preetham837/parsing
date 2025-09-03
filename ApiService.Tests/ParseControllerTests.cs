using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ApiService.Controllers;
using ApiService.Parse.Services;
using ApiService.Parse.Models;

namespace ApiService.Tests;

public class ParseControllerTests
{
    private readonly Mock<TextParserService> _mockTextParserService;
    private readonly Mock<ImageParserService> _mockImageParserService;
    private readonly Mock<LookupService> _mockLookupService;
    private readonly Mock<ILogger<ParseController>> _mockLogger;
    private readonly ParseController _controller;

    public ParseControllerTests()
    {
        _mockTextParserService = new Mock<TextParserService>(
            Mock.Of<GroqClient>(),
            Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(),
            Mock.Of<ILogger<TextParserService>>());
        
        _mockImageParserService = new Mock<ImageParserService>(
            Mock.Of<GroqClient>(),
            Mock.Of<HttpClient>(),
            Mock.Of<ILogger<ImageParserService>>(),
            Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>());
        
        _mockLookupService = new Mock<LookupService>(Mock.Of<ILogger<LookupService>>());
        _mockLogger = new Mock<ILogger<ParseController>>();
        
        _controller = new ParseController(
            _mockTextParserService.Object,
            _mockImageParserService.Object,
            _mockLookupService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Parse_WithValidId_ReturnsIdLookupResponse()
    {
        // Arrange
        var request = new ParseRequest { Id = "person-1", InputText = "some text" };
        var expectedPerson = new Person { Name = "John Smith", City = "Springfield" };
        
        _mockLookupService.Setup(x => x.GetPersonById("person-1"))
                         .Returns(expectedPerson);

        // Act
        var result = await _controller.Parse(request);

        // Assert
        var okResult = Assert.IsType<ActionResult<ParseResponse>>(result);
        var okValue = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<ParseResponse>(okValue.Value);
        
        Assert.Equal("id_lookup", response.Source);
        Assert.Equal("John Smith", response.Data.Name);
        Assert.Equal("Springfield", response.Data.City);
    }

    [Fact]
    public async Task Parse_WithInvalidId_FallsBackToTextParsing()
    {
        // Arrange
        var request = new ParseRequest { Id = "invalid-id", InputText = "John Doe, Chicago" };
        var expectedPerson = new Person { Name = "John Doe", City = "Chicago" };
        
        _mockLookupService.Setup(x => x.GetPersonById("invalid-id"))
                         .Returns((Person?)null);
        
        _mockTextParserService.Setup(x => x.ParseTextAsync("John Doe, Chicago"))
                             .ReturnsAsync(expectedPerson);

        // Act
        var result = await _controller.Parse(request);

        // Assert
        var okResult = Assert.IsType<ActionResult<ParseResponse>>(result);
        var okValue = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<ParseResponse>(okValue.Value);
        
        Assert.Equal("text", response.Source);
        Assert.Equal("John Doe", response.Data.Name);
        Assert.Equal("Chicago", response.Data.City);
    }

    [Fact]
    public async Task Parse_WithTextOnly_ReturnsTextParsingResponse()
    {
        // Arrange
        var request = new ParseRequest { InputText = "Alice Smith, Boston, MA" };
        var expectedPerson = new Person { Name = "Alice Smith", City = "Boston", State = "MA" };
        
        _mockTextParserService.Setup(x => x.ParseTextAsync("Alice Smith, Boston, MA"))
                             .ReturnsAsync(expectedPerson);

        // Act
        var result = await _controller.Parse(request);

        // Assert
        var okResult = Assert.IsType<ActionResult<ParseResponse>>(result);
        var okValue = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<ParseResponse>(okValue.Value);
        
        Assert.Equal("text", response.Source);
        Assert.Equal("Alice Smith", response.Data.Name);
        Assert.Equal("Boston", response.Data.City);
        Assert.Equal("MA", response.Data.State);
    }

    [Fact]
    public async Task Parse_WithEmptyInputText_ReturnsBadRequest()
    {
        // Arrange
        var request = new ParseRequest { InputText = "" };

        // Act
        var result = await _controller.Parse(request);

        // Assert
        var badRequestResult = Assert.IsType<ActionResult<ParseResponse>>(result);
        Assert.IsType<BadRequestObjectResult>(badRequestResult.Result);
    }

    [Fact]
    public async Task Parse_TextParsingThrowsException_ReturnsProblem()
    {
        // Arrange
        var request = new ParseRequest { InputText = "test" };
        
        _mockTextParserService.Setup(x => x.ParseTextAsync("test"))
                             .ThrowsAsync(new Exception("Parsing error"));

        // Act
        var result = await _controller.Parse(request);

        // Assert
        var problemResult = Assert.IsType<ActionResult<ParseResponse>>(result);
        var objectResult = Assert.IsType<ObjectResult>(problemResult.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }
}