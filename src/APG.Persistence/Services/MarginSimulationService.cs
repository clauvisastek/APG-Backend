using APG.Application.DTOs;
using APG.Application.Services;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Services;

public class MarginSimulationService : IMarginSimulationService
{
    private const decimal HOURS_PER_DAY = 7.5m;
    private const string RESOURCE_TYPE_SALARIE = "Salarie";
    private const string RESOURCE_TYPE_PIGISTE = "Pigiste";
    private const string STATUS_OK = "OK";
    private const string STATUS_WARNING = "WARNING";
    private const string STATUS_KO = "KO";

    private readonly AppDbContext _context;

    public MarginSimulationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MarginSimulationResponse> SimulateAsync(MarginSimulationRequest request)
    {
        // Validate resource type
        if (request.ResourceType != RESOURCE_TYPE_SALARIE && request.ResourceType != RESOURCE_TYPE_PIGISTE)
        {
            throw new ArgumentException($"Type de ressource invalide. Valeurs acceptées : {RESOURCE_TYPE_SALARIE}, {RESOURCE_TYPE_PIGISTE}");
        }

        // Load client
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClientId);

        if (client == null)
        {
            throw new ArgumentException($"Client avec l'ID {request.ClientId} introuvable.");
        }

        // Validate client financial configuration
        if (!client.IsFinancialConfigComplete)
        {
            throw new InvalidOperationException(
                "Les paramètres financiers du client sont incomplets. " +
                "Veuillez configurer : marge cible, marge minimale, remise, jours de vacances forcés et vendant cible.");
        }

        // Load active global salary settings (only needed for Salarie)
        Domain.Entities.GlobalSalarySettings? globalSettings = null;
        if (request.ResourceType == RESOURCE_TYPE_SALARIE)
        {
            globalSettings = await _context.GlobalSalarySettings
                .AsNoTracking()
                .Where(g => g.IsActive && !g.IsDeleted)
                .FirstOrDefaultAsync();

            if (globalSettings == null)
            {
                throw new InvalidOperationException(
                    "Aucun paramètre global de salaire actif trouvé. " +
                    "Veuillez configurer les paramètres globaux pour les calculs de ressources salariées.");
            }
        }

        // Calculate cost per hour
        decimal costPerHour = CalculateCostPerHour(request, client, globalSettings);

        // Calculate target results (CFO objectives)
        var targetResults = CalculateTargetResults(
            costPerHour,
            client.TargetHourlyRate!.Value,
            client.DiscountPercent!.Value,
            client.DefaultTargetMarginPercent!.Value,
            client.DefaultMinimumMarginPercent!.Value,
            client.ForcedVacationDaysPerYear!.Value);

        // Calculate proposed results (simulation with proposed bill rate)
        var proposedResults = CalculateProposedResults(
            costPerHour,
            request.ProposedBillRate,
            targetResults.EffectiveTargetBillRate,
            client.DefaultTargetMarginPercent!.Value,
            client.DefaultMinimumMarginPercent!.Value);

