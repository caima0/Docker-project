namespace api.Dtos
{
    public class CurrencyExchangeRequest
    {
        public required string SourceCurrency { get; set; }
        public required string TargetCurrency { get; set; }
        public decimal Amount { get; set; }
    }
} 