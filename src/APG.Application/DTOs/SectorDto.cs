namespace APG.Application.DTOs;

/// <summary>
/// DTO for Sector entity
/// </summary>
public class SectorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? BusinessUnitId { get; set; }
}

/// <summary>
/// DTO for creating a new sector
/// </summary>
public class CreateSectorDto
{
    public string Name { get; set; } = string.Empty;
}
