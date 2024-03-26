using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using VNPAY_CS_ASPX;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyService _companyService;
        private readonly IBillRepository _billRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenderBidService _tenderBidService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public VnPayService(IConfiguration configuration, 
            IHttpContextAccessor contextAccessor, 
            ITenderBidRepository tenderBidRepository, 
            IUserRepository userRepository,
            ICompanyService companyService,
            IBillRepository billRepository,
            ITenderRepository tenderRepository,
            IUnitOfWork unitOfWork,
            ITenderBidService tenderBidService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _tenderBidRepository = tenderBidRepository;
            _userRepository = userRepository;
            _companyService = companyService;
            _billRepository = billRepository;
            _tenderRepository = tenderRepository;
            _unitOfWork = unitOfWork;
            _tenderBidService = tenderBidService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<GeneralResponse> TransacVNPay(int TenderBidId, ApplicationUser _currentUser)
        {
            try
            {
                var tenderBid = await _tenderBidRepository
                    .AsNoTracking()
                    .Where(t => t.Id == TenderBidId)
                    .FirstOrDefaultAsync();

                if(tenderBid == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy đặt giá");
                }

                var company = await _companyService.GetCompanyByCurrentUserId(_currentUser);

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
                vnpay.AddRequestData("vnp_Amount", ((tenderBid!.TransportPrice + tenderBid!.TransportPrice*company.Data.ShipperFeePercentage/100) * 100).ToString(CultureInfo.InvariantCulture)); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
                var locale = string.Empty;

                vnpay.AddRequestData("vnp_Locale", !string.IsNullOrEmpty(locale) ? locale : "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Công ty thuê vận chuyển {company.Data.CompanyName} thanh toán tiền của thầu số #000{tenderBid.TenderId}");
                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);

                vnpay.AddRequestData("vnp_TxnRef", $"{tenderBid.Id}_{Guid.NewGuid()}"); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                //Add Params of 2.1.0 Version
                vnpay.AddRequestData("vnp_ExpireDate", string.Empty);
                //Billing
                vnpay.AddRequestData("vnp_Bill_Mobile", _currentUser.PhoneNumber == null ? string.Empty : _currentUser.PhoneNumber);
                vnpay.AddRequestData("vnp_Bill_Email", _currentUser.Email == null ? string.Empty : _currentUser.Email);

                vnpay.AddRequestData("vnp_Bill_FirstName", _currentUser.FirstName);
                vnpay.AddRequestData("vnp_Bill_LastName", _currentUser.LastName);

                vnpay.AddRequestData("vnp_Bill_Address", _currentUser.Address == null ? string.Empty : _currentUser.Address);
                vnpay.AddRequestData("vnp_Bill_City", string.Empty);
                vnpay.AddRequestData("vnp_Bill_Country", string.Empty);
                vnpay.AddRequestData("vnp_Bill_State", "");

                // Invoice

                vnpay.AddRequestData("vnp_Inv_Phone", _currentUser.PhoneNumber == null ? string.Empty : _currentUser.PhoneNumber);
                vnpay.AddRequestData("vnp_Inv_Email", _currentUser.Email == null ? string.Empty : _currentUser.Email);
                vnpay.AddRequestData("vnp_Inv_Customer", $"{_currentUser.FirstName} {_currentUser.LastName}");
                vnpay.AddRequestData("vnp_Inv_Address", _currentUser.Address == null ? string.Empty : _currentUser.Address);
                vnpay.AddRequestData("vnp_Inv_Company", company.Data.CompanyName);
                vnpay.AddRequestData("vnp_Inv_Taxcode", string.Empty);
                vnpay.AddRequestData("vnp_Inv_Type", string.Empty);

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return new GeneralResponse(true, paymentUrl);
            }catch(Exception ex)
            {
                return new GeneralResponse(false, "Có lỗi xảy ra " + ex.Message);
            }
        }

        public async Task<GeneralResponse> GetDataReturn_ShipperToAdminDinoTrans(Bill dto, ApplicationUser _currentUser)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var billForTender = await _billRepository
                    .AsNoTracking()
                    .Where(b => b.TenderBidId == dto.TenderBidId
                    && b.BillType == BillTypeEnum.ShipperToAdminDinoTrans)
                    .FirstOrDefaultAsync();

                var tenderBid = await _tenderBidRepository
                    .AsNoTracking()
                    .Where(tb => tb.Id == dto.TenderBidId)
                    .FirstOrDefaultAsync();

                if (billForTender != null)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, $"Giao dịch từ Shipper và Admin của thầu #000{tenderBid!.TenderId} đã được hoàn tất, không thể giao dịch lại");
                }
                VnPayLibrary vnpay = new VnPayLibrary();
                string vnp_HashSecret = _configuration.GetSection("AppSettings:HashSecret").Value!; //Secret Key
                bool checkSignature = vnpay.ValidateSignature(dto.vnp_SecureHash!, vnp_HashSecret);
                if (checkSignature)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, "Đã xảy ra lỗi trong quá trình xử lý");
                }
                if (dto.vnp_TransactionStatus != "00" && dto.vnp_ResponseCode != "00")
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, $"Đã xảy ra lỗi trong quá trình xử lý. Mã giao dịch: {dto.vnp_TransactionStatus}; mã phản hồi API: {dto.vnp_ResponseCode}");
                }
                await _tenderBidService.ChooseTenderBid(tenderBid!.Id, _currentUser);
                _billRepository.Add(dto);
                _billRepository.SaveChange();                
                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                return new GeneralResponse(true, "Giao dịch thành công");
            }
            catch(Exception ex)
            {
                _unitOfWork.Rollback();
                return new GeneralResponse(false, ex.Message);
            }
        }

        public async Task<GeneralResponse> TransacVNPay_FromAdmin(int TenderBidId)
        {
            try
            {
                var tenderBid = await _tenderBidRepository
                    .AsNoTracking()
                    .Where(t => t.Id == TenderBidId)
                    .FirstOrDefaultAsync();

                var carrierCompany = await _companyService.GetCompanyByCompanyId(tenderBid!.CompanyCarrierId);

                if (tenderBid == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy đặt giá");
                }

                var adminUser = (await _userManager.GetUsersInRoleAsync(Role.DinoTransAdmin)).FirstOrDefault();

                var company = await _companyService.GetCompanyByCurrentUserId(adminUser!);

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
                vnpay.AddRequestData("vnp_Amount", ((tenderBid!.TransportPrice - tenderBid!.TransportPrice * carrierCompany.Data.CarrierFeePercentage / 100) * 100).ToString(CultureInfo.InvariantCulture)); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
                var locale = string.Empty;

                vnpay.AddRequestData("vnp_Locale", !string.IsNullOrEmpty(locale) ? locale : "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"AdminDinoTrans thanh toan tien cho ben van chuyen cong ty {company.Data.CompanyName} hoa don cua thau so #000{tenderBid.TenderId}");
                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);

                vnpay.AddRequestData("vnp_TxnRef", $"{tenderBid.Id}_{Guid.NewGuid()}"); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                //Add Params of 2.1.0 Version
                vnpay.AddRequestData("vnp_ExpireDate", string.Empty);
                //Billing
                vnpay.AddRequestData("vnp_Bill_Mobile", adminUser.PhoneNumber == null ? string.Empty : adminUser.PhoneNumber);
                vnpay.AddRequestData("vnp_Bill_Email", adminUser.Email == null ? string.Empty : adminUser.Email);

                vnpay.AddRequestData("vnp_Bill_FirstName", adminUser.FirstName);
                vnpay.AddRequestData("vnp_Bill_LastName", adminUser.LastName);

                vnpay.AddRequestData("vnp_Bill_Address", adminUser.Address == null ? string.Empty : adminUser.Address);
                vnpay.AddRequestData("vnp_Bill_City", string.Empty);
                vnpay.AddRequestData("vnp_Bill_Country", string.Empty);
                vnpay.AddRequestData("vnp_Bill_State", "");

                // Invoice

                vnpay.AddRequestData("vnp_Inv_Phone", adminUser.PhoneNumber == null ? string.Empty : adminUser.PhoneNumber);
                vnpay.AddRequestData("vnp_Inv_Email", adminUser.Email == null ? string.Empty : adminUser.Email);
                vnpay.AddRequestData("vnp_Inv_Customer", $"{adminUser.FirstName} {adminUser.LastName}");
                vnpay.AddRequestData("vnp_Inv_Address", adminUser.Address == null ? string.Empty : adminUser.Address);
                vnpay.AddRequestData("vnp_Inv_Company", company.Data.CompanyName);
                vnpay.AddRequestData("vnp_Inv_Taxcode", string.Empty);
                vnpay.AddRequestData("vnp_Inv_Type", string.Empty);

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return new GeneralResponse(true, paymentUrl);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, "Có lỗi xảy ra " + ex.Message);
            }
        }

        public async Task<GeneralResponse> GetDataReturn_AdminDinoTransToCarrier(Bill dto)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var billForTender = await _billRepository
                    .AsNoTracking()
                    .Where(b => b.TenderBidId == dto.TenderBidId
                    && b.BillType == BillTypeEnum.AdminDinoTransToCarrier)
                    .FirstOrDefaultAsync();

                var tenderBid = await _tenderBidRepository
                    .AsNoTracking()
                    .Where(tb => tb.Id == dto.TenderBidId)
                    .FirstOrDefaultAsync();

                if (billForTender != null)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, $"Giao dịch từ Admin và Carrier của thầu #000{tenderBid!.TenderId} đã được hoàn tất, không thể giao dịch lại");
                }
                VnPayLibrary vnpay = new VnPayLibrary();
                string vnp_HashSecret = _configuration.GetSection("AppSettings:HashSecret").Value!; //Secret Key
                bool checkSignature = vnpay.ValidateSignature(dto.vnp_SecureHash!, vnp_HashSecret);
                if (checkSignature)
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, "Đã xảy ra lỗi trong quá trình xử lý");
                }
                if (dto.vnp_TransactionStatus != "00" && dto.vnp_ResponseCode != "00")
                {
                    _unitOfWork.Rollback();
                    return new GeneralResponse(false, $"Đã xảy ra lỗi trong quá trình xử lý. Mã giao dịch: {dto.vnp_TransactionStatus}; mã phản hồi API: {dto.vnp_ResponseCode}");
                }

                var tender = await _tenderRepository
                    .Queryable()
                    .Where(t => t.Id == tenderBid!.TenderId)
                    .FirstOrDefaultAsync();

                if (tender!.IsShipperComfirm && tender.IsCarrierComfirm) tender.TenderStatus = TenderStatuses.Completed;
                else
                {
                    return new GeneralResponse(false, "Chưa thể đóng thầu vì còn công ty vận chuyển hoặc bên thuê vận chuyển chưa xác nhận");
                }    
                _tenderRepository.Update(tender);
                _tenderRepository.SaveChange();
                _billRepository.Add(dto);
                _billRepository.SaveChange();
                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                return new GeneralResponse(true, "Giao dịch thành công");
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return new GeneralResponse(false, ex.Message);
            }
        }
    }
}
