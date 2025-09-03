using System.Text.Json;
using ApiService.Parse.Models;

namespace ApiService.Parse.Services;

public class ImageParserService
{
    private readonly GroqClient _groqClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageParserService> _logger;
    private readonly string _model;

    public ImageParserService(GroqClient groqClient, HttpClient httpClient, ILogger<ImageParserService> logger, IConfiguration configuration)
    {
        _groqClient = groqClient;
        _httpClient = httpClient;
        _logger = logger;
        _model = configuration["ImageParserModel"] ?? "llama-3.2-90b-vision-preview";
    }

    public async Task<IdParsed> ParseImageAsync(string imageBase64)
    {
        try
        {
            var prompt = @"You are an expert at reading US driver's licenses. Extract ALL information from this driver's license image. Return ONLY valid JSON with these exact fields:

{
  ""full_name"": """",
  ""date_of_birth"": """",
  ""address"": {
    ""street"": """",
    ""city"": """",
    ""state"": """",
    ""country"": """",
    ""zip_code"": """"
  },
  ""document_number"": """",
  ""expiration_date"": """",
  ""issue_date"": """",
  ""license_class"": """",
  ""endorsements"": """",
  ""restrictions"": """",
  ""sex"": """",
  ""eye_color"": """",
  ""height"": """",
  ""detected_country"": """",
  ""detected_state"": """",
  ""barcode_present"": false,
  ""warnings"": [],
  ""confidences"": {},
  ""boxes"": {}
}

CRITICAL INSTRUCTIONS FOR DRIVER'S LICENSE READING:
- FULL_NAME: Look for the largest text showing the person's name, often in format 'LAST, FIRST MIDDLE'. Extract and reformat as 'FIRST MIDDLE LAST'. Check multiple areas of the license.
- DATE_OF_BIRTH: MANDATORY field on all licenses. Look for patterns like 'DOB', 'DATE OF BIRTH', or dates in MM/DD/YYYY, MM-DD-YYYY, or similar formats. Common locations: data section, near photo, or separate field.
- EYE_COLOR: MANDATORY field. Look for 'EYE', 'EYES', 'E' patterns. Values: BRO/BRN (Brown), BLU (Blue), GRN (Green), HZL (Hazel), GRY (Gray), BLK (Black).
- DOCUMENT_NUMBER: The license number, usually prominent alphanumeric string. Look for patterns like 'DL', 'LIC', or standalone number sequences.
- LICENSE_CLASS: Usually single letter or letter-number combo (A, B, C, CDL, etc.). Often near license number.
- EXPIRATION_DATE: Look for 'EXP', 'EXPIRES', 'EXPIRATION' labels. Critical for validity.
- ISSUE_DATE: Look for 'ISS', 'ISSUED', 'ISSUE DATE' labels.
- ADDRESS COMPONENTS: Extract complete address:
  * Street: Full street address with number and name
  * City: City name
  * State: 2-letter state code (CA, NY, TX, etc.)
  * ZIP: 5-digit or 9-digit postal code
- PHYSICAL DATA: 
  * SEX: M/F
  * HEIGHT: Various formats (5-10, 5'10"", 510, etc.)
- SCAN STRATEGY:
  * Read top section for name and main info
  * Check data grid/table for abbreviated fields
  * Look for date patterns in MM/DD/YYYY format
  * Identify license number (usually prominent)
  * Extract address block completely
- DATE HANDLING: Convert all dates to YYYY-MM-DD format when extracting
- MANDATORY VALIDATION: Name, DOB, license number, and eye color must be present on valid US licenses
- OUTPUT: Return ONLY valid JSON with no explanatory text";

            var response = await _groqClient.ChatCompletionAsync(_model, prompt, imageBase64);
            
            var result = await ParseGroqResponse(response);
            
            // If critical fields are missing, try with a focused retry prompt
            if (string.IsNullOrWhiteSpace(result.FullName) || string.IsNullOrWhiteSpace(result.EyeColor) || 
                string.IsNullOrWhiteSpace(result.DateOfBirth) || string.IsNullOrWhiteSpace(result.DocumentNumber))
            {
                _logger.LogWarning("Critical fields missing, attempting focused retry extraction");
                
                var retryPrompt = @"FOCUSED RE-EXTRACTION: The previous attempt missed critical information. 
Examine this driver's license image EXTREMELY carefully and extract these MANDATORY fields:

1. FULL NAME - Look for the person's complete name (usually the largest text)
2. DATE OF BIRTH - Look for DOB, birth date (MM/DD/YYYY format)
3. DOCUMENT NUMBER - The license number (alphanumeric)
4. EYE COLOR - Look for abbreviated eye color (BRN, BLU, GRN, HZL, etc.)
5. EXPIRATION DATE - Look for EXP date
6. LICENSE CLASS - Look for class (A, B, C, etc.)

Return only JSON format:
{
  ""full_name"": ""[COMPLETE NAME]"", 
  ""date_of_birth"": ""[DOB in YYYY-MM-DD format]"", 
  ""document_number"": ""[LICENSE NUMBER]"",
  ""eye_color"": ""[EYE COLOR]"",
  ""expiration_date"": ""[EXP DATE in YYYY-MM-DD format]"",
  ""license_class"": ""[CLASS]""
}

CRITICAL: These fields are mandatory on all US driver's licenses. Scan every part of the image systematically.";

                var retryResponse = await _groqClient.ChatCompletionAsync(_model, retryPrompt, imageBase64);
                
                try
                {
                    var focusedResult = JsonSerializer.Deserialize<Dictionary<string, string>>(retryResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (focusedResult != null)
                    {
                        if (string.IsNullOrWhiteSpace(result.FullName) && focusedResult.TryGetValue("full_name", out var name) && !string.IsNullOrWhiteSpace(name))
                        {
                            result.FullName = name;
                            result.Warnings.Add("Full name extracted on retry attempt");
                        }
                        
                        if (string.IsNullOrWhiteSpace(result.DateOfBirth) && focusedResult.TryGetValue("date_of_birth", out var dob) && !string.IsNullOrWhiteSpace(dob))
                        {
                            result.DateOfBirth = dob;
                            result.Warnings.Add("Date of birth extracted on retry attempt");
                        }
                        
                        if (string.IsNullOrWhiteSpace(result.DocumentNumber) && focusedResult.TryGetValue("document_number", out var docNum) && !string.IsNullOrWhiteSpace(docNum))
                        {
                            result.DocumentNumber = docNum;
                            result.Warnings.Add("Document number extracted on retry attempt");
                        }
                        
                        if (string.IsNullOrWhiteSpace(result.EyeColor) && focusedResult.TryGetValue("eye_color", out var eyeColor) && !string.IsNullOrWhiteSpace(eyeColor))
                        {
                            result.EyeColor = StandardizeEyeColor(eyeColor);
                            result.Warnings.Add("Eye color extracted on retry attempt");
                        }
                        
                        if (string.IsNullOrWhiteSpace(result.ExpirationDate) && focusedResult.TryGetValue("expiration_date", out var expDate) && !string.IsNullOrWhiteSpace(expDate))
                        {
                            result.ExpirationDate = expDate;
                            result.Warnings.Add("Expiration date extracted on retry attempt");
                        }
                        
                        if (string.IsNullOrWhiteSpace(result.LicenseClass) && focusedResult.TryGetValue("license_class", out var licClass) && !string.IsNullOrWhiteSpace(licClass))
                        {
                            result.LicenseClass = licClass;
                            result.Warnings.Add("License class extracted on retry attempt");
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse focused retry response");
                }
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing image");
            throw;
        }
    }

    public async Task<IdParsed> ParseImageFromUrlAsync(string imageUrl)
    {
        try
        {
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var imageBase64 = Convert.ToBase64String(imageBytes);
            
            return await ParseImageAsync(imageBase64);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading and parsing image from URL: {Url}", imageUrl);
            throw;
        }
    }

    private async Task<IdParsed> ParseGroqResponse(string response)
    {
        try
        {
            var result = JsonSerializer.Deserialize<IdParsed>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize response");
            }

            // Post-process dates and add warnings
            result = PostProcessResult(result);
            
            // Log extraction results for monitoring
            _logger.LogInformation("Image parsing completed. Name: {HasName}, DOB: {HasDOB}, DocNum: {HasDocNum}, EyeColor: {HasEyeColor}, ExpDate: {HasExpDate}, Warnings: {WarningCount}", 
                !string.IsNullOrWhiteSpace(result.FullName), 
                !string.IsNullOrWhiteSpace(result.DateOfBirth),
                !string.IsNullOrWhiteSpace(result.DocumentNumber),
                !string.IsNullOrWhiteSpace(result.EyeColor),
                !string.IsNullOrWhiteSpace(result.ExpirationDate),
                result.Warnings.Count);
            
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Invalid JSON response, retrying with stricter prompt");
            
            // Retry with stricter instruction
            var retryPrompt = "The previous response was not valid JSON. Return only a valid JSON object with the driver's license data. No explanatory text.";
            var retryResponse = await _groqClient.ChatCompletionAsync(_model, retryPrompt);
            
            try
            {
                var result = JsonSerializer.Deserialize<IdParsed>(retryResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result ?? new IdParsed();
            }
            catch (JsonException)
            {
                _logger.LogError("Failed to parse JSON response from GROQ after retry");
                throw new InvalidOperationException("Failed to parse response from AI model", ex);
            }
        }
    }

    private IdParsed PostProcessResult(IdParsed result)
    {
        // Mask document number for logging
        if (!string.IsNullOrEmpty(result.DocumentNumber) && result.DocumentNumber.Length > 4)
        {
            var masked = "****" + result.DocumentNumber.Substring(result.DocumentNumber.Length - 4);
            _logger.LogInformation("Processed document with number ending in: {LastFour}", result.DocumentNumber.Substring(result.DocumentNumber.Length - 4));
        }

        // Critical field validation - these should always be present on valid US licenses
        if (string.IsNullOrWhiteSpace(result.FullName))
        {
            result.Warnings.Add("CRITICAL: Full name not detected - this is required on all US driver's licenses");
            _logger.LogWarning("Full name field is empty or missing");
        }

        if (string.IsNullOrWhiteSpace(result.DateOfBirth))
        {
            result.Warnings.Add("Date of birth not detected - this is required on all US driver's licenses");
            _logger.LogWarning("Date of birth field is empty or missing");
        }

        if (string.IsNullOrWhiteSpace(result.DocumentNumber))
        {
            result.Warnings.Add("License number not detected - this is the primary identifier");
            _logger.LogWarning("Document number field is empty or missing");
        }

        if (string.IsNullOrWhiteSpace(result.EyeColor))
        {
            result.Warnings.Add("Eye color not detected - check if license image is clear and complete");
            _logger.LogWarning("Eye color field is empty or missing");
        }

        if (string.IsNullOrWhiteSpace(result.ExpirationDate))
        {
            result.Warnings.Add("Expiration date not detected - critical for license validity");
            _logger.LogWarning("Expiration date field is empty or missing");
        }

        // Standardize eye color abbreviations
        if (!string.IsNullOrWhiteSpace(result.EyeColor))
        {
            result.EyeColor = StandardizeEyeColor(result.EyeColor);
        }

        // Standardize and validate date formats
        if (!string.IsNullOrEmpty(result.DateOfBirth))
        {
            result.DateOfBirth = StandardizeDateFormat(result.DateOfBirth, "Date of birth");
            if (!IsValidDateFormat(result.DateOfBirth))
            {
                result.Warnings.Add("Date of birth format may be uncertain");
            }
        }
        
        if (!string.IsNullOrEmpty(result.ExpirationDate))
        {
            result.ExpirationDate = StandardizeDateFormat(result.ExpirationDate, "Expiration date");
            if (!IsValidDateFormat(result.ExpirationDate))
            {
                result.Warnings.Add("Expiration date format may be uncertain");
            }
        }

        if (!string.IsNullOrEmpty(result.IssueDate))
        {
            result.IssueDate = StandardizeDateFormat(result.IssueDate, "Issue date");
            if (!IsValidDateFormat(result.IssueDate))
            {
                result.Warnings.Add("Issue date format may be uncertain");
            }
        }

        // Validate name format
        if (!string.IsNullOrWhiteSpace(result.FullName))
        {
            if (result.FullName.Contains(","))
            {
                result.Warnings.Add("Name may be in 'Last, First' format - check extraction accuracy");
            }
            
            if (result.FullName.Length < 3)
            {
                result.Warnings.Add("Name appears unusually short - may be partially extracted");
            }
        }

        return result;
    }

    private static string StandardizeEyeColor(string eyeColor)
    {
        var normalized = eyeColor.ToUpperInvariant().Trim();
        
        return normalized switch
        {
            "BRO" or "BRN" or "BR" => "Brown",
            "BLU" or "BL" or "BLUE" => "Blue", 
            "GRN" or "GR" or "GREEN" => "Green",
            "HZL" or "HAZ" or "HAZEL" => "Hazel",
            "GRY" or "GRAY" or "GREY" => "Gray",
            "BLK" or "BLACK" => "Black",
            "AMB" or "AMBER" => "Amber",
            _ => eyeColor // Return original if no match
        };
    }

    private static string StandardizeDateFormat(string dateString, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return dateString;

        var trimmed = dateString.Trim();
        
        // Try to parse various common date formats and convert to YYYY-MM-DD
        var formats = new[]
        {
            "MM/dd/yyyy", "MM-dd-yyyy", "MM.dd.yyyy",
            "MM/dd/yy", "MM-dd-yy", "MM.dd.yy",
            "M/d/yyyy", "M-d-yyyy", "M.d.yyyy",
            "M/d/yy", "M-d-yy", "M.d.yy",
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", // Already in target format
            "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy", // European format (less common on US licenses)
            "MMM dd, yyyy", "MMM dd yyyy", "MMM-dd-yyyy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(trimmed, format, null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd");
            }
        }

        // If no standard format matches, try general parsing
        if (DateTime.TryParse(trimmed, out var generalDate))
        {
            return generalDate.ToString("yyyy-MM-dd");
        }

        // If all parsing fails, return original
        return dateString;
    }

    private static bool IsValidDateFormat(string date)
    {
        return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);
    }
}