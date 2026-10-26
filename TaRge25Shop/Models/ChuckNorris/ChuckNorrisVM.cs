using System.ComponentModel.DataAnnotations;

﻿using System.Text.Json.Serialization;

namespace TaRge25Shop.Models.ChuckNorris
{
    public class ChuckNorrisVM
    {
        [Display(Name = "Kategooriad")]
        public string Categories { get; set; } = string.Empty;
        [Display(Name = "Lisatud")]
        public string CreatedAt { get; set; } = string.Empty;
        [Display(Name = "Ikooni aadress")]
        public string IconUrl { get; set; } = string.Empty;
        [Display(Name = "Tunnus")]
        public string Id { get; set; } = string.Empty;
        [Display(Name = "Muudetud")]
        public string UpdatedAt { get; set; } = string.Empty;
        [Display(Name = "Allika aadress")]
        public string Url { get; set; } = string.Empty;
        [Display(Name = "Nali")]
        public string Value { get; set; } = string.Empty;
    }
}
