namespace TaRge25Shop.Models.RealEstate
{
    public class RealEstateDetailVM
    {
        public Guid? Id { get; set; }
        public double? Area { get; set; }
        public string Location { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public string BuildingType { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<RealEstateImageVM> Images { get; set; } = new();
    }
}
