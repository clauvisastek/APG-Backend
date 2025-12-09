namespace APG.Domain.Entities;

/// <summary>
/// Sample test entity to verify database connectivity and migrations
/// </summary>
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
