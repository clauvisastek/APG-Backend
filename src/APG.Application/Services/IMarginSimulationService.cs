using APG.Application.DTOs;

namespace APG.Application.Services;

public interface IMarginSimulationService
{
    Task<MarginSimulationResponse> SimulateAsync(MarginSimulationRequest request);
}
