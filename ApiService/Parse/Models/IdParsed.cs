using System.ComponentModel;

namespace ApiService.Parse.Models;

public class IdParsed
{
    /// <summary>
    /// Full name as it appears on the ID (typically LAST, FIRST MIDDLE)
    /// </summary>
    /// <example>SMITH, JOHN MICHAEL</example>
    [Description("Full name as it appears on the ID")]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of birth in yyyy-mm-dd format
    /// </summary>
    /// <example>1985-06-15</example>
    [Description("Date of birth in yyyy-mm-dd format")]
    public string DateOfBirth { get; set; } = string.Empty;
    
    /// <summary>
    /// Address information from the ID
    /// </summary>
    [Description("Address information from the ID")]
    public IdAddress Address { get; set; } = new();
    
    /// <summary>
    /// Driver's license or ID document number
    /// </summary>
    /// <example>S123456789</example>
    [Description("Driver's license or ID document number")]
    public string DocumentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Expiration date in yyyy-mm-dd format
    /// </summary>
    /// <example>2028-06-15</example>
    [Description("Expiration date in yyyy-mm-dd format")]
    public string ExpirationDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Issue date in yyyy-mm-dd format
    /// </summary>
    /// <example>2024-06-15</example>
    [Description("Issue date in yyyy-mm-dd format")]
    public string IssueDate { get; set; } = string.Empty;
    
    /// <summary>
    /// License class (e.g., D, M, CDL-A)
    /// </summary>
    /// <example>D</example>
    [Description("License class")]
    public string LicenseClass { get; set; } = string.Empty;
    
    /// <summary>
    /// License endorsements
    /// </summary>
    /// <example>MOTORCYCLE</example>
    [Description("License endorsements")]
    public string Endorsements { get; set; } = string.Empty;
    
    /// <summary>
    /// License restrictions
    /// </summary>
    /// <example>CORRECTIVE LENSES</example>
    [Description("License restrictions")]
    public string Restrictions { get; set; } = string.Empty;
    
    /// <summary>
    /// Sex/Gender (M/F)
    /// </summary>
    /// <example>M</example>
    [Description("Sex/Gender")]
    public string Sex { get; set; } = string.Empty;
    
    /// <summary>
    /// Eye color (BRN, BLU, GRN, etc.)
    /// </summary>
    /// <example>BRN</example>
    [Description("Eye color")]
    public string EyeColor { get; set; } = string.Empty;
    
    /// <summary>
    /// Height
    /// </summary>
    /// <example>6'00"</example>
    [Description("Height")]
    public string Height { get; set; } = string.Empty;
    
    /// <summary>
    /// Detected country from ID analysis
    /// </summary>
    /// <example>USA</example>
    [Description("Detected country from ID analysis")]
    public string DetectedCountry { get; set; } = string.Empty;
    
    /// <summary>
    /// Detected state from ID analysis
    /// </summary>
    /// <example>IL</example>
    [Description("Detected state from ID analysis")]
    public string DetectedState { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether a barcode was detected on the ID
    /// </summary>
    /// <example>true</example>
    [Description("Whether a barcode was detected on the ID")]
    public bool BarcodePresent { get; set; } = false;
    
    /// <summary>
    /// Warning messages about data quality or uncertainty
    /// </summary>
    /// <example>["Date format may be uncertain", "Low image quality detected"]</example>
    [Description("Warning messages about data quality")]
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// Confidence scores for extracted fields (0.0 to 1.0)
    /// </summary>
    /// <example>{"name": 0.95, "date_of_birth": 0.98, "address": 0.92}</example>
    [Description("Confidence scores for extracted fields")]
    public Dictionary<string, float> Confidences { get; set; } = new();
    
    /// <summary>
    /// Bounding boxes for detected fields [x, y, width, height] normalized to 0-1
    /// </summary>
    /// <example>{"name": [0.1, 0.2, 0.4, 0.05], "address": [0.1, 0.3, 0.6, 0.08]}</example>
    [Description("Bounding boxes for detected fields")]
    public Dictionary<string, float[]> Boxes { get; set; } = new();
}