using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using System.Globalization;
using System.Net;

namespace DemoQuanLyDienLuc
{
    public class VNPayTest
    {
        private readonly string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        private readonly string vnp_TmnCode = "2QXUI4J4";
        private readonly string vnp_HashSecret = "NYYZTXVDGFWGTVBQDWJKVQSXJUIORNZN";
        private readonly string vnp_ReturnUrl = "http://localhost:8888/vnpay_return.aspx";

        public string CreatePaymentUrl(string orderId, long amount, string orderInfo)
        {
            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString());

            // Format ngày giờ theo đúng yêu cầu của VNPay
            string createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            vnpay.AddRequestData("vnp_CreateDate", createDate);

            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", HttpUtility.UrlEncode(orderInfo));
            vnpay.AddRequestData("vnp_OrderType", "billpayment"); // Sửa thành billpayment
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);
            vnpay.AddRequestData("vnp_BankCode", "NCB"); // Thêm mã ngân hàng test

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }


        public class VnPayLibrary
        {
            private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());

            public void AddRequestData(string key, string value)
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _requestData.Add(key, value);
                }
            }

            public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
            {
                StringBuilder data = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in _requestData)
                {
                    if (!String.IsNullOrEmpty(kv.Value))
                    {
                        data.Append(Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value) + "&");
                    }
                }

                string queryString = data.ToString();
                string rawData = queryString.Remove(queryString.Length - 1, 1);
                string vnp_SecureHash = HashString(vnp_HashSecret, rawData);
                baseUrl += "?" + queryString;
                baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

                return baseUrl;
            }

            private string HashString(string key, string data)
            {
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] keyBytes = encoding.GetBytes(key);
                byte[] messageBytes = encoding.GetBytes(data);

                using (HMACSHA512 hmac = new HMACSHA512(keyBytes))
                {
                    byte[] hashMessage = hmac.ComputeHash(messageBytes);
                    return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
                }
            }
        }

        public class VnPayCompare : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var vnpCompare = CompareInfo.GetCompareInfo("en-US");
                return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
            }
        }
    }
}