        return new MarginSimulationResponse
        {
            TargetResults = targetResults,
            ProposedResults = proposedResults
        };
    }

    private decimal CalculateCostPerHour(
        MarginSimulationRequest request,
        Domain.Entities.Client client,
        Domain.Entities.GlobalSalarySettings? globalSettings)
    {
        if (request.ResourceType == RESOURCE_TYPE_PIGISTE)
        {
            // For freelancers, cost per hour = proposed bill rate (as a proxy)
            return request.ProposedBillRate;
        }
        else // RESOURCE_TYPE_SALARIE
        {
            if (!request.AnnualGrossSalary.HasValue)
            {
                throw new ArgumentException("Le salaire annuel brut est requis pour une ressource salariée.");
            }

            if (globalSettings == null)
            {
                throw new InvalidOperationException("Les paramètres globaux sont requis pour calculer le coût d'un salarié.");
            }

            // Calculate employer charges factor (e.g., 1.65 for 65%)
            decimal employerChargesFactor = 1 + (globalSettings.EmployerChargesRate / 100m);

            // Total annual salary cost with employer charges
            decimal totalAnnualSalaryCost = request.AnnualGrossSalary.Value * employerChargesFactor;

            // Total annual cost including indirect costs
            decimal totalAnnualCost = totalAnnualSalaryCost + globalSettings.IndirectAnnualCosts;

            // Calculate effective billable hours considering forced vacation days
            decimal forcedVacationHours = client.ForcedVacationDaysPerYear!.Value * HOURS_PER_DAY;
            decimal effectiveBillableHours = globalSettings.BillableHoursPerYear - forcedVacationHours;

            // Safety check to avoid division by zero
            if (effectiveBillableHours <= 0)
            {
                effectiveBillableHours = 1;
            }

            // Cost per hour
            return totalAnnualCost / effectiveBillableHours;
        }
    }

    private TargetResults CalculateTargetResults(
        decimal costPerHour,
        decimal targetBillRate,
        decimal discountPercent,
        decimal targetMarginPercent,
        decimal minMarginPercent,
        int forcedVacationDaysPerYear)
    {
        // Calculate effective target bill rate after discount
        decimal effectiveTargetBillRate = targetBillRate * (1 - (discountPercent / 100m));

        // Avoid division by zero
        if (effectiveTargetBillRate <= 0)
        {
            effectiveTargetBillRate = 0.01m;
        }

        // Calculate theoretical margin with discount and vacation days considered
        decimal theoreticalMarginPercent = 
            ((effectiveTargetBillRate - costPerHour) / effectiveTargetBillRate) * 100m;
        decimal theoreticalMarginPerHour = effectiveTargetBillRate - costPerHour;

        // Determine status
        string status = DetermineStatus(theoreticalMarginPercent, targetMarginPercent, minMarginPercent);

        return new TargetResults
        {
            CostPerHour = Math.Round(costPerHour, 2),
            EffectiveTargetBillRate = Math.Round(effectiveTargetBillRate, 2),
            TheoreticalMarginPercent = Math.Round(theoreticalMarginPercent, 2),
            TheoreticalMarginPerHour = Math.Round(theoreticalMarginPerHour, 2),
            ConfiguredTargetMarginPercent = Math.Round(targetMarginPercent, 2),
            ConfiguredMinMarginPercent = Math.Round(minMarginPercent, 2),
            ConfiguredDiscountPercent = Math.Round(discountPercent, 2),
            ForcedVacationDaysPerYear = forcedVacationDaysPerYear,
            Status = status
        };
    }

    private ProposedResults CalculateProposedResults(
        decimal costPerHour,
        decimal proposedBillRate,
        decimal effectiveTargetBillRate,
        decimal targetMarginPercent,
        decimal minMarginPercent)
    {
        // Avoid division by zero
        if (proposedBillRate <= 0)
        {
            proposedBillRate = 0.01m;
        }

        // Calculate margin with proposed bill rate
        decimal marginPercentProposed = 
            ((proposedBillRate - costPerHour) / proposedBillRate) * 100m;
        decimal marginPerHourProposed = proposedBillRate - costPerHour;

        // Calculate discount applied vs target bill rate
        decimal? discountPercentApplied = null;
        if (effectiveTargetBillRate > 0)
        {
            discountPercentApplied = 
                ((effectiveTargetBillRate - proposedBillRate) / effectiveTargetBillRate) * 100m;
        }

        // Determine status
        string status = DetermineStatus(marginPercentProposed, targetMarginPercent, minMarginPercent);

        return new ProposedResults
        {
            ProposedBillRate = Math.Round(proposedBillRate, 2),
            MarginPercent = Math.Round(marginPercentProposed, 2),
            MarginPerHour = Math.Round(marginPerHourProposed, 2),
            DiscountPercentApplied = discountPercentApplied.HasValue 
                ? Math.Round(discountPercentApplied.Value, 2) 
                : null,
            Status = status
        };
    }

    private string DetermineStatus(decimal actualMarginPercent, decimal targetMarginPercent, decimal minMarginPercent)
    {
        if (actualMarginPercent >= targetMarginPercent)
        {
            return STATUS_OK;
        }
        else if (actualMarginPercent >= minMarginPercent)
        {
            return STATUS_WARNING;
        }
        else
        {
            return STATUS_KO;
        }
    }
}
