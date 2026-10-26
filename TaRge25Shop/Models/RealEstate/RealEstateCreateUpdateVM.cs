using System.ComponentModel.DataAnnotations;

﻿namespace TaRge25Shop.Models.RealEstate
{
    public class RealEstateCreateUpdateVM
    {
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Pindala")]
        public double? Area { get; set; }
        [Display(Name = "Asukoht")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string Location { get; set; } = string.Empty;
        [Display(Name = "Tubade arv")]
        public int RoomNumber { get; set; }
        [Display(Name = "Hoone tüüp")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string BuildingType { get; set; } = string.Empty;
        [Display(Name = "Lisatud")]
        public DateTime? CreatedAt { get; set; }
        [Display(Name = "Muudetud")]
        public DateTime? ModifiedAt { get; set; }
        [Display(Name = "Pildid")]
        public List<IFormFile> Files { get; set; } = new();
        [Display(Name = "Pildid")]
        public List<RealEstateImageVM> Images { get; set; } = new List<RealEstateImageVM>();
    }
}
