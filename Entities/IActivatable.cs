namespace OnlineDelivery.Api.Entities;

/// <summary>Master data is made inactive, never deleted: SPEC-0002.</summary>
public interface IActivatable
{
    bool IsActive { get; set; }
}
