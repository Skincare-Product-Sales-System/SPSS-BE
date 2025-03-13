namespace BusinessObjects.Dto.Address;

public class AddressForUpdateDto
{
    public int UserId { get; set; }

    public int CountryId { get; set; }

    public string StreetNumber { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string Ward { get; set; }

    public string Postcode { get; set; }

    public string Province { get; set; }
}