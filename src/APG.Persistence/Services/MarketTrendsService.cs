using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text.Json;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for market trends analysis using OpenAI LLM
/// </summary>
public class MarketTrendsService : IMarketTrendsService
{
    private readonly ILogger<MarketTrendsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ChatClient _chatClient;

    public MarketTrendsService(
        ILogger<MarketTrendsService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        // Get API key from configuration
        var apiKey = _configuration["OpenAI:ApiKey"] 
            ?? throw new InvalidOperationException("OpenAI API key is not configured. Please set OpenAI:ApiKey in appsettings.json or environment variables.");

        var model = _configuration["OpenAI:Model"] ?? "gpt-4o";
        
        _chatClient = new ChatClient(model, apiKey);
    }

    public async Task<MarketTrendsResponse> AnalyzeMarketTrendsAsync(MarketTrendsRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidateRequest(request);

        try
        {
            var prompt = BuildPrompt(request);
            _logger.LogInformation("Calling LLM for market trends analysis - Role: {Role}, Location: {Location}", 
                request.Role, request.Location ?? "Not specified");

            var chatMessages = new List<ChatMessage>
            {
                new SystemChatMessage(GetSystemPrompt()),
                new UserChatMessage(prompt)
            };

            var completion = await _chatClient.CompleteChatAsync(chatMessages);
            var rawOutput = completion.Value.Content[0].Text;

            _logger.LogDebug("Received LLM response (length: {Length})", rawOutput.Length);

            var response = ParseAndValidateResponse(rawOutput, request.Currency);
            response.RawModelOutput = rawOutput;

            return response;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse LLM response as valid JSON");
            throw new InvalidOperationException("The AI model returned an invalid response format. Please try again.", ex);
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not ArgumentNullException)
        {
            _logger.LogError(ex, "Error calling LLM service for market trends analysis");
            throw new InvalidOperationException("Market trends service is currently unavailable. Please try again later.", ex);
        }
    }

    private void ValidateRequest(MarketTrendsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
        {
            throw new ArgumentException("Role is required", nameof(request.Role));
        }

        if (string.IsNullOrWhiteSpace(request.ResourceType))
        {
            throw new ArgumentException("ResourceType is required", nameof(request.ResourceType));
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new ArgumentException("Currency is required", nameof(request.Currency));
        }

        var validResourceTypes = new[] { "Employee", "Freelancer", "Salarie", "Pigiste" };
        if (!validResourceTypes.Contains(request.ResourceType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"ResourceType must be one of: {string.Join(", ", validResourceTypes)}", 
                nameof(request.ResourceType));
        }
    }

    private string GetSystemPrompt()
    {
        return @"Vous êtes un expert en rémunération et en marché du travail, spécialisé dans les rôles TI au Canada. 
Votre rôle est de fournir une analyse de marché objective et basée sur les données du marché canadien pour l'évaluation de la rentabilité interne dans une société de conseil en TI.

Vous devez répondre UNIQUEMENT avec un objet JSON valide suivant ce schéma exact:
{
  ""salaryRangeByLevel"": {
    ""junior"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" },
    ""intermediate"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" },
    ""senior"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" }
  },
  ""freelanceRateRangeByLevel"": {
    ""junior"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" },
    ""intermediate"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" },
    ""senior"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" }
  },
  ""salaryRange"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" },
  ""freelanceRateRange"": { ""min"": number, ""max"": number, ""currency"": ""CAD"" },
  ""employeePositioning"": ""far_below"" | ""below"" | ""in_line"" | ""above"" | ""far_above"",
  ""freelancePositioning"": ""far_below"" | ""below"" | ""in_line"" | ""above"" | ""far_above"",
  ""marketDemand"": ""low"" | ""medium"" | ""high"" | ""very_high"",
  ""riskLevel"": ""low"" | ""medium"" | ""high"",
  ""summary"": ""string"",
  ""recommendation"": ""string""
}

Directives:
- salaryRangeByLevel: Fourchettes de salaire annuel brut en CAD pour chaque niveau de séniorité (junior, intermediate, senior)
- freelanceRateRangeByLevel: Fourchettes de taux horaire facturable en CAD pour chaque niveau de séniorité
- salaryRange: Fourchette de salaire pour le niveau de séniorité spécifié (ou niveau intermédiaire si non spécifié)
- freelanceRateRange: Fourchette de taux horaire pour le niveau de séniorité spécifié
- employeePositioning: Comparer le salaire proposé aux taux du marché canadien
- freelancePositioning: Comparer le taux facturé proposé aux taux du marché canadien
- marketDemand: Demande actuelle du marché canadien pour ce profil
- riskLevel: Risque d'attrition/difficulté d'embauche au Canada si la rémunération reste au niveau proposé
- summary: Aperçu du marché canadien en 2-3 phrases (en français)
- recommendation: Recommandation commerciale claire en 1-2 phrases (en français)

