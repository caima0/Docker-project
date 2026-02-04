using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.Rate
{
    public class RateDto
    {
        public int Id { get; set; }
        public required string Currency { get; set; } = string.Empty;
        public required string Code { get; set; } = string.Empty;
        public required double Bid { get; set; }
        public required double Ask { get; set; }
    }
}