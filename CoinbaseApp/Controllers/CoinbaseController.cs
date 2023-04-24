using Coinbase.Commerce;
using Coinbase.Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace CoinbaseApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CoinbaseController : ControllerBase
    {
        private static readonly string CommerceApiKey = "4d183389-a1cb-422a-acd0-3a87e42f0d2b";
        private static readonly string SharedSecret = "3989a6c6-6c42-434a-aef5-69193de49484";

        [HttpGet("CreateCharge")]
        public async Task<IActionResult> CreateCharge(string email, string name, string phone, string comment, string client_type, string mode_type)
        {
            var commerceApi = new CommerceApi(CommerceApiKey);

            var charge = new CreateCharge
            {
                Name = "WeTalentAI",
                Description = "Sweet Tasting Chocolate",
                PricingType = PricingType.FixedPrice,
                LocalPrice = new Money { Amount = 15.00m, Currency = "USD" },
                Metadata = 
                 {       
                    { "userEmail", email }                
                },
            };

            var response = await commerceApi.CreateChargeAsync(charge);

            if (response.HasError())
            {
                return Problem(response.Error.ToString());
            }
          
            return Ok(response.Data.HostedUrl);
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> Webhook()
        {
            var requestSignature = Request.Headers[HeaderNames.WebhookSignature];

            Request.EnableBuffering();
            var json = await new System.IO.StreamReader(Request.Body).ReadToEndAsync();
            Request.Body.Position = 0;

            if (!WebhookHelper.IsValid(SharedSecret, requestSignature, json))
            {
                return BadRequest();
            }

            var webhook = JsonConvert.DeserializeObject<Webhook>(json);

            using var client = new HttpClient();

            var payment = new Payment
            {
                Email = webhook.Event.Data["metadata"]["userEmail"].ToString(),
                PaymentDate = webhook.Event.CreatedAt.UtcDateTime,
                PaymentType = webhook.Event.Type
            };

            var paymentJson = JsonConvert.SerializeObject(payment);
            var data = new StringContent(paymentJson, Encoding.UTF8, "application/json");
            var url = "https://hook.eu1.make.com/vgtoie5meazavjwwar7hqcy73a9hadmg";
            var response = await client.PostAsync(url, data);

            //if (webhook.Event.IsChargeConfirmed)
            //{
            //    var charge = webhook.Event.DataAs<Charge>();

            //    if (charge.Name == "PRODUCT_NAME")
            //    {
            //        //THE PAYMENT IS SUCCESSFUL
            //        //DO SOMETHING TO MARK THE PAYMENT IS COMPLETE
            //        //IN YOUR DATABASE
            //    }
            //}

            return Ok(response);
        }
    }
}
