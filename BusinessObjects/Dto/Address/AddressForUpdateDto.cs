namespace BusinessObjects.Dto.Address;

public class AddressForUpdateDto
{
    public Guid Id { get; set; }
    public int UserId { get; set; }

    public int CountryId { get; set; }

    public string StreetNumber { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string Ward { get; set; }

    public string Postcode { get; set; }

    public string Province { get; set; }

    public bool IsDefault { get; set; }

    public string CreatedBy { get; set; }

    public string LastUpdatedBy { get; set; }

    public string DeletedBy { get; set; }

    public DateTimeOffset CreatedTime { get; set; }

    public DateTimeOffset? LastUpdatedTime { get; set; }

    public DateTimeOffset? DeletedTime { get; set; }

    public bool IsDeleted { get; set; }
}