# Parsing API

A dual-parser system for extracting personal information from text and driver's license images using GROQ AI models.

## Features

- **Text Parser**: Extracts personal information from natural language text using regex pre-processing and GROQ LLM
- **Image Parser**: Analyzes driver's license images using vision-capable GROQ models  
- **ID Lookup**: In-memory lookup for stored person/ID data
- **Multiple Input Formats**: JSON, multipart form data, image URLs
- **OpenAPI Documentation**: Full Swagger/OpenAPI support with examples
- **Comprehensive Testing**: Unit tests for all components

## Architecture

- **Backend**: ASP.NET Core with CQRS/MediatR pattern
- **Frontend**: Next.js with RTK Query (auto-generated from OpenAPI)
- **AI Provider**: GROQ API for both text and vision models
- **Documentation**: NSwag for OpenAPI generation

## Quick Start

### Prerequisites

1. .NET 9.0 SDK
2. Node.js and pnpm
3. GROQ API key (configured in `ApiService/appsettings.json`)

### Running the Application

```bash
# Build and run all services (includes Aspire orchestration)
dotnet build
dotnet run --project AppHost

# Or run API service individually
dotnet run --project ApiService

# Frontend development (after API changes)
cd web
pnpm install
pnpm generate-api  # Updates TypeScript client from OpenAPI spec
pnpm dev
```

### Running Tests

```bash
dotnet test ApiService.Tests
```

## API Endpoints

### POST /api/parse

Parse personal information from text input or return stored data by ID.

**Request Body:**
```json
{
  "input_text": "John Smith lives at 123 Main Street, Springfield, IL 62701. Phone: (555) 123-4567",
  "id": "person-1"  // Optional: returns stored data if provided
}
```

**Response:**
```json
{
  "source": "text",  // "text" | "id_lookup"
  "data": {
    "name": "John Smith",
    "street": "123 Main Street", 
    "city": "Springfield",
    "state": "IL",
    "country": "USA",
    "zip_code": "62701",
    "phone_number": "(555) 123-4567"
  }
}
```

### POST /api/parse/id

Parse driver's license information from image, URL, or text fallback.

**Multipart Form Upload:**
```bash
curl -X POST http://localhost:5000/api/parse/id \
  -F "image=@drivers-license.jpg" \
  -F "id=id-1"  # Optional: returns stored data if provided
```

**JSON Body:**
```json
{
  "image_url": "https://example.com/drivers-license.jpg",
  "id": "id-1",        // Optional: ID lookup
  "input_text": "..."  // Optional: text fallback
}
```

**Response:**
```json
{
  "source": "image",  // "image" | "text" | "id_lookup"
  "data": {
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
    "warnings": ["Date format may be uncertain"],
    "confidences": {"name": 0.95, "date_of_birth": 0.98},
    "boxes": {"name": [0.1, 0.2, 0.4, 0.05]}
  }
}
```

## Configuration

### Environment Variables

- `GROQ_API_KEY`: Your GROQ API key (required)
- `TextParserModel`: GROQ model for text parsing (default: `llama-3.3-70b-versatile`)
- `ImageParserModel`: GROQ model for image parsing (default: `llama-3.2-90b-vision-preview`)

### Supported Image Formats

- JPEG, JPG, PNG, WebP
- Maximum file size: 10MB
- Recommended: High-resolution, well-lit driver's licenses

## Sample Data

### Available Person IDs
- `person-1`: John Smith (Springfield, IL)
- `person-2`: Jane Doe (Chicago, IL) 
- `person-3`: Michael Johnson (New York, NY)

### Available ID Data IDs
- `id-1`: John Michael Smith (IL driver's license)
- `id-2`: Jane Elizabeth Doe (IL driver's license)

## Security Features

- PII masking in logs (document numbers show only last 4 digits)
- Input validation and sanitization
- File type and size restrictions
- No credentials stored in source code

## Error Handling

- Invalid JSON responses trigger automatic retry with stricter prompts
- Comprehensive error logging with structured data
- Graceful fallbacks (ID lookup → image parsing → text parsing)
- Detailed validation messages for invalid inputs

## API Documentation

Once running, visit:
- Swagger UI: `http://localhost:5000/swagger`
- OpenAPI spec: `http://localhost:5000/swagger/v1/swagger.json`

The frontend automatically generates TypeScript clients from the OpenAPI spec using:
```bash
pnpm generate-api
```

## Development Notes

- Text parser uses regex pre-pass for phone numbers and ZIP codes
- Image parser normalizes dates to `yyyy-mm-dd` format when certain
- All services registered with proper DI lifetime scopes
- Logging includes performance metrics and confidence scores
- Unit tests cover happy paths, edge cases, and error conditions