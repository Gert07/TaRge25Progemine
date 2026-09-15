namespace TaRge25Shop.Models.Spaceship
{
    public class SpaceshipIndexViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShipType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Crew { get; set; }
    }
}
