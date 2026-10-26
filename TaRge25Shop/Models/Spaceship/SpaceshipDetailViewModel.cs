using System.ComponentModel.DataAnnotations;

﻿namespace TaRge25Shop.Models.Spaceship
{
    public class SpaceshipDetailViewModel
    {
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Nimi")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Laeva tüüp")]
        public string ShipType { get; set; } = string.Empty;
        [Display(Name = "Meeskonna suurus")]
        public int Crew { get; set; }
        [Display(Name = "Mootori võimsus")]
        public int EnginePower { get; set; }
        [Display(Name = "Pilt")]
        public List<ImageViewModel> Image { get; set; }
            = new List<ImageViewModel>();
        [Display(Name = "Lisatud")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Muudetud")]
        public DateTime UpdatedAt { get; set; }

    }
}
