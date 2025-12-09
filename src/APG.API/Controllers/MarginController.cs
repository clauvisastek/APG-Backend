using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarginController : ControllerBase
{
    private readonly IMarginSimulationService _marginService;
    private readonly ILogger<MarginController> _logger;

    public MarginController(IMarginSimulationService marginService, ILogger<MarginController> logger)
    {
        _marginService = marginService;
        _logger = logger;
    }

    /// <summary>
    /// Simulate margin calculation based on client configuration and resource details
    /// </summary>
    /// <param name="request">Margin simulation request parameters</param>
    /// <returns>Simulation results with target (CFO) and proposed scenarios</returns>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(MarginSimulationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarginSimulationResponse>> Simulate([FromBody] MarginSimulationRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Requête invalide",
                    Detail = "Le corps de la requête ne peut pas être vide."
                });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.ResourceType))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Type de ressource requis",
                    Detail = "Le type de ressource (Salarie ou Pigiste) est obligatoire."
                });
            }

            if (request.ClientId <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Client requis",
                    Detail = "Un ID de client valide est requis."
                });
            }

            if (request.ProposedBillRate <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Vendant proposé requis",
                    Detail = "Le vendant client proposé doit être supérieur à zéro."
                });
            }

            // Validate salary for Salarie resource type
            if (request.ResourceType.Equals("Salarie", StringComparison.OrdinalIgnoreCase) 
                && (!request.AnnualGrossSalary.HasValue || request.AnnualGrossSalary.Value <= 0))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Salaire requis",
                    Detail = "Le salaire annuel brut est requis pour une ressource salariée."
                });
            }

            var result = await _marginService.SimulateAsync(request);
            
            _logger.LogInformation(
                "Margin simulation completed for client {ClientId}, resource type {ResourceType}, proposed rate {ProposedRate}",
                request.ClientId,
                request.ResourceType,
                request.ProposedBillRate);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in margin simulation request");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Données invalides",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in margin simulation");
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Configuration incomplète",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during margin simulation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erreur serveur",
                Detail = "Une erreur est survenue lors du calcul de la marge. Veuillez réessayer."
            });
        }
    }
}
