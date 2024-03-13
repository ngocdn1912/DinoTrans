using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderBidService : ITenderBidService
    {
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyRepository _companyRepository;
        public TenderBidService(ITenderBidRepository tenderBidRepository,
            ITenderRepository tenderRepository,
            IUnitOfWork unitOfWork,
            ICompanyRepository companyRepository) 
        {
            _tenderBidRepository = tenderBidRepository;
            _tenderRepository = tenderRepository;
            _unitOfWork = unitOfWork;
            _companyRepository = companyRepository;
        }

        public async Task<ServiceResponses.GeneralResponse> ChooseTenderBid(int TenderBidId, ApplicationUser currentUser)
        {
            var company = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == currentUser.CompanyId)
                .FirstOrDefaultAsync();

            if (company == null || company.Role != CompanyRoleEnum.Shipper)
                return new ServiceResponses.GeneralResponse(false, "Không tồn tại công ty hoặc công ty của bạn không có chức năng này");

            var tenderBid = await _tenderBidRepository
                .AsNoTracking()
                .Where(tb => tb.Id == TenderBidId)
                .FirstOrDefaultAsync();

            if (tenderBid == null)
                return new ServiceResponses.GeneralResponse(false, "Không tồn tại TenderBid");

            var tender = await _tenderRepository
                .AsNoTracking()
                .Where(tb => tb.Id == tenderBid.TenderId)
                .FirstOrDefaultAsync();

            if(tender == null)
                return new ServiceResponses.GeneralResponse(false, "Không tìm thấy Tender");

            var anyBids = _tenderBidRepository
                .AsNoTracking()
                .Any(tb => tb.TenderId == tender.Id
                && tb.IsSelected == true);

            if(anyBids)
                return new ServiceResponses.GeneralResponse(false, "Thầu này đã có công ty được chọn!");

            tenderBid.IsSelected = true;
            _tenderBidRepository.Update(tenderBid);
            _tenderBidRepository.SaveChange();
            tender.FinalPrice = tenderBid.TransportPrice;
            tender.TenderStatus = TenderStatuses.InExcecution;
            _tenderRepository.Update(tender);
            _tenderRepository.SaveChange();

            return new ServiceResponses.GeneralResponse(true, "Chọn thầu thành công");
        }

        public async Task<ResponseModel<TenderBid>> DeleteTenderBid(int TenderBidId)
        {
            var tenderBid = await _tenderBidRepository
                .Queryable()
                .Include(c => c.CompanyCarrier)
                .Where(t => t.Id == TenderBidId)
                .FirstOrDefaultAsync();

            if (tenderBid == null)
                return new ResponseModel<TenderBid>
                {
                    Success = false,
                    Message = "Không tìm thấy đặt giá để xóa"
                };

            var deletedBid = new TenderBid
            {
                Id = tenderBid.Id,
                TenderId = tenderBid.TenderId,
                CompanyCarrierId = tenderBid.CompanyCarrierId,
                TransportPrice = tenderBid.TransportPrice,
                ShipperFee = tenderBid.ShipperFee,
                CarrierFee = tenderBid.CarrierFee,
                IsSelected = tenderBid.IsSelected,
                CompanyCarrier = tenderBid.CompanyCarrier
            };

            _tenderBidRepository.Delete(tenderBid);
            _tenderBidRepository.SaveChange();

            return new ResponseModel<TenderBid>
            {
                Success = true,
                Data = deletedBid
            };
        }

        public async Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId)
        {
            var tenderBids = await _tenderBidRepository
                .AsNoTracking()
                .Include(t => t.CompanyCarrier)
                .Where(t => t.TenderId == TenderId)
                .ToListAsync();

            if(tenderBids == null)
            {
                return new ResponseModel<List<TenderBid>>
                {
                    Success = false,
                    Message = "Can't get TenderBids"
                };
            }    
            return new ResponseModel<List<TenderBid>>
            {
                Success = true,
                Data = tenderBids
            };
        }

        public async Task<ResponseModel<TenderBid>> SubmitTenderBid(TenderBidDTO dto, ApplicationUser currentUser)
        {
            var tender = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.TenderId)
                .FirstOrDefaultAsync();

            if (tender == null)
            {
                _unitOfWork.Rollback();
                return new ResponseModel<TenderBid>
                {
                    Success = false,
                    Message = "Không tìm thấy thầu"
                }; 
            }

            var tenderBidExist = _tenderBidRepository
                .AsNoTracking()
                .Any(t => t.TenderId == dto.TenderId
                && t.CompanyCarrierId == currentUser.CompanyId);

            if (tenderBidExist)
            {
                _unitOfWork.Rollback();
                return new ResponseModel<TenderBid>
                {
                    Message = "Công ty của bạn đã đặt giá cho thầu này rồi",
                    Success = false,
                };
            }

            var newTenderBid = new TenderBid
            {
                TenderId = dto.TenderId,
                CompanyCarrierId = (int)currentUser.CompanyId!,
                TransportPrice = dto.TransportPrice
            };
            _tenderBidRepository.Add(newTenderBid);
            _tenderBidRepository.SaveChange();
            newTenderBid.CompanyCarrier = _companyRepository.AsNoTracking().Where(c => c.Id == currentUser.CompanyId).FirstOrDefault();
            return new ResponseModel<TenderBid>
            {
                Success = true,
                Data = newTenderBid
            };
        }

        public async Task<ResponseModel<TenderBid>> UpdateTenderBid(UpdateTenderBidDTO dto)
        {
            var tenderBid = await _tenderBidRepository
                .Queryable()
                .Include(t => t.CompanyCarrier)
                .Where(tb => tb.Id == dto.TenderBidId)
                .FirstOrDefaultAsync();

            if (tenderBid == null) 
            {
                return new ResponseModel<TenderBid>
                {
                    Success = false,
                    Message = "Không tồn tại TenderBid"
                };
            }

            tenderBid.TransportPrice = dto.TransportPrice;
            _tenderBidRepository.Update(tenderBid);
            _tenderBidRepository.SaveChange();

            return new ResponseModel<TenderBid>
            {
                Success = true,
                Data = tenderBid
            };
        }
    }
}
