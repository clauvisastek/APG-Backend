namespace APG.Application.DTOs;

/// <summary>
/// DTO for global salary settings
/// </summary>
public class GlobalSalarySettingsDto
{
    public int Id { get; set; }
    public decimal EmployerChargesRate { get; set; }
    public decimal IndirectAnnualCosts { get; set; }
    public int BillableHoursPerYear { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response for getting active global salary settings
/// </summary>
public class ActiveGlobalSalarySettingsResponse
{
    public bool HasActiveSettings { get; set; }
    public GlobalSalarySettingsDto? Settings { get; set; }
}

/// <summary>
/// Request DTO for creating global salary settings
/// </summary>
public class CreateGlobalSalarySettingsRequest
{
    public decimal EmployerChargesRate { get; set; }
    public decimal IndirectAnnualCosts { get; set; }
    public int BillableHoursPerYear { get; set; }
}

/// <summary>
/// Request DTO for updating global salary settings
/// </summary>
public class UpdateGlobalSalarySettingsRequest
{
    public decimal EmployerChargesRate { get; set; }
    public decimal IndirectAnnualCosts { get; set; }
    public int BillableHoursPerYear { get; set; }
}

/// <summary>
/// DTO for client margin settings
/// </summary>
public class ClientMarginSettingsDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public decimal TargetMarginPercent { get; set; }
    public decimal TargetHourlyRate { get; set; }
}

/// <summary>
/// Request DTO for creating client margin settings
/// </summary>
public class CreateClientMarginSettingsRequest
{
    public int ClientId { get; set; }
    public decimal TargetMarginPercent { get; set; }
    public decimal TargetHourlyRate { get; set; }
}

/// <summary>
/// Request DTO for updating client margin settings
/// </summary>
public class UpdateClientMarginSettingsRequest
{
    public decimal TargetMarginPercent { get; set; }
    public decimal TargetHourlyRate { get; set; }
}
