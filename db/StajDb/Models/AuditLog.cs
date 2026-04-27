namespace StajDb.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public StoreUser? User { get; set; }

    public string Action { get; set; } = null!;

    public string? Details { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
