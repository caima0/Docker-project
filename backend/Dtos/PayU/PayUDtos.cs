using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api.Dtos.PayU
{
    public class PayUResponse
    {
        public required string RedirectUri { get; set; }
        public required string OrderId { get; set; }
        public required string ExtOrderId { get; set; }
        public required Status Status { get; set; }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        [JsonPropertyName("grant_type")]
        public required string GrantType { get; set; }
    }

    public class Status
    {
        public required string StatusCode { get; set; }
        public required string StatusDesc { get; set; }
        public required string Code { get; set; }
        public required string CodeLiteral { get; set; }
    }

    public class OrderStatus
    {
        public required List<Order> Orders { get; set; } = new();
        public required Status Status { get; set; }
    }

    public class Order
    {
        public required string OrderId { get; set; }
        public required string ExtOrderId { get; set; }
        public required string OrderCreateDate { get; set; }
        public required string NotifyUrl { get; set; }
        public required string CustomerIp { get; set; }
        public required string MerchantPosId { get; set; }
        public required string Description { get; set; }
        public required string CurrencyCode { get; set; }
        public required string TotalAmount { get; set; }
        public required string Status { get; set; }
        public required List<Product> Products { get; set; } = new();
    }

    public class Product
    {
        public required string Name { get; set; }
        public required string UnitPrice { get; set; }
        public required string Quantity { get; set; }
    }

    public class PayUStatus
    {
        [JsonPropertyName("statusCode")]
        public required string StatusCode { get; set; }

        [JsonPropertyName("statusDesc")]
        public required string StatusDesc { get; set; }

        [JsonPropertyName("redirectUri")]
        public required string RedirectUri { get; set; }
    }
} 