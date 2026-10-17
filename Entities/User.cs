namespace OnlineDelivery.Api.Entities;

/// <summary>A person who signs in with a mobile number: SPEC-0003.</summary>
public class User
{
    public Guid Id { get; set; }
    public required string MobileNumber { get; set; }
    public string Name { get; set; } = "";
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public enum UserRole
{
    Customer,
    Agent,
    Admin,
}

public enum UserStatus
{
    Active,
    Waiting,
    Blocked,
}
