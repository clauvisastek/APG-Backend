using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

[ApiController]
[Route("api/market-trends")]
[Authorize]
public class MarketTrendsController : ControllerBase
{
    private readonly IMarketTrendsService _marketTrendsService;
    private readonly ILogger<MarketTrendsController> _logger;

    public MarketTrendsController(
        IMarketTrendsService marketTrendsService,
        ILogger<MarketTrendsController> logger)
    {
        _marketTrendsService = marketTrendsService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze market trends for a given professional profile using AI
    /// </summary>
    /// <param name="request">Market trends analysis request</param>
    /// <returns>Market trends analysis with salary ranges, positioning, and recommendations</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MarketTrendsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketTrendsResponse>> AnalyzeMarketTrends(
        [FromBody] MarketTrendsRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid Request",
                    Detail = "Request body cannot be empty."
                });
            }

            _logger.LogInformation(
                "Market trends analysis requested for role: {Role}, location: {Location}", 
                request.Role, 
                request.Location ?? "Not specified");

            var response = await _marketTrendsService.AnalyzeMarketTrendsAsync(request);

            return Ok(response);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Null argument in market trends request");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Request",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for market trends analysis");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Request",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Market trends service is unavailable");
            
            // Check if it's a configuration issue or service availability issue
            if (ex.Message.Contains("API key") || ex.Message.Contains("configured"))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Service Configuration Error",
                    Detail = "Market trends service is not properly configured."
                });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Service Unavailable",
                Detail = "Market trends analysis service is temporarily unavailable. Please try again later.",
                Extensions = { ["errorCode"] = "market_trends_unavailable" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during market trends analysis");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request."
            });
        }
    }
}
