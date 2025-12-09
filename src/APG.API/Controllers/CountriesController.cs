using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // TODO: Enable when authentication is configured
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ICountryService countryService, ILogger<CountriesController> logger)
    {
        _countryService = countryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active countries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CountryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CountryDto>>> GetAll()
    {
        try
        {
            var countries = await _countryService.GetAllActiveAsync();
            return Ok(countries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving countries");
            return StatusCode(500, new { message = "An error occurred while retrieving countries" });
        }
    }

    /// <summary>
    /// Get a country by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CountryDto>> GetById(int id)
    {
        try
        {
            var country = await _countryService.GetByIdAsync(id);

            if (country == null)
                return NotFound(new { message = $"Country with ID {id} not found" });

            return Ok(country);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving country {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the country" });
        }
    }

    /// <summary>
    /// Create a new country
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CountryDto>> Create([FromBody] CountryCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var country = await _countryService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = country.Id },
                country);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating country");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating country");
            return StatusCode(500, new { message = "An error occurred while creating the country" });
        }
    }
}
