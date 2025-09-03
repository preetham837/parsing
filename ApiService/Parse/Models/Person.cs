using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ApiService.Parse.Models;

public class Person
{
    /// <summary>
    /// Full name of the person
    /// </summary>
    /// <example>John Smith</example>
    [Description("Full name of the person")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Street address
    /// </summary>
    /// <example>123 Main Street</example>
    [Description("Street address")]
    public string Street { get; set; } = string.Empty;
    
    /// <summary>
    /// City name
    /// </summary>
    /// <example>Springfield</example>
    [Description("City name")]
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// State or province
    /// </summary>
    /// <example>IL</example>
    [Description("State or province")]
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Country
    /// </summary>
    /// <example>USA</example>
    [Description("Country")]
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// ZIP or postal code
    /// </summary>
    /// <example>62701</example>
    [Description("ZIP or postal code")]
    public string ZipCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number
    /// </summary>
    /// <example>(555) 123-4567</example>
    [Description("Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;
}