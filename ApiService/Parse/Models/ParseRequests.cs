using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ApiService.Parse.Models;

public class ParseRequest
{
    /// <summary>
    /// Text containing personal information to parse using AI
    /// </summary>
    /// <example>My name is Jim Croce, I live in 2944 Monaco dr, Manchester, Colorado, USA, 92223. My phone number is 893-366-8888.</example>
    [Description("Text containing personal information to parse using AI")]
    public string? InputText { get; set; }
    
    /// <summary>
    /// ID to lookup stored person data (jim-croce, person-1, person-2, person-3)
    /// </summary>
    /// <example>jim-croce</example>
    [Description("ID to lookup stored person data")]
    public string? Id { get; set; }
}

public class ParseIdRequest
{
    /// <summary>
    /// Optional ID to lookup stored ID data
    /// </summary>
    /// <example>id-1</example>
    [Description("Optional ID to lookup stored ID data")]
    public string? Id { get; set; }
    
    /// <summary>
    /// Text fallback input if no image provided
    /// </summary>
    /// <example>John Smith, 123 Main St, Springfield IL</example>
    [Description("Text fallback input if no image provided")]
    public string? InputText { get; set; }
    
    /// <summary>
    /// URL to driver's license image (JPG, PNG, WebP)
    /// </summary>
    /// <example>https://example.com/drivers-license.jpg</example>
    [Description("URL to driver's license image")]
    public string? ImageUrl { get; set; }
}