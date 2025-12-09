# Market Trends AI Analysis Endpoint

## Overview

The Market Trends endpoint provides AI-powered compensation analysis for IT professionals using OpenAI's GPT models. It analyzes salary ranges, freelance rates, market positioning, and provides business recommendations.

## Endpoint

**POST** `/api/market-trends`

**Authorization**: Required (JWT Bearer token)

## Request Schema

```json
{
  "role": "string (required)",
  "seniority": "string (optional)",
  "resourceType": "string (required: Employee|Freelancer|Salarie|Pigiste)",
  "location": "string (optional)",
  "currency": "string (required: CAD|USD|EUR|etc.)",
  "proposedAnnualSalary": "number (optional)",
  "proposedBillRate": "number (optional)",
  "clientName": "string (optional)",
  "businessUnit": "string (optional)"
}
```

### Example Request

```json
{
  "role": "Java Developer",
  "seniority": "Senior",
  "resourceType": "Employee",
  "location": "Montreal, Canada",
  "currency": "CAD",
  "proposedAnnualSalary": 95000,
  "proposedBillRate": 150,
  "clientName": "Bank of X",
  "businessUnit": "Banking France BU-001"
}
```

## Response Schema

```json
{
  "salaryRange": {
    "min": "number",
    "max": "number",
    "currency": "string"
  },
  "freelanceRateRange": {
    "min": "number",
    "max": "number",
    "currency": "string"
  },
  "employeePositioning": "far_below | below | in_line | above | far_above",
  "freelancePositioning": "far_below | below | in_line | above | far_above",
  "marketDemand": "low | medium | high | very_high",
  "riskLevel": "low | medium | high",
  "summary": "string",
  "recommendation": "string",
  "rawModelOutput": "string (optional)"
}
```

### Example Response

```json
{
  "salaryRange": {
    "min": 85000,
    "max": 110000,
    "currency": "CAD"
  },
  "freelanceRateRange": {
    "min": 100,
    "max": 175,
    "currency": "CAD"
  },
  "employeePositioning": "in_line",
  "freelancePositioning": "in_line",
  "marketDemand": "high",
  "riskLevel": "low",
  "summary": "Senior Java Developers in Montreal are in high demand. The proposed salary of $95,000 CAD is competitive and within market range.",
  "recommendation": "The current compensation is appropriate for the market. Consider regular market reviews to maintain competitiveness.",
  "rawModelOutput": "..."
}
```

## HTTP Status Codes

- **200 OK**: Analysis completed successfully
- **400 Bad Request**: Invalid request parameters (missing required fields, invalid values)
- **401 Unauthorized**: Missing or invalid authentication token
- **500 Internal Server Error**: Service configuration error (e.g., missing API key)
- **503 Service Unavailable**: LLM service temporarily unavailable

### Error Response Format

```json
{
  "status": 400,
  "title": "Invalid Request",
  "detail": "Role is required"
}
```

## Setup Instructions

### 1. Install NuGet Package

The OpenAI package has been added to `APG.Persistence.csproj`:

```bash
cd src/APG.Persistence
dotnet add package OpenAI --version 2.1.0
```

Or restore all packages:

```bash
cd /path/to/APG_Backend
dotnet restore
```

### 2. Configure OpenAI API Key

You have three options to configure your OpenAI API key:

#### Option A: appsettings.json (Not Recommended for Production)

Edit `src/APG.API/appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key-here",
    "Model": "gpt-4o"
  }
}
```

#### Option B: Environment Variable (Recommended)

Set environment variable:

**macOS/Linux:**
```bash
export OpenAI__ApiKey="sk-your-api-key-here"
```

**Windows:**
```cmd
set OpenAI__ApiKey=sk-your-api-key-here
```

**Docker:**
Add to `docker-compose.yml`:
```yaml
environment:
  - OpenAI__ApiKey=sk-your-api-key-here
```

#### Option C: User Secrets (Recommended for Development)

```bash
cd src/APG.API
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-api-key-here"
```

### 3. Build and Run

```bash
cd src/APG.API
dotnet build
dotnet run
```

## Testing

### Using curl