Répondez UNIQUEMENT avec du JSON valide. N'incluez aucun texte en dehors de l'objet JSON.
IMPORTANT: 
- Tous les montants doivent être en dollars canadiens (CAD)
- Les champs ""summary"" et ""recommendation"" doivent être rédigés en français
- Fournissez toujours les données pour les trois niveaux de séniorité (junior, intermediate, senior)
- Basez vos analyses sur le marché canadien (Toronto, Montréal, Vancouver, etc.)";
    }

    private string BuildPrompt(MarketTrendsRequest request)
    {
        var resourceTypeText = request.ResourceType.Equals("Employee", StringComparison.OrdinalIgnoreCase) 
            || request.ResourceType.Equals("Salarie", StringComparison.OrdinalIgnoreCase)
            ? "Employee" 
            : "Freelancer";

        // Default to Canada if location not specified
        var location = string.IsNullOrWhiteSpace(request.Location) ? "Canada" : request.Location;
        
        // Default to CAD if currency not specified or convert reference
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "CAD" : request.Currency;

        var promptParts = new List<string>
        {
            "Analyze the market trends for the following IT professional profile in the Canadian market:",
            $"- Role: {request.Role}",
            $"- Seniority: {request.Seniority ?? "Not specified - provide data for all levels"}",
            $"- Resource Type: {resourceTypeText}",
            $"- Location: {location}",
            $"- Currency: CAD (Canadian Dollar)"
        };

        if (request.ProposedAnnualSalary.HasValue)
        {
            promptParts.Add($"- Proposed Annual Salary: {request.ProposedAnnualSalary:N0} CAD");
        }

        if (request.ProposedBillRate.HasValue)
        {
            promptParts.Add($"- Proposed Hourly Bill Rate: {request.ProposedBillRate:N0} CAD");
        }

        if (!string.IsNullOrWhiteSpace(request.ClientName))
        {
            promptParts.Add($"- Client: {request.ClientName}");
        }

        if (!string.IsNullOrWhiteSpace(request.BusinessUnit))
        {
            promptParts.Add($"- Business Unit: {request.BusinessUnit}");
        }

        promptParts.Add("\nProvide a comprehensive market analysis for the CANADIAN market comparing the proposed compensation to current market rates.");
        promptParts.Add("Include salary ranges and freelance rate ranges for ALL THREE seniority levels: junior, intermediate, and senior.");
        promptParts.Add("Base your analysis on Canadian cities such as Toronto, Montreal, Vancouver, Calgary, and Ottawa.");
        promptParts.Add("\nIMPORTANT: All monetary values must be in Canadian Dollars (CAD).");
        promptParts.Add("Respond with valid JSON only, following the specified schema exactly.");

        return string.Join("\n", promptParts);
    }

    private MarketTrendsResponse ParseAndValidateResponse(string rawOutput, string requestCurrency)
    {
        // Try to extract JSON if there's extra text
        var jsonStart = rawOutput.IndexOf('{');
        var jsonEnd = rawOutput.LastIndexOf('}');

        if (jsonStart == -1 || jsonEnd == -1 || jsonEnd < jsonStart)
        {
            throw new JsonException("No valid JSON object found in response");
        }

        var jsonContent = rawOutput.Substring(jsonStart, jsonEnd - jsonStart + 1);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var response = JsonSerializer.Deserialize<MarketTrendsResponse>(jsonContent, options)
            ?? throw new JsonException("Failed to deserialize response");

        // Always use CAD as currency for Canadian market
        var currency = "CAD";
        
        // Set currency for main ranges
        if (string.IsNullOrWhiteSpace(response.SalaryRange.Currency))
        {
            response.SalaryRange.Currency = currency;
        }
        if (string.IsNullOrWhiteSpace(response.FreelanceRateRange.Currency))
        {
            response.FreelanceRateRange.Currency = currency;
        }

        // Set currency for seniority-based ranges
        if (response.SalaryRangeByLevel != null)
        {
            if (string.IsNullOrWhiteSpace(response.SalaryRangeByLevel.Junior.Currency))
                response.SalaryRangeByLevel.Junior.Currency = currency;
            if (string.IsNullOrWhiteSpace(response.SalaryRangeByLevel.Intermediate.Currency))
                response.SalaryRangeByLevel.Intermediate.Currency = currency;
            if (string.IsNullOrWhiteSpace(response.SalaryRangeByLevel.Senior.Currency))
                response.SalaryRangeByLevel.Senior.Currency = currency;
        }

        if (response.FreelanceRateRangeByLevel != null)
        {
            if (string.IsNullOrWhiteSpace(response.FreelanceRateRangeByLevel.Junior.Currency))
                response.FreelanceRateRangeByLevel.Junior.Currency = currency;
            if (string.IsNullOrWhiteSpace(response.FreelanceRateRangeByLevel.Intermediate.Currency))
                response.FreelanceRateRangeByLevel.Intermediate.Currency = currency;
            if (string.IsNullOrWhiteSpace(response.FreelanceRateRangeByLevel.Senior.Currency))
                response.FreelanceRateRangeByLevel.Senior.Currency = currency;
        }

        // Validate required fields
        ValidateResponseData(response);

        return response;
    }

    private void ValidateResponseData(MarketTrendsResponse response)
    {
        var validPositioning = new[] { "far_below", "below", "in_line", "above", "far_above" };
        var validMarketDemand = new[] { "low", "medium", "high", "very_high" };
        var validRiskLevel = new[] { "low", "medium", "high" };

        if (!validPositioning.Contains(response.EmployeePositioning, StringComparer.OrdinalIgnoreCase))
        {
            response.EmployeePositioning = "in_line"; // fallback
        }

        if (!validPositioning.Contains(response.FreelancePositioning, StringComparer.OrdinalIgnoreCase))
        {
            response.FreelancePositioning = "in_line"; // fallback
        }

        if (!validMarketDemand.Contains(response.MarketDemand, StringComparer.OrdinalIgnoreCase))
        {
            response.MarketDemand = "medium"; // fallback
        }

        if (!validRiskLevel.Contains(response.RiskLevel, StringComparer.OrdinalIgnoreCase))
        {
            response.RiskLevel = "medium"; // fallback
        }

        if (string.IsNullOrWhiteSpace(response.Summary))
        {
            response.Summary = "Market analysis completed.";
        }

        if (string.IsNullOrWhiteSpace(response.Recommendation))
        {
            response.Recommendation = "Review market data and adjust compensation strategy accordingly.";
        }
    }
}
