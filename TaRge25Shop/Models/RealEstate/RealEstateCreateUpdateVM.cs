namespace TaRge25Shop.Models.RealEstate
{
    public class RealEstateCreateUpdateVM
    {
        public Guid? Id { get; set; }
        public double? Area { get; set; }
        public string Location { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public string BuildingType { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<IFormFile> Files { get; set; } = new();
        public List<RealEstateImageVM> Images { get; set; } = new List<RealEstateImageVM>();
    }
}
