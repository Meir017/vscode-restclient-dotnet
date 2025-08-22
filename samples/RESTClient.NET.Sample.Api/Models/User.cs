namespace RESTClient.NET.Sample.Api.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual List<Order> Orders { get; set; } = [];
}

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    Customer = 0,
    Admin = 1
}
