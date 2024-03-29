using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using DinoTrans.Shared.DTOs.SearchDTO;
using System.Globalization;
using NHibernate.Util;
using Microsoft.IdentityModel.Tokens;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class BillService : IBillService
    {
        private readonly ICompanyService _companyService;
        private readonly IBillRepository _billRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        public BillService(ICompanyService companyService, 
            IBillRepository billRepository,
            ITenderBidRepository tenderBidRepository,
            ITenderRepository tenderRepository)
        {
            _companyService = companyService;
            _billRepository = billRepository;
            _tenderBidRepository = tenderBidRepository;
            _tenderRepository = tenderRepository;
        }

        public async Task<ResponseModel<List<BillDTO>>> GetAllBills(SearchBill model, ApplicationUser _currentUser)
        {
            var currentUserCompany = await _companyService.GetCompanyByCurrentUserId(_currentUser);
            if(!currentUserCompany.Success) 
            {
                return new ResponseModel<List<BillDTO>>
                {
                    Success = false,
                    Message = "Không tìm thấy công ty để lấy hóa đơn"
                };
            }
            var tenders = _tenderRepository
                   .AsNoTracking()
                   .Where(t => t.TenderStatus == TenderStatuses.Completed);

            var data = (from t in tenders
                        join tb in _tenderBidRepository.AsNoTracking() on t.Id equals tb.TenderId
                        join b in _billRepository.AsNoTracking() on tb.Id equals b.TenderBidId into bidBills
                        from bbills in bidBills.DefaultIfEmpty()
                        where tb.IsSelected == true 
                        
                        select new
                        {
                            TenderId = t.Id,
                            Name = t.Name,
                            Amount = bbills.vnp_Amount != null ? (float)bbills.vnp_Amount : default,
                            BillType = bbills.BillType != null ? bbills.BillType : BillTypeEnum.ErrorConvert,
                            BankCode = bbills.vnp_BankCode != null ? bbills.vnp_BankCode : "",
                            BankTransNo = bbills.vnp_BankTranNo != null ? bbills.vnp_BankTranNo : "",
                            CardType = bbills.vnp_CardType != null ? bbills.vnp_CardType : "",
                            OrderInfo = bbills.vnp_OrderInfo != null ? bbills.vnp_OrderInfo : "",
                            PayDate = bbills.vnp_PayDate != null ? bbills.vnp_PayDate : default,                     
                            CompanyShipperId = t.CompanyShipperId,
                            CompanyCarrierId = t.CompanyCarrierId != null ? (int)t.CompanyCarrierId : default,
                        }).ToList();

            var listBillDTO = new List<BillDTO>();
            foreach ( var item in data )
            {
                if (DateTime.TryParseExact(item.PayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result1))
                {
                    listBillDTO.Add(new BillDTO
                    {
                        TenderId = item.TenderId,
                        Name = item.Name,
                        Amount = item.Amount,
                        BillType = item.BillType,
                        BankCode = item.BankCode,
                        BankTransNo = item.BankTransNo,
                        CardType = item.CardType,
                        OrderInfo = item.OrderInfo,
                        PayDate = result1,
                        CompanyCarrierId = item.CompanyCarrierId,
                        CompanyShipperId = item.CompanyShipperId
                    });
                }
                else
                {
                    listBillDTO.Add(new BillDTO
                    {
                        TenderId = item.TenderId,
                        Name = item.Name,
                        Amount = item.Amount,
                        BillType = item.BillType,
                        BankCode = item.BankCode,
                        BankTransNo = item.BankTransNo,
                        CardType = item.CardType,
                        OrderInfo = item.OrderInfo,
                        PayDate = null,
                        CompanyCarrierId = item.CompanyCarrierId,
                        CompanyShipperId = item.CompanyShipperId
                    });
                }
            }
            if (currentUserCompany.Data.Role == CompanyRoleEnum.Shipper)
            {
                listBillDTO = listBillDTO
                .Where(t => t.CompanyShipperId == _currentUser.CompanyId
                && t.BillType == BillTypeEnum.ShipperToAdminDinoTrans
                && (model.SearchText.IsNullOrEmpty()
                    || t.Name.Contains(model.SearchText)
                    || $"#000{t.TenderId}".Contains(model.SearchText)
                    || (t.BankCode != null ? t.BankCode.Contains(model.SearchText) : true)
                    || (t.BankTransNo != null ? t.BankTransNo.Contains(model.SearchText) : true)
                    || (t.CardType != null ? t.CardType.Contains(model.SearchText) : true)
                    || (t.OrderInfo != null ? t.OrderInfo.Contains(model.SearchText) : true))
                && (model.FromDate != null || t.PayDate >= model.FromDate)
                && (model.ToDate != null || t.PayDate <= model.ToDate))
                .ToList();

                var listPaging = listBillDTO
                    .Skip((model.pageIndex-1)*model.pageSize)
                    .Take(model.pageSize).ToList();

                return new ResponseModel<List<BillDTO>>
                {
                    Data = listPaging,
                    Success = true,
                    Total = listBillDTO.Count(),
                    PageCount = listBillDTO.Count() / 10 + 1
                };
            }

            if (currentUserCompany.Data.Role == CompanyRoleEnum.Carrier)
            {
               /* data = data
                .Where(t => t.CompanyCarrierId == _currentUser.CompanyId
                && t.BillType == BillTypeEnum.ShipperToAdminDinoTrans);*/

                return new ResponseModel<List<BillDTO>>
                {
                    /*Data = data.ToList(),*/
                    Success = true
                };
            }

            return new ResponseModel<List<BillDTO>>
            {
                /*Data = data.ToList(),*/
                Success = true
            };
        }

        public async Task<ResponseModel<BillDTO>> GetBillDetail(int BillId)
        {
            throw new NotImplementedException();
        }
    }
}
