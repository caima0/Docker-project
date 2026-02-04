using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.PayU
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
    }
}