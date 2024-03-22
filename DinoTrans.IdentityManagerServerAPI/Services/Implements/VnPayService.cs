using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using VNPAY_CS_ASPX;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public VnPayService(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }

        public string TransacVNPay(int TenderId)
        {
            //Get Config Info
            string vnp_Returnurl = _configuration.GetSection("AppSettings:ReturnVnPayUrl").Value!; //URL nhan ket qua tra ve 
            string vnp_Url = _configuration.GetSection("AppSettings:TransacVnPayUrl").Value!; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _configuration.GetSection("AppSettings:TmnCode").Value!; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = _configuration.GetSection("AppSettings:HashSecret").Value!; //Secret Key

            //Get payment input

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            //Build URL for VNPAY
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (100000 * 100).ToString(CultureInfo.InvariantCulture)); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            var locale = string.Empty;

            vnpay.AddRequestData("vnp_Locale", !string.IsNullOrEmpty(locale) ? locale : "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {TenderId}");
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);

            vnpay.AddRequestData("vnp_TxnRef", $"{TenderId}_{Guid.NewGuid()}"); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            vnpay.AddRequestData("vnp_ExpireDate", string.Empty);
            //Billing
            vnpay.AddRequestData("vnp_Bill_Mobile", string.Empty);
            vnpay.AddRequestData("vnp_Bill_Email", string.Empty);
            var fullName = string.Empty;
            if (!String.IsNullOrEmpty(fullName))
            {
                var indexof = fullName.IndexOf(' ');
                vnpay.AddRequestData("vnp_Bill_FirstName", fullName.Substring(0, indexof));
                vnpay.AddRequestData("vnp_Bill_LastName",
                    fullName.Substring(indexof + 1, fullName.Length - indexof - 1));
            }

            vnpay.AddRequestData("vnp_Bill_Address", string.Empty);
            vnpay.AddRequestData("vnp_Bill_City", string.Empty);
            vnpay.AddRequestData("vnp_Bill_Country", string.Empty);
            vnpay.AddRequestData("vnp_Bill_State", "");

            // Invoice

            vnpay.AddRequestData("vnp_Inv_Phone", string.Empty);
            vnpay.AddRequestData("vnp_Inv_Email", string.Empty);
            vnpay.AddRequestData("vnp_Inv_Customer", string.Empty);
            vnpay.AddRequestData("vnp_Inv_Address", string.Empty);
            vnpay.AddRequestData("vnp_Inv_Company", string.Empty);
            vnpay.AddRequestData("vnp_Inv_Taxcode", string.Empty);
            vnpay.AddRequestData("vnp_Inv_Type", string.Empty);

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }

        public async Task<VnPayReturnDTO> GetDataReturn (VnPayReturnDTO dto)
        {
            return dto;
        }
    }
}
