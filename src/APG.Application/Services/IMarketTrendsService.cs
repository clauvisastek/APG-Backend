using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service interface for market trends analysis using AI/LLM
/// </summary>
public interface IMarketTrendsService
{
    /// <summary>
    /// Analyzes market trends for a given profile using an AI model
    /// </summary>
    /// <param name="request">Market trends request with profile information</param>
    /// <returns>Market trends analysis response</returns>
    /// <exception cref="ArgumentNullException">When request is null</exception>
    /// <exception cref="ArgumentException">When required fields are missing or invalid</exception>
    /// <exception cref="InvalidOperationException">When LLM service is unavailable or misconfigured</exception>
    Task<MarketTrendsResponse> AnalyzeMarketTrendsAsync(MarketTrendsRequest request);
}
