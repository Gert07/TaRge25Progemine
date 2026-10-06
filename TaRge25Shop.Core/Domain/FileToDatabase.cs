using System;
using System.Collections.Generic;
using System.Text;

namespace TaRge25Shop.Core.Domain
{
    public class FileToDatabase
    {
        public Guid Id { get; set; }
        public string? ImageTitle { get; set; }
        public byte[]? ImageData { get; set; }
        public Guid? KindergardenId { get; set; }
    }
}
