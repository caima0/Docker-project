using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace api.Models
{
    public class ResponseItem
    {
        public int Id { get; set; }
        public required string Table { get; set; }
        public required string No { get; set; }
        public required string TradingDate { get; set; }
        public required string EffectiveDate { get; set; }
        public ICollection<Rate> Rates { get; set; } = new List<Rate>();
    }
}