# Market Trends Feature - Deployment Checklist

## Pre-Deployment

### Development Environment

- [ ] OpenAI API key configured (user secrets or environment variable)
- [ ] Application builds successfully (`dotnet build`)
- [ ] All tests pass
- [ ] Endpoint tested via Swagger UI
- [ ] Test scenarios validated (see MARKET_TRENDS_QUICKSTART.md)
- [ ] Error handling verified (400, 401, 500, 503 responses)
- [ ] JWT authentication working correctly
- [ ] Documentation reviewed and up to date

### Code Review

- [ ] Code follows project conventions
- [ ] No API keys or secrets in source code
- [ ] Logging is appropriate (no PII logged)
- [ ] Error messages are user-friendly
- [ ] Input validation is comprehensive
- [ ] Response schema matches specification
- [ ] Service abstraction allows for future provider changes

### Security Review

- [ ] API key stored securely (environment variables)
- [ ] JWT authentication enforced
- [ ] Input sanitization implemented
- [ ] No sensitive data in logs
- [ ] `.env` file in `.gitignore`
- [ ] HTTPS enforced in production
- [ ] CORS configured correctly

## Deployment Steps

### 1. Environment Configuration

#### Production Server

```bash
# Set environment variables
export OpenAI__ApiKey="sk-prod-your-api-key"
export OpenAI__Model="gpt-4o"
export ASPNETCORE_ENVIRONMENT="Production"
```

#### Azure App Service

```bash
# Set application settings
az webapp config appsettings set \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --settings OpenAI__ApiKey="sk-prod-key" OpenAI__Model="gpt-4o"
```

#### Docker

Update `docker-compose.yml`:
```yaml
environment:
  - OpenAI__ApiKey=${OPENAI_API_KEY}
  - OpenAI__Model=gpt-4o
```

### 2. Database Migration

No database changes required for this feature.

### 3. Dependencies

```bash
# Restore NuGet packages
cd /path/to/APG_Backend
dotnet restore

# Verify OpenAI package installed
dotnet list package | grep OpenAI
# Should show: OpenAI 2.1.0
```

### 4. Build & Test

```bash
# Build in Release mode
dotnet build --configuration Release

# Run tests (if available)
dotnet test

# Publish for deployment
dotnet publish --configuration Release --output ./publish
```

### 5. Deploy

Choose your deployment method:

#### Option A: Azure App Service
```bash
az webapp deployment source config-zip \
  --resource-group YourResourceGroup \
  --name YourAppName \
  --src ./publish.zip
```

#### Option B: Docker
```bash
docker build -t apg-backend:latest .
docker push your-registry/apg-backend:latest
```

#### Option C: Manual Deploy
- Copy files from `./publish` to server
- Restart application

## Post-Deployment

### Smoke Tests

- [ ] API is accessible (`GET /health`)
- [ ] Swagger UI loads (`GET /swagger`)
- [ ] Authentication works (test with JWT token)
- [ ] Market trends endpoint responds (`POST /api/market-trends`)
- [ ] Error handling works (test with invalid data)

### Functional Tests

Run these test scenarios:

#### Test 1: Valid Request
```bash
curl -X POST https://your-domain.com/api/market-trends \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "role": "Java Developer",
    "seniority": "Senior",
    "resourceType": "Employee",
    "location": "Montreal, Canada",
    "currency": "CAD",
    "proposedAnnualSalary": 95000
  }'
```
**Expected:** 200 OK with full market analysis

#### Test 2: Missing Required Field
```bash
curl -X POST https://your-domain.com/api/market-trends \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "seniority": "Senior",
    "currency": "CAD"
  }'
```
**Expected:** 400 Bad Request

#### Test 3: No Authentication
```bash
curl -X POST https://your-domain.com/api/market-trends \
  -H "Content-Type: application/json" \
  -d '{
    "role": "Developer",
    "resourceType": "Employee",
    "currency": "CAD"
  }'
```
**Expected:** 401 Unauthorized

### Performance Monitoring

- [ ] Monitor response times (should be 2-5 seconds)
- [ ] Check OpenAI API usage and costs
- [ ] Monitor error rates
- [ ] Set up alerts for failures

### Cost Monitoring

Track OpenAI API costs:

```bash
# Check OpenAI usage dashboard
# https://platform.openai.com/usage

# Expected costs:
# - gpt-4o: ~$0.01-0.03 per request
# - 100 requests/day = ~$1-3/day
# - 3000 requests/month = ~$30-90/month
```

## Configuration Validation

### Environment Variables (Required)

