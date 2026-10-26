using System.ComponentModel.DataAnnotations;

﻿namespace TaRge25Shop.Models.RealEstate
{
    public class RealEstateIndexVM
    {
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Pindala")]
        public double? Area { get; set; }
        [Display(Name = "Asukoht")]
        public string Location { get; set; } = string.Empty;
        [Display(Name = "Tubade arv")]
        public int RoomNumber { get; set; }
        [Display(Name = "Hoone tüüp")]
        public string BuildingType { get; set; } = string.Empty;
        [Display(Name = "Lisatud")]
        public DateTime? CreatedAt { get; set; }
        [Display(Name = "Muudetud")]
        public DateTime? ModifiedAt { get; set; }
    }
}
