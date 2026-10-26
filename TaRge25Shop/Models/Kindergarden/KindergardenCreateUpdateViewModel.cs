using System.ComponentModel.DataAnnotations;

namespace TaRge25Shop.Models.Kindergarden
{
    public class KindergardenCreateUpdateViewModel
    {
        [Display(Name = "Pildid")]
        public List<KindergardenImageViewModel> Images { get; set; } = new();
        [Display(Name = "Pildid")]
        public List<IFormFile> Files { get; set; } = new();
        [Display(Name = "Tunnus")]
        public Guid? Id { get; set; }
        [Display(Name = "Rühma nimi")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string GroupName { get; set; } = string.Empty;
        [Display(Name = "Laste arv")]
        public int ChildrenCount { get; set; }
        [Display(Name = "Lasteaia nimi")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string KindergardenName { get; set; } = string.Empty;
        [Display(Name = "Õpetaja nimi")]
        [Required(ErrorMessage = "{0} on kohustuslik.")]
        public string TeacherName { get; set; } = string.Empty;
        [Display(Name = "Lisatud")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Muudetud")]
        public DateTime UpdatedAt { get; set; }
    }
}
