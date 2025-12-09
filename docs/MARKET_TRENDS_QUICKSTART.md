# Quick Start: Testing Market Trends Endpoint

## Prerequisites

1. **OpenAI API Key**: Get one from https://platform.openai.com/api-keys

2. **Set API Key** (choose one method):

   ```bash
   # Option A: Environment variable (recommended)
   export OpenAI__ApiKey="sk-your-api-key-here"
   
   # Option B: User secrets
   cd src/APG.API
   dotnet user-secrets set "OpenAI:ApiKey" "sk-your-api-key-here"
   ```

3. **Build & Run**:
   ```bash
   cd src/APG.API
   dotnet run
   ```

## Test Scenarios

### Scenario 1: Senior Developer - Employee

```json
POST https://localhost:5001/api/market-trends
Authorization: Bearer YOUR_JWT_TOKEN

{
  "role": "Java Developer",
  "seniority": "Senior",
  "resourceType": "Employee",
  "location": "Montreal, Canada",
  "currency": "CAD",
  "proposedAnnualSalary": 95000
}
```

**Expected Response:**
- Salary range for Senior Java Developer in Montreal
- Positioning (e.g., "in_line", "below")
- Market demand assessment
- Risk level
- Business recommendation

### Scenario 2: Freelance React Developer

```json
POST https://localhost:5001/api/market-trends
Authorization: Bearer YOUR_JWT_TOKEN

{
  "role": "React Developer",
  "seniority": "Mid-Level",
  "resourceType": "Freelancer",
  "location": "Paris, France",
  "currency": "EUR",
  "proposedBillRate": 450,
  "clientName": "Tech Corp",
  "businessUnit": "Digital France BU-002"
}
```

**Expected Response:**
- Freelance rate range for React Developer
- Comparison with proposed 450 EUR/hour
- Market demand in Paris
- Recommendations for pricing

### Scenario 3: Junior Developer - Both Types

```json
POST https://localhost:5001/api/market-trends
Authorization: Bearer YOUR_JWT_TOKEN

{
  "role": "Python Developer",
  "seniority": "Junior",
  "resourceType": "Employee",
  "location": "Toronto, Canada",
  "currency": "CAD",
  "proposedAnnualSalary": 65000,
  "proposedBillRate": 75
}
```

**Expected Response:**
- Both employee salary range and freelance rates
- Dual positioning analysis
- Entry-level market insights

### Scenario 4: Specialized Role - High Demand

```json
POST https://localhost:5001/api/market-trends
Authorization: Bearer YOUR_JWT_TOKEN

{
  "role": "Machine Learning Engineer",
  "seniority": "Senior",
  "resourceType": "Employee",
  "location": "San Francisco, USA",
  "currency": "USD",
  "proposedAnnualSalary": 180000
}
```

**Expected Response:**
- Premium market analysis
- High demand indicators
- Competitive insights for specialized roles

## Testing with curl

```bash
# Set your JWT token
TOKEN="eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."

# Test the endpoint
curl -X POST https://localhost:5001/api/market-trends \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "role": "Full Stack Developer",
    "seniority": "Senior",
    "resourceType": "Employee",
    "location": "Montreal, Canada",
    "currency": "CAD",
    "proposedAnnualSalary": 105000
  }' | jq
```

## Validation Tests

### Test 1: Missing Required Field (Role)

```json
POST /api/market-trends
{
  "seniority": "Senior",
  "resourceType": "Employee",
  "currency": "CAD"
}
```

**Expected:** 400 Bad Request - "Role is required"

### Test 2: Invalid Resource Type

```json
POST /api/market-trends
{
  "role": "Developer",
  "resourceType": "Contractor",
  "currency": "CAD"
}
```

**Expected:** 400 Bad Request - "ResourceType must be one of: Employee, Freelancer, Salarie, Pigiste"

### Test 3: Missing API Key

Remove API key from configuration and restart.

**Expected:** 500 Internal Server Error - "Service Configuration Error"

### Test 4: No Authentication

Don't include Authorization header.

**Expected:** 401 Unauthorized

## Response Interpretation

### Positioning Values

- **far_below**: 20%+ below market average
- **below**: 10-20% below market
- **in_line**: Within 10% of market average
- **above**: 10-20% above market
- **far_above**: 20%+ above market

### Market Demand

- **low**: Limited demand, many candidates
- **medium**: Balanced supply and demand
- **high**: Strong demand, competitive hiring
- **very_high**: Critical shortage, premium rates

### Risk Level

- **low**: Compensation is competitive, low attrition risk
- **medium**: Some risk, monitor market changes
- **high**: High attrition risk, immediate attention needed

## Troubleshooting Quick Fixes

| Error | Quick Fix |
|-------|-----------|
| 500 "API key not configured" | Set `OpenAI__ApiKey` environment variable |
| 503 "Service Unavailable" | Check OpenAI service status, retry |
| 400 "Invalid Request" | Verify all required fields present |
| 401 "Unauthorized" | Include valid JWT token |
| JSON parse error | Check model setting, try gpt-4o |

## Performance Notes

- **Response time**: Typically 2-5 seconds (depends on OpenAI API)
- **Cost per request**: ~$0.01-0.03 with gpt-4o
- **Rate limits**: OpenAI default limits apply

## Integration with Frontend

The frontend can call this endpoint from the margin calculator:

```typescript
// Example integration
async function getMarketTrends(profileData: MarketTrendsRequest): Promise<MarketTrendsResponse> {
  const response = await fetch('https://localhost:5001/api/market-trends', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`
    },
    body: JSON.stringify(profileData)
  });
  
  if (!response.ok) {
    throw new Error('Market trends unavailable');
  }
  
  return await response.json();
}
```

## Next Steps

1. ✅ Configure OpenAI API key
2. ✅ Test endpoint with Swagger UI
3. ✅ Verify different scenarios work
4. ✅ Integrate with frontend margin calculator
5. ✅ Monitor costs and usage
6. ✅ Add caching if needed

For complete documentation, see [MARKET_TRENDS_API.md](./MARKET_TRENDS_API.md).
