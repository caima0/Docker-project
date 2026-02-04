namespace api.Dtos
{
    public class CurrencyExchangeResponse
    {
        public required string SourceCurrency { get; set; }
        public required string TargetCurrency { get; set; }
        public decimal SourceAmount { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public required string TransactionId { get; set; }
        public required DateTime Timestamp { get; set; }
    }
} 