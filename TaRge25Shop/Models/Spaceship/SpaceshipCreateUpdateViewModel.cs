using System.ComponentModel.DataAnnotations;

﻿namespace TaRge25Shop.Models.Spaceship
{
    public class SpaceshipCreateUpdateViewModel
    {
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Nimi")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Laeva tüüp")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string ShipType { get; set; } = string.Empty;
        [Display(Name = "Meeskonna suurus")]
        public int Crew { get; set; }
        [Display(Name = "Mootori võimsus")]
        public int EnginePower { get; set; }
        [Display(Name = "Pildid")]
        public List<IFormFile> Files { get; set; } = new();
        [Display(Name = "Pilt")]
        public List<ImageViewModel> Image { get; set; }
            = new List<ImageViewModel>();
        [Display(Name = "Lisatud")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Muudetud")]
        public DateTime UpdatedAt { get; set; }
    }
}
