using Microsoft.AspNetCore.Http;


namespace TaRge25Shop.Core.Dto
{
    public class KindergardenDto
    {
        public List<IFormFile> Files { get; set; } = new();
        public Guid? Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int ChildrenCount { get; set; }
        public string KindergardenName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
