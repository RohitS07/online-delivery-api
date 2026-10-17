namespace OnlineDelivery.Api.Entities;

public class City : IActivatable
{
    public int Id { get; set; }
    public required string NameEn { get; set; }
    public required string NameMr { get; set; }
    public bool IsActive { get; set; }
    public List<Village> Villages { get; set; } = [];
}
