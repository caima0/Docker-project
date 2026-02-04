using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dtos.PayU
{
    public class PaymentNotification
    {
        public required Order[] Orders { get; set; }

        public class Order
        {
            public required string OrderId { get; set; }
            public required string ExtOrderId { get; set; }
            public required string OrderStatus { get; set; }
            public required decimal TotalAmount { get; set; }
            public required string CurrencyCode { get; set; }
        }
    }
}
