using System.ComponentModel.DataAnnotations;

﻿
namespace TaRge25Shop.Models.Kindergarden
{
    public class KindergardenIndexViewModel
    {
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Rühma nimi")]
        public string GroupName { get; set; } = string.Empty;
        [Display(Name = "Laste arv")]
        public int ChildrenCount { get; set; }
        [Display(Name = "Lasteaia nimi")]
        public string KindergardenName { get; set; } = string.Empty;
        [Display(Name = "Õpetaja nimi")]
        public string TeacherName { get; set; } = string.Empty;
        [Display(Name = "Lisatud")]
        public DateTime CreatedAt { get; set; }
    }
}
