using ApiService.Parse.Models;

namespace ApiService.Parse.Services;

public class LookupService
{
    private readonly Dictionary<string, Person> _personIndex;
    private readonly Dictionary<string, IdParsed> _idIndex;
    private readonly ILogger<LookupService> _logger;

    public LookupService(ILogger<LookupService> logger)
    {
        _logger = logger;
        _personIndex = InitializePersonIndex();
        _idIndex = InitializeIdIndex();
    }

    public Person? GetPersonById(string id)
    {
        _logger.LogInformation("Looking up person with ID: {Id}", id);
        return _personIndex.TryGetValue(id, out var person) ? person : null;
    }

    public IdParsed? GetIdParsedById(string id)
    {
        _logger.LogInformation("Looking up ID data with ID: {Id}", id);
        return _idIndex.TryGetValue(id, out var idData) ? idData : null;
    }

    private static Dictionary<string, Person> InitializePersonIndex()
    {
        return new Dictionary<string, Person>
        {
            ["person-1"] = new Person
            {
                Name = "John Smith",
                Street = "123 Main Street",
                City = "Springfield",
                State = "IL",
                Country = "USA",
                ZipCode = "62701",
                PhoneNumber = "(555) 123-4567"
            },
            ["person-2"] = new Person
            {
                Name = "Jane Doe",
                Street = "456 Oak Avenue",
                City = "Chicago",
                State = "IL", 
                Country = "USA",
                ZipCode = "60601",
                PhoneNumber = "(555) 987-6543"
            },
            ["person-3"] = new Person
            {
                Name = "Michael Johnson",
                Street = "789 Elm Drive",
                City = "New York",
                State = "NY",
                Country = "USA",
                ZipCode = "10001",
                PhoneNumber = "(555) 456-7890"
            },
            ["jim-croce"] = new Person
            {
                Name = "Jim Croce",
                Street = "2944 Monaco Dr",
                City = "Manchester",
                State = "Colorado",
                Country = "USA",
                ZipCode = "92223",
                PhoneNumber = "893-366-8888"
            }
        };
    }

    private static Dictionary<string, IdParsed> InitializeIdIndex()
    {
        return new Dictionary<string, IdParsed>
        {
            ["id-1"] = new IdParsed
            {
                FullName = "SMITH, JOHN MICHAEL",
                DateOfBirth = "1985-06-15",
                Address = new IdAddress
                {
                    Street = "123 MAIN ST",
                    City = "SPRINGFIELD",
                    State = "IL",
                    Country = "USA",
                    ZipCode = "62701"
                },
                DocumentNumber = "S123456789",
                ExpirationDate = "2028-06-15",
                IssueDate = "2024-06-15",
                LicenseClass = "D",
                Endorsements = "",
                Restrictions = "CORRECTIVE LENSES",
                Sex = "M",
                EyeColor = "BRN",
                Height = "6'00\"",
                DetectedCountry = "USA",
                DetectedState = "IL",
                BarcodePresent = true,
                Confidences = new Dictionary<string, float>
                {
                    ["name"] = 0.95f,
                    ["date_of_birth"] = 0.98f,
                    ["address"] = 0.92f
                }
            },
            ["id-2"] = new IdParsed
            {
                FullName = "DOE, JANE ELIZABETH",
                DateOfBirth = "1990-03-22",
                Address = new IdAddress
                {
                    Street = "456 OAK AVE",
                    City = "CHICAGO",
                    State = "IL",
                    Country = "USA",
                    ZipCode = "60601"
                },
                DocumentNumber = "D987654321",
                ExpirationDate = "2029-03-22",
                IssueDate = "2025-03-22",
                LicenseClass = "D",
                Endorsements = "",
                Restrictions = "",
                Sex = "F",
                EyeColor = "BLU",
                Height = "5'06\"",
                DetectedCountry = "USA",
                DetectedState = "IL",
                BarcodePresent = true,
                Confidences = new Dictionary<string, float>
                {
                    ["name"] = 0.97f,
                    ["date_of_birth"] = 0.99f,
                    ["address"] = 0.94f
                }
            }
        };
    }
}