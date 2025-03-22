using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dto.Address;

public class AddressForCreationDto
{
    [Required(ErrorMessage = "Country ID is required.")]
    public int CountryId { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(100, ErrorMessage = "Customer name can't exceed 100 characters.")]
    public string CustomerName { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; }

    [StringLength(50, ErrorMessage = "Street number can't exceed 50 characters.")]
    public string StreetNumber { get; set; }

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string AddressLine1 { get; set; }

    [StringLength(150, ErrorMessage = "Address Line 2 can't exceed 150 characters.")]
    public string AddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, ErrorMessage = "City name can't exceed 100 characters.")]
    public string City { get; set; }

    [StringLength(100, ErrorMessage = "Ward name can't exceed 100 characters.")]
    public string Ward { get; set; }

    [StringLength(10, ErrorMessage = "Postcode can't exceed 10 characters.")]
    public string Postcode { get; set; }

    [StringLength(100, ErrorMessage = "Province name can't exceed 100 characters.")]
    public string Province { get; set; }

    public bool IsDefault { get; set; }
}
