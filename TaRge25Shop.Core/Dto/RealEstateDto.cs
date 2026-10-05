using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaRge25Shop.Core.Dto
{
    public class RealEstateDto
    {
        public Guid? Id { get; set; }
        public double? Area { get; set; }
        public string Location { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public string BuildingType { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<IFormFile> Files { get; set; } = new();
        public IEnumerable<FileToDatabaseDto> Image { get; set; }
            = new List<FileToDatabaseDto>();
    }
}
