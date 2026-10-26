using System.ComponentModel.DataAnnotations;

﻿namespace TaRge25Shop.Models.Spaceship
{
    public class SpaceshipIndexViewModel
    {
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Nimi")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Laeva tüüp")]
        public string ShipType { get; set; } = string.Empty;
        [Display(Name = "Lisatud")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Meeskonna suurus")]
        public int Crew { get; set; }
    }
}
