namespace APG.Application.DTOs;

/// <summary>
/// Request model for market trends analysis
/// </summary>
public class MarketTrendsRequest
{
    public string Role { get; set; } = string.Empty;
    public string? Seniority { get; set; }
    public string ResourceType { get; set; } = string.Empty; // "Employee" or "Freelancer"
    public string? Location { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal? ProposedAnnualSalary { get; set; }
    public decimal? ProposedBillRate { get; set; }
    public string? ClientName { get; set; }
    public string? BusinessUnit { get; set; }
}

/// <summary>
/// Response model for market trends analysis
/// </summary>
public class MarketTrendsResponse
{
    // Ranges by seniority level
    public SalaryRangeByLevel? SalaryRangeByLevel { get; set; }
    public FreelanceRateRangeByLevel? FreelanceRateRangeByLevel { get; set; }
    
    // Original ranges (for the specified seniority or default)
    public SalaryRange SalaryRange { get; set; } = new();
    public FreelanceRateRange FreelanceRateRange { get; set; } = new();
    
    public string EmployeePositioning { get; set; } = string.Empty; // "far_below" | "below" | "in_line" | "above" | "far_above"
    public string FreelancePositioning { get; set; } = string.Empty; // "far_below" | "below" | "in_line" | "above" | "far_above"
    public string MarketDemand { get; set; } = string.Empty; // "low" | "medium" | "high" | "very_high"
    public string RiskLevel { get; set; } = string.Empty; // "low" | "medium" | "high"
    public string Summary { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? RawModelOutput { get; set; }
}

/// <summary>
/// Salary ranges by seniority level
/// </summary>
public class SalaryRangeByLevel
{
    public SalaryRange Junior { get; set; } = new();
    public SalaryRange Intermediate { get; set; } = new();
    public SalaryRange Senior { get; set; } = new();
}

/// <summary>
/// Freelance rate ranges by seniority level
/// </summary>
public class FreelanceRateRangeByLevel
{
    public FreelanceRateRange Junior { get; set; } = new();
    public FreelanceRateRange Intermediate { get; set; } = new();
    public FreelanceRateRange Senior { get; set; } = new();
}

/// <summary>
/// Salary range with currency information
/// </summary>
public class SalaryRange
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Freelance rate range with currency information
/// </summary>
public class FreelanceRateRange
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public string Currency { get; set; } = string.Empty;
}