| Variable | Required | Example | Description |
|----------|----------|---------|-------------|
| `OpenAI__ApiKey` | ✅ Yes | `sk-proj-...` | OpenAI API key |
| `OpenAI__Model` | Optional | `gpt-4o` | Model to use (default: gpt-4o) |
| `ConnectionStrings__DefaultConnection` | ✅ Yes | `Server=...` | Database connection |
| `ASPNETCORE_ENVIRONMENT` | ✅ Yes | `Production` | Environment name |

### Verification Script

```bash
#!/bin/bash
# verify-market-trends-deployment.sh

echo "Checking Market Trends deployment..."

# Check if API key is set
if [ -z "$OpenAI__ApiKey" ]; then
    echo "❌ OpenAI__ApiKey not set"
    exit 1
else
    echo "✅ OpenAI__ApiKey configured"
fi

# Check if application is running
HEALTH_CHECK=$(curl -s -o /dev/null -w "%{http_code}" https://your-domain.com/health)
if [ "$HEALTH_CHECK" = "200" ]; then
    echo "✅ Application is running"
else
    echo "❌ Application health check failed"
    exit 1
fi

# Test market trends endpoint (requires valid token)
if [ -z "$JWT_TOKEN" ]; then
    echo "⚠️  JWT_TOKEN not set, skipping endpoint test"
else
    MARKET_TRENDS_CHECK=$(curl -s -o /dev/null -w "%{http_code}" \
        -X POST https://your-domain.com/api/market-trends \
        -H "Authorization: Bearer $JWT_TOKEN" \
        -H "Content-Type: application/json" \
        -d '{"role":"Test","resourceType":"Employee","currency":"CAD"}')
    
    if [ "$MARKET_TRENDS_CHECK" = "200" ]; then
        echo "✅ Market trends endpoint working"
    else
        echo "❌ Market trends endpoint returned: $MARKET_TRENDS_CHECK"
        exit 1
    fi
fi

echo "✅ All checks passed!"
```

## Rollback Plan

If issues occur after deployment:

### Step 1: Immediate Actions
1. Disable the feature flag (if implemented)
2. Or remove the controller temporarily
3. Or set API key to empty (will return 500 - service misconfigured)

### Step 2: Investigate
1. Check application logs
2. Check OpenAI API status
3. Verify environment variables
4. Test with different inputs

### Step 3: Fix or Revert
1. Fix configuration issue
2. Or revert to previous deployment
3. Re-deploy when fixed

## Monitoring & Alerts

### Application Insights (Azure)

```csharp
// Already logging in MarketTrendsService:
_logger.LogInformation("Calling LLM for market trends analysis...");
_logger.LogError(ex, "Error calling LLM service...");
```

### Custom Metrics to Track

- Request count per day
- Average response time
- Error rate
- OpenAI API costs
- Most requested roles/locations

### Alert Thresholds

| Metric | Threshold | Action |
|--------|-----------|--------|
| Error rate | > 5% | Investigate immediately |
| Response time | > 10 seconds | Check OpenAI status |
| Cost per day | > $20 | Review usage patterns |
| 503 responses | > 10 per hour | Check OpenAI rate limits |

## Documentation

### User Documentation

- [ ] Update API documentation
- [ ] Add to user guides
- [ ] Create video tutorial (if applicable)
- [ ] Update frontend documentation

### Internal Documentation

- [ ] Add to architecture docs
- [ ] Update deployment runbooks
- [ ] Document cost structure
- [ ] Add to troubleshooting guide

## Support Preparation

### Training

- [ ] Train support team on feature
- [ ] Provide troubleshooting guide
- [ ] Document common issues and solutions

### Escalation Path

1. **Tier 1**: Check configuration and logs
2. **Tier 2**: Review OpenAI status and API limits
3. **Tier 3**: Development team for code issues

## Success Criteria

After deployment, verify:

- [ ] ✅ Endpoint responds correctly to valid requests
- [ ] ✅ Error handling works for invalid inputs
- [ ] ✅ Authentication is enforced
- [ ] ✅ Response times are acceptable (2-5 seconds)
- [ ] ✅ No errors in application logs
- [ ] ✅ OpenAI costs are within budget
- [ ] ✅ Frontend integration works (if deployed)
- [ ] ✅ Users can access the feature
- [ ] ✅ Documentation is available

## Maintenance

### Monthly Tasks

- [ ] Review OpenAI API costs
- [ ] Check for API updates
- [ ] Review error logs
- [ ] Update model if new versions available
- [ ] Check rate limits and adjust if needed

### Quarterly Tasks

- [ ] Review prompt effectiveness
- [ ] Analyze user feedback
- [ ] Consider implementing caching
- [ ] Review alternative AI providers
- [ ] Update documentation

---

**Prepared by**: Development Team  
**Last Updated**: December 5, 2025  
**Version**: 1.0