```bash
# Get JWT token first (use your Auth0 credentials)
TOKEN="your-jwt-token"

# Make request
curl -X POST https://localhost:5001/api/market-trends \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "role": "React Developer",
    "seniority": "Mid-Level",
    "resourceType": "Freelancer",
    "location": "Paris, France",
    "currency": "EUR",
    "proposedBillRate": 450
  }'
```

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Click "Authorize" and enter your JWT token
3. Find the `/api/market-trends` endpoint
4. Click "Try it out"
5. Enter request body and click "Execute"

### Using Postman

1. Create new POST request to `https://localhost:5001/api/market-trends`
2. Set Authorization: Bearer Token
3. Set Body: raw JSON
4. Enter request JSON
5. Send

## Architecture

### Components

1. **MarketTrendsDto.cs** (`APG.Application/DTOs`)
   - Request and response models
   - Strongly-typed data contracts

2. **IMarketTrendsService.cs** (`APG.Application/Services`)
   - Service interface
   - Defines contract for market analysis

3. **MarketTrendsService.cs** (`APG.Persistence/Services`)
   - Service implementation
   - OpenAI integration
   - Prompt engineering
   - Response parsing and validation

4. **MarketTrendsController.cs** (`APG.API/Controllers`)
   - REST API endpoint
   - Request validation
   - Error handling

### Data Flow

```
Frontend Request
    ↓
MarketTrendsController (validation, error handling)
    ↓
IMarketTrendsService (business logic)
    ↓
MarketTrendsService (LLM integration)
    ↓
OpenAI GPT-4o API
    ↓
Response parsing & validation
    ↓
MarketTrendsResponse
    ↓
Frontend
```

## Security Considerations

1. **API Key Security**
   - Never commit API keys to source control
   - Use environment variables or user secrets
   - Rotate keys regularly

2. **Logging**
   - No PII logged
   - Minimal prompt logging
   - Role and location only logged for debugging

3. **Authentication**
   - Endpoint requires JWT authentication
   - Auth0 integration

4. **Rate Limiting**
   - Consider implementing rate limiting for cost control
   - OpenAI has default rate limits

## Cost Management

OpenAI API costs depend on:
- Model used (gpt-4o is more expensive than gpt-3.5-turbo)
- Number of tokens (input + output)
- Request frequency

**Estimated costs** (as of Dec 2024):
- gpt-4o: ~$0.01-0.03 per request
- gpt-3.5-turbo: ~$0.001-0.003 per request

To reduce costs:
1. Change model to `gpt-3.5-turbo` in appsettings.json
2. Implement caching for similar requests
3. Add rate limiting per user
4. Use shorter prompts

## Troubleshooting

### "OpenAI API key is not configured"

**Solution**: Set the API key using one of the methods in Setup Instructions.

### "Service Unavailable" (503)

**Causes**:
- OpenAI API is down
- Network connectivity issues
- Rate limit exceeded

**Solution**: Wait and retry. Check OpenAI status page.

### "Invalid Request" (400)

**Causes**:
- Missing required fields (role, resourceType, currency)
- Invalid resourceType value

**Solution**: Validate request body matches schema.

### Invalid JSON Response

**Causes**:
- Model returned malformed JSON
- Model didn't follow instructions

**Solution**: 
- Service automatically tries to extract JSON from response
- Check logs for `rawModelOutput`
- Consider adjusting system prompt

## Model Configuration

You can use different OpenAI models by changing the `Model` setting:

```json
{
  "OpenAI": {
    "Model": "gpt-4o"  // Options: gpt-4o, gpt-4-turbo, gpt-3.5-turbo
  }
}
```

**Model Recommendations**:
- **gpt-4o**: Most accurate, best for complex analysis (recommended)
- **gpt-4-turbo**: Good balance of cost and quality
- **gpt-3.5-turbo**: Fastest and cheapest, good for simple cases

## Future Enhancements

1. **Response Caching**
   - Cache similar requests for 24 hours
   - Reduce API costs

2. **Multiple LLM Providers**
   - Add Azure OpenAI support
   - Add Anthropic Claude support
   - Provider abstraction layer

3. **Enhanced Analytics**
   - Store analysis results in database
   - Historical trend tracking
   - Benchmark reports

4. **Fine-tuning**
   - Create custom model for compensation analysis
   - Industry-specific training data

5. **Validation**
   - Cross-reference with external salary databases
   - Confidence scoring

## Support

For issues or questions:
1. Check application logs
2. Review this documentation
3. Contact the development team
