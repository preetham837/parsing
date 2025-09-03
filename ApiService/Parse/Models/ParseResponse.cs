using System.ComponentModel;

namespace ApiService.Parse.Models;

public class ParseResponse
{
    /// <summary>
    /// Source of the data: 'text' (parsed from input_text) or 'id_lookup' (retrieved by ID)
    /// </summary>
    /// <example>text</example>
    [Description("Source of the data")]
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Extracted personal information
    /// </summary>
    [Description("Extracted personal information")]
    public Person Data { get; set; } = new();
}

public class ParseIdResponse
{
    /// <summary>
    /// Source of the data: 'image' (parsed from image), 'text' (fallback), or 'id_lookup' (retrieved by ID)
    /// </summary>
    /// <example>image</example>
    [Description("Source of the data")]
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Extracted ID document information
    /// </summary>
    [Description("Extracted ID document information")]
    public IdParsed Data { get; set; } = new();
}