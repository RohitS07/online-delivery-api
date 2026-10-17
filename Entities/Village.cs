namespace OnlineDelivery.Api.Entities;

public class Village : IActivatable
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public City? City { get; set; }
    public required string NameEn { get; set; }
    public required string NameMr { get; set; }
    public int DeliveryHours { get; set; }
    public bool IsActive { get; set; }
}
