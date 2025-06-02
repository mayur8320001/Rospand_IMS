namespace Rospand_IMS.Models;

public class Address
{
    public int Id { get; set; }
    public string? Attention { get; set; }
    public string? ContactNo { get; set; }
    public int? CountryId { get; set; }
    public int? StateId { get; set; }
    public int? CityId { get; set; }
    public string? ZipCode { get; set; }
    public string? StreetAddress { get; set; }

    // Navigation properties
    public Country? Country { get; set; }
    public State? State { get; set; }
    public City? City { get; set; }
}
