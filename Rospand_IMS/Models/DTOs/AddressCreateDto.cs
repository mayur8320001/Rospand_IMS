namespace Rospand_IMS.Models.DTOs
{
    public class AddressCreateDto
    {
        public string? Attention { get; set; }
        public string? ContactNo { get; set; }
        public int? CountryId { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public string? ZipCode { get; set; }
        public string? StreetAddress { get; set; }
    }
}
