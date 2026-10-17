namespace OnlineDelivery.Api.Entities;

public class Category : IActivatable
{
    public int Id { get; set; }
    public required string NameEn { get; set; }
    public required string NameMr { get; set; }
    public bool IsActive { get; set; }
}
