using Newtonsoft.Json;

namespace CoinbaseApp
{
    public class Payment
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("payment_date")]
        public DateTime PaymentDate { get; set; }
    }
}
