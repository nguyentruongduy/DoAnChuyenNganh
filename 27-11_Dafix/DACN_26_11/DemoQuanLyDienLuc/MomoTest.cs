using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DemoQuanLyDienLuc
{
    public class MomoTest
    {
        private readonly string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
        private readonly string partnerCode = "MOMOBKUN20180529";
        private readonly string accessKey = "klm05TvNBzhg7h7j";
        private readonly string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
        private readonly string redirectUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";
        private readonly string ipnUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";

        public async Task<string> CreateTestPayment(string orderId, long amount, string orderInfo)
        {
            try
            {
                string requestId = DateTime.Now.Ticks.ToString();
                string rawHash = $"accessKey={accessKey}&amount={amount}&extraData=&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType=captureWallet";

                // Tạo chữ ký
                byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
                byte[] messageBytes = Encoding.UTF8.GetBytes(rawHash);
                string signature;
                using (var hmac = new HMACSHA256(keyBytes))
                {
                    byte[] hashBytes = hmac.ComputeHash(messageBytes);
                    signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }

                // Tạo request body
                var requestBody = new
                {
                    partnerCode = partnerCode,
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = redirectUrl,
                    ipnUrl = ipnUrl,
                    requestType = "captureWallet",
                    extraData = "",
                    lang = "vi",
                    signature = signature
                };

                // Gọi API
                using (var client = new HttpClient())
                {
                    var jsonRequest = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(endpoint, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse response
                    dynamic json = JsonConvert.DeserializeObject(responseContent);
                    if (json.resultCode == 0)
                    {
                        return json.payUrl;
                    }
                    else
                    {
                        throw new Exception($"Lỗi từ MOMO: {json.message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tạo giao dịch test: {ex.Message}");
            }
        }
    }
}
