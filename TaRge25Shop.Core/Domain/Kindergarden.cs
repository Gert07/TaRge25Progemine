using System;
using System.Collections.Generic;
using System.Text;

namespace TaRge25Shop.Core.Domain
{
    public class Kindergarden
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int ChildrenCount { get; set; }
        public string KindergardenName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
