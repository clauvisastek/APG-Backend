using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // TODO: Enable when authentication is configured
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrenciesController> _logger;

    public CurrenciesController(ICurrencyService currencyService, ILogger<CurrenciesController> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active currencies
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CurrencyDto>>> GetAll()
    {
        try
        {
            var currencies = await _currencyService.GetAllActiveAsync();
            return Ok(currencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currencies");
            return StatusCode(500, new { message = "An error occurred while retrieving currencies" });
        }
    }

    /// <summary>
    /// Get a currency by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyDto>> GetById(int id)
    {
        try
        {
            var currency = await _currencyService.GetByIdAsync(id);

            if (currency == null)
                return NotFound(new { message = $"Currency with ID {id} not found" });

            return Ok(currency);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currency {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the currency" });
        }
    }

    /// <summary>
    /// Create a new currency
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CurrencyDto>> Create([FromBody] CurrencyCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currency = await _currencyService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = currency.Id },
                currency);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating currency");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating currency");
            return StatusCode(500, new { message = "An error occurred while creating the currency" });
        }
    }
}
