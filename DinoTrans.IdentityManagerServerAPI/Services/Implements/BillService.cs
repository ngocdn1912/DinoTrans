using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ResponseModel<List<Bill>>> GetAllBills(ApplicationUser _currentUser)
        {
            var currentUserCompany = await _companyService.GetCompanyByCurrentUserId(_currentUser);
            if(!currentUserCompany.Success) 
            {
                return new ResponseModel<List<Bill>>
                {
                    Success = false,
                    Message = "Không tìm thấy công ty để lấy hóa đơn"
                };
            }
            var tenders = _tenderRepository
                   .AsNoTracking()
                   .Where(t => t.TenderStatus == TenderStatuses.Completed);

            var data = from t in tenders
                        join tb in _tenderBidRepository.AsNoTracking() on t.Id equals tb.TenderId
                        join b in _billRepository.AsNoTracking() on tb.Id equals b.TenderBidId into bidBills
                        from bbills in bidBills.DefaultIfEmpty()
                        where tb.IsSelected == true
                        select new
                        {
                            TenderId = t.Id,
                            Name = t.Name,
                            TransportPrice = tb.TransportPrice,
                            BillType = bbills.BillType,
                            BankCode = bbills.vnp_BankCode,
                            BankTransNo = bbills.vnp_BankTranNo,
                            CardType = bbills.vnp_CardType,
                            OrderInfo = bbills.vnp_OrderInfo,
                            PayDate = bbills.vnp_PayDate,
                            CompanyShipperId = t.CompanyShipperId,
                            CompanyCarrierId = t.CompanyCarrierId,
                        };
            if (currentUserCompany.Data.Role == CompanyRoleEnum.Shipper)
            {
                data = data
                .Where(t => t.CompanyShipperId == _currentUser.CompanyId
                && t.BillType == BillTypeEnum.ShipperToAdminDinoTrans);

                return new ResponseModel<List<Bill>>
                {

                };
            }    
        }

        public async Task<ResponseModel<Bill>> GetBillDetail(int BillId)
        {
            throw new NotImplementedException();
        }
    }
}
