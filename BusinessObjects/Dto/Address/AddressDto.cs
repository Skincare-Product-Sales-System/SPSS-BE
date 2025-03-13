namespace BusinessObjects.Dto.Address;

public class AddressDto
{
    public Guid Id { get; set; }

    public string CustomerName { get; set; }

    public string PhoneNumber { get; set; }

    public string CountryName { get; set; }

    public string StreetNumber { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string Ward { get; set; }

    public string Postcode { get; set; }

    public string Province { get; set; }
}