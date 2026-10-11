namespace TaRge25Shop.Models.RealEstate
{
    public class RealEstateImageVM
    {
        public Guid ImageId { get; set; }
        public string? ImageTitle { get; set; } = string.Empty;
        public byte[]? ImageData { get; set; }
        public string? Image { get; set; }
        public Guid? RealEstateId { get; set; }
    }
}
