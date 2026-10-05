using System;
using System.Collections.Generic;
using System.Text;

namespace TaRge25Shop.Core.Dto
{
    public class FileToDatabaseDto
    {
        public Guid Id { get; set; }
        public string? ImageTitle { get; set; }
        public byte[]? ImageData { get; set; }
        public Guid? RealEstateId { get; set; }
    }
}
