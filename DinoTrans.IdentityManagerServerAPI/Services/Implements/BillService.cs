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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                            BillId = bbills.Id != null ? bbills.Id:default,
                            TenderId = t.Id,
                            Name = t.Name,
                            Amount = bbills.vnp_Amount != null ? (float)bbills.vnp_Amount : default,
                            BillType = bbills.BillType != null ? bbills.BillType : BillTypeEnum.ErrorConvert,
                            BankCode = bbills.vnp_BankCode != null ? bbills.vnp_BankCode : "",
                            BankTransNo = bbills.vnp_BankTranNo != null ? bbills.vnp_BankTranNo : "",
                            CardType = bbills.vnp_CardType != null ? bbills.vnp_CardType : "",
                            OrderInfo = bbills.vnp_OrderInfo != null ? bbills.vnp_OrderInfo : "",
                            PayDate = bbills.vnp_PayDate != null ? bbills.vnp_PayDate : null,     
                            TransactionNo = bbills.vnp_TransactionNo != null ? bbills.vnp_TransactionNo : "",
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
                        BillId = item.BillId,
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
                        CompanyShipperId = item.CompanyShipperId,
                        TransactionNo = item.TransactionNo
                    });
                }
                else
                {
                    listBillDTO.Add(new BillDTO
                    {
                        BillId = item.BillId,
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
                        CompanyShipperId = item.CompanyShipperId,
                        TransactionNo = item.TransactionNo,
                    });
                }
            }
            
            if (currentUserCompany.Data.Role == CompanyRoleEnum.Admin)
            {
                listBillDTO = listBillDTO
                 .Where(t => string.IsNullOrEmpty(model.SearchText)
                     || t.Name.Contains(model.SearchText)
                     || $"#000{t.TenderId}".Contains(model.SearchText)
                     || (t.BankCode != null ? t.BankCode.Contains(model.SearchText) : true)
                     || (t.BankTransNo != null ? t.BankTransNo.Contains(model.SearchText) : true)
                     || (t.CardType != null ? t.CardType.Contains(model.SearchText) : true)
                     || (t.TransactionNo != null ? t.TransactionNo.Contains(model.SearchText) : true)
                     || (t.OrderInfo != null ? t.OrderInfo.Contains(model.SearchText) : true))
                 .ToList();

                if (model.FromDate != null)
                    listBillDTO = listBillDTO.Where(l => l.PayDate != null && l.PayDate >= model.FromDate).ToList();

                if (model.ToDate != null)
                    listBillDTO = listBillDTO.Where(l => l.PayDate != null && l.PayDate <= model.ToDate).ToList();

                switch(model.BillType)
                {
                    case BillTypeSearchModel.All:
                        break;
                    case BillTypeSearchModel.ShipperToAdmin:
                        listBillDTO = listBillDTO.Where(l => l.BillType == BillTypeEnum.ShipperToAdminDinoTrans).ToList();
                        break;
                    case BillTypeSearchModel.AdminToCarrier:
                        listBillDTO = listBillDTO.Where(l => l.BillType == BillTypeEnum.AdminDinoTransToCarrier).ToList();
                        break;
                }    

                switch(model.AmountType)
                {
                    case AmountTypeSearchModel.All : break;
                    case AmountTypeSearchModel.LessThan5M:
                        listBillDTO = listBillDTO.Where(l => l.Amount != null && l.Amount < 5000000).ToList();
                        break;
                    case AmountTypeSearchModel.From5MTo10M:
                        listBillDTO = listBillDTO.Where(l => l.Amount != null && l.Amount <= 10000000 && l.Amount >= 5000000).ToList();
                        break;
                    case AmountTypeSearchModel.MoreThan10M:
                        listBillDTO = listBillDTO.Where(l => l.Amount != null && l.Amount > 10000000).ToList();
                        break;
                }

                
            }else if (currentUserCompany.Data.Role == CompanyRoleEnum.Carrier)
            {
                listBillDTO = listBillDTO
                 .Where(t => t.CompanyCarrierId == _currentUser.CompanyId
                 && t.BillType == BillTypeEnum.AdminDinoTransToCarrier).ToList();
            }else if (currentUserCompany.Data.Role == CompanyRoleEnum.Shipper)
            {
                listBillDTO = listBillDTO
                .Where(t => t.CompanyShipperId == _currentUser.CompanyId
                && t.BillType == BillTypeEnum.ShipperToAdminDinoTrans).ToList();
            }

            var listPaging = listBillDTO
                    .Skip((model.pageIndex - 1) * model.pageSize)
                    .Take(model.pageSize).ToList();

                return new ResponseModel<List<BillDTO>>
                {
                    Data = listPaging,
                    Success = true,
                    Total = listBillDTO.Count(),
                    PageCount = listBillDTO.Count() / 10 + 1
                };
        }

        public async Task<ResponseModel<BillDTO>> GetBillDetail(int BillId)
        {
            var bill = (from b in _billRepository.AsNoTracking().Where(b => b.Id == BillId)
                       join tb in _tenderBidRepository.AsNoTracking() on b.TenderBidId equals tb.Id
                       join tender in _tenderRepository.AsNoTracking() on tb.TenderId equals tender.Id
                       select new
                       {
                           TenderId = tender.Id,
                           Name = tender.Name,
                           Amount = b.vnp_Amount != null ? b.vnp_Amount! : default,
                           BillType = b.BillType != null ? b.BillType : BillTypeEnum.ErrorConvert,
                           BankCode = b.vnp_BankCode != null ? b.vnp_BankCode : default,
                           BankTransNo = b.vnp_BankTranNo != null ? b.vnp_BankTranNo : default,
                           CardType = b.vnp_CardType != null ? b.vnp_CardType : default,
                           OrderInfo = b.vnp_OrderInfo != null ? b.vnp_OrderInfo : default,
                           PayDate = b.vnp_PayDate != null ? b.vnp_PayDate : default,
                           TransactionNo = b.vnp_TransactionNo != null ? b.vnp_TransactionNo : default,
                           CompanyShipperId = tender.CompanyShipperId,
                           CompanyCarrierId = tender.CompanyCarrierId != null ? (int)tender.CompanyCarrierId : default
                       }).FirstOrDefault();

            if (bill == null)
                return new ResponseModel<BillDTO>
                {
                    Success = false,
                    Message = $"Không tìm thấy bill với Id = {BillId}"
                };

            var billDTO = new BillDTO();
            if (DateTime.TryParseExact(bill.PayDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result1))
            {
                
                billDTO.PayDate = result1;
            }else
            {
                billDTO.PayDate = null;
            }
            billDTO.TenderId = bill.TenderId;
            billDTO.Name = bill.Name;
            billDTO.Amount = bill.Amount != null ? (float)bill.Amount : default;
            billDTO.BillType = bill.BillType;
            billDTO.BankCode = bill.BankCode;
            billDTO.BankTransNo = bill.BankTransNo;
            billDTO.CardType = bill.CardType;
            billDTO.OrderInfo = bill.OrderInfo;
            billDTO.TransactionNo = bill.TransactionNo;
            billDTO.CompanyCarrierId = bill.CompanyCarrierId;
            billDTO.CompanyShipperId = bill.CompanyShipperId;
            return new ResponseModel<BillDTO>
            {
                Data = billDTO,
                Success = true
            };
        }
    }
}
