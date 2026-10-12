namespace TaRge25Shop.Models.Spaceship
{
    public class SpaceshipCreateUpdateViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Range(4, int.MaxValue, ErrorMessage = "Crew must contain at least 4 members.")]
        public int Crew { get; set; }
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Engine power must be greater than zero.")]
        public int EnginePower { get; set; }
        public List<IFormFile> Files { get; set; } = new();
        public List<ImageViewModel> Image { get; set; }
            = new List<ImageViewModel>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
