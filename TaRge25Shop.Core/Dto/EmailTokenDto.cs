using System;
using System.Collections.Generic;
using System.Text;

namespace TaRge25Shop.Core.Dto
{
    public class EmailTokenDto : EmailDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
