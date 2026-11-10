using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TaRge25Shop.Core.Dto
{
    public class EmailDto
    {
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    }
}
