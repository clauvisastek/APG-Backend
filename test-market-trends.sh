#!/bin/bash

TOKEN="eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjRzRlFRclRQaDk0LVZsdnVMRzVIXyJ9.eyJodHRwczovL2FwZy1hc3Rlay5jb20vcm9sZXMiOlsiQWRtaW4iLCJCVS0wMDEiLCJCVS0wMDMiLCJDRk8iXSwibmlja25hbWUiOiJjbGF1dmlzLmtpdGlldSIsIm5hbWUiOiJjbGF1dmlzLmtpdGlldUBhc3Rlay5uZXQiLCJwaWN0dXJlIjoiaHR0cHM6Ly9zLmdyYXZhdGFyLmNvbS9hdmF0YXIvMjY3NmUxYmI2MmI4ODMxZDJmNGVkYzVhNGFjZWQ0MmY_cz00ODAmcj1wZyZkPWh0dHBzJTNBJTJGJTJGY2RuLmF1dGgwLmNvbSUyRmF2YXRhcnMlMkZjbC5wbmciLCJ1cGRhdGVkX2F0IjoiMjAyNS0xMi0wNVQyMToxODowNS44NjhaIiwiZW1haWwiOiJjbGF1dmlzLmtpdGlldUBhc3Rlay5uZXQiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiaXNzIjoiaHR0cHM6Ly9hc3Rla2NhbmFkYS5jYS5hdXRoMC5jb20vIiwiYXVkIjoieTk4ZHJMN2kxTEFWblc4SG05Qzc0M002dHhMa0xDSEUiLCJzdWIiOiJhdXRoMHw2OTJlODIwNWEwZmRiYzBiODQyM2U4NDIiLCJpYXQiOjE3NjQ5Njk0ODYsImV4cCI6MTc2NTAwNTQ4Niwic2lkIjoiWGxFVTNIeWsweEluQ1JBUU44Yi1aRGwyanJMREwtTloiLCJub25jZSI6Ik4xUk5jVUZqUm5SWlRESmFlVEZQY25oWVlubFJZMDUwZUd4Qk1FVm1iMll0UzJkeGFYNXFSMGhMU0E9PSJ9.r1gGM5-Yvq1-XQF7FmwsaqEJD-ruwq08g1YEtPjjSZv9D7SXtQUbdGcXRovxiHlL7WwCOO1YqNKmIvbwGHuOQs4vSoRZ2VqGx8FdznEZXMDvjQZoK9mb2xdjdifx78Ikdv7eQloUDKEeVTiNXDXgoABJUH3j0mCyIp7y98kx0AiUEXKDBbj3oGtB4YYxiHUIm8uPOsA7iJ9h9XQCq-JUfTvHX4cVZ90kvoimNN8Owz997Zp0z-lFQr5WHkxdNfURV9Gp53BhInen81utrGiIEb7KnDt8jzGmhAXW80N1O1lGYpQ9_s2r_tHEefCdoZrNV3G0gRaGRmPtNVHmSrFmoQ"

echo "🧪 Test du endpoint Market Trends avec OpenAI..."
echo ""

curl -X POST http://localhost:5001/api/market-trends \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "role": "Java Developer",
    "seniority": "Senior",
    "resourceType": "Employee",
    "location": "Montreal, Canada",
    "currency": "CAD",
    "proposedAnnualSalary": 95000
  }' | python3 -m json.tool

echo ""
echo "✅ Test terminé!"
