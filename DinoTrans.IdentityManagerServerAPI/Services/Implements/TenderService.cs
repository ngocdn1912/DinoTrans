using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs.ContructionMachine;
using static DinoTrans.Shared.DTOs.ServiceResponses;
using DinoTrans.Shared.Repositories.Implements;
using Microsoft.AspNetCore.Identity;
using DinoTrans.Shared.DTOs.UserResponse;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DinoTrans.Shared.DTOs.SearchDTO;
using Microsoft.IdentityModel.Tokens;
using NHibernate.Engine;
using DinoTrans.Shared.DTOs.TendersActive;
using System.Drawing.Drawing2D;
using static Azure.Core.HttpHeader;
using System.Xml.Linq;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class TenderService : ITenderService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IConstructionMachineRepository _contructionMachineRepository;
        private readonly ITenderConstructionMachineRepository _tenderConstructionMachineRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IConstructionMachineService _machineService;
        private readonly ITenderBidService _tenderBidService;
        private readonly IVnPayService _vnPayService;
        public TenderService(ITenderRepository tenderRepository,
            ICompanyRepository companyRepository, 
            IConstructionMachineRepository contructionMachineRepository,
            ITenderConstructionMachineRepository tenderConstructionMachineRepository,
            IUnitOfWork unitOfWork,
            ITenderBidRepository tenderBidRepository,
            IConstructionMachineService machineService,
            ITenderBidService tenderBidService,
            IVnPayService vnPayService)
        {
            _tenderRepository = tenderRepository;
            _companyRepository = companyRepository;
            _contructionMachineRepository = contructionMachineRepository;
            _tenderConstructionMachineRepository = tenderConstructionMachineRepository;
            _unitOfWork = unitOfWork;
            _tenderBidRepository = tenderBidRepository;
            _machineService = machineService;
            _tenderBidService = tenderBidService;
            _vnPayService = vnPayService;
        }

        public async Task<GeneralResponse> ConfirmCompleteTender(int TenderId, ApplicationUser? currentUser)
        {
            var tender = await _tenderRepository
                .Queryable()
                .Include(t => t.CompanyCarrier)
                .Include(t => t.CompanyShipper)
                .Where(t => t.Id == TenderId)
                .FirstOrDefaultAsync();

            var tenderBid = (await _tenderBidService.GetTenderBidsByTenderId(TenderId)).Data.FirstOrDefault(tb => tb.IsSelected);
            if(tenderBid  == null)
            {
                return new GeneralResponse(false, "Không tìm thấy đấu giá đã được chọn");
            }    

            if(tender == null)
            {
                return new GeneralResponse(false, $"Không thể tìm thấy đầu thầu với Id = {TenderId}");
            }

            if(tender.TenderStatus != TenderStatuses.InExcecution) 
            {
                return new GeneralResponse(false, $"Thầu không ở trạng thái đang hoạt động");
            }

            if(currentUser!.CompanyId != tender.CompanyShipperId && currentUser!.CompanyId != tender.CompanyCarrierId && currentUser.Company.Role != CompanyRoleEnum.Admin)
            {
                return new GeneralResponse(false, "Bạn không sở hữu thầu này");
            }    

            if(tender.DeiliverDate > DateTime.Now)
            {
                return new GeneralResponse(false, "Chưa tới hạn giao máy, không thể kết thúc thầu");
            }    

            if(currentUser!.CompanyId == tender.CompanyCarrierId)
            {
                if (tender.IsCarrierComfirm == true) return new GeneralResponse(false, $"Bạn đã đóng thành công vận chuyển này rồi, hãy đợi công ty thuê vận chuyển {tender.CompanyShipper.CompanyName} và Admin kết thúc thầu");
                tender.IsCarrierComfirm = true;
            }

            if (currentUser!.CompanyId == tender.CompanyShipperId)
            {
                if(tender.IsCarrierComfirm == false) return new GeneralResponse(false, $"Bên công ty giao máy {tender.CompanyCarrier.CompanyName} chưa xác nhận, chưa thể kết thúc thầu");
                if(tender.IsShipperComfirm == true) return new GeneralResponse(false, "Bạn đã đóng thành công vận chuyển này rồi, hãy đợi Admin hoàn tất thầu");
                tender.IsShipperComfirm = true;
            }

            if(currentUser!.Company.Role == CompanyRoleEnum.Admin)
            {
                if (tender.IsCarrierComfirm == false) return new GeneralResponse(false, $"Công ty vận chuyển {tender.CompanyCarrier.CompanyName} chưa xác nhận, chưa thể kết thúc thầu");
                if (tender.IsShipperComfirm == false) return new GeneralResponse(false, $"Công ty thuê vận chuyển {tender.CompanyShipper.CompanyName} chưa xác nhận, chưa thể kết thúc thầu");

                var result = await _vnPayService.TransacVNPay_FromAdmin(tenderBid.Id);
                return new GeneralResponse(result.Flag, result.Message);
            }

            _tenderRepository.Update(tender);
            _tenderRepository.SaveChange();
            return new GeneralResponse(true, currentUser!.CompanyId == tender.CompanyCarrierId ? "Bạn đã đóng thầu thành công, hãy chờ công ty tạo thầu kết thúc" : "Bạn đã đóng thầu thành công");
        }

        public async Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            var companyShipper = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == dto.CompanyShipperId)
                .FirstOrDefaultAsync();
            if(companyShipper == null ) { }
            var newTender = new Tender()
            {
                TenderStatus = TenderStatuses.Draft,
                CompanyShipperId = dto.CompanyShipperId,
                Name = dto.TenderName,
                StartDate = dto.TenderStartDate,
                EndDate = dto.TenderEndDate
            };
            _tenderRepository.Add(newTender);
            _tenderRepository.SaveChange();
            newTender.CompanyShipper = companyShipper;
            return new ResponseModel<Tender>
            {
                Success = true,
                Data = newTender
            };
        }

        public async Task<ResponseModel<Tender>> CreateTenderStep2(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            _unitOfWork.BeginTransaction();
            var tender = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.TenderId)
                .FirstOrDefaultAsync();

            if( tender == null ) 
            {
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = "Can't find tender to update",
                    ResponseCode = "404"
                };
            }

            var listConstructionMachine = _contructionMachineRepository
                .AsNoTracking()
                .Where(c => c.CompanyShipperId == tender.CompanyShipperId);

            foreach(var machineId in dto.ConstructionMachineIds)
            {
                if(!listConstructionMachine.Select(s => s.Id).Contains(machineId))
                {
                    _unitOfWork.Rollback();
                    return new ResponseModel<Tender>
                    {
                        Success = false,
                        Message = $"Can't find machine with Id = {machineId}",
                        ResponseCode = "404"
                    };
                }    
            }
            var tenderConstructionMachineList = new List<TenderContructionMachine>();
            var existedTenderConstructionMachines = await _tenderConstructionMachineRepository
                .AsNoTracking()
                .Where(c => c.TenderId == dto.TenderId)
                .ToListAsync();
            if(existedTenderConstructionMachines.Any())
            {
                _tenderConstructionMachineRepository.DeleteRange(existedTenderConstructionMachines);
            }    
            foreach(var item in dto.ConstructionMachineIds) 
            {
                var newTenderConstructionMachine = new TenderContructionMachine
                {
                    TenderId = dto.TenderId,
                    ContructionMachineId = item
                };
                tenderConstructionMachineList.Add(newTenderConstructionMachine);
            }
            tender.PickUpDate = dto.PickUpDateAndTime;
            tender.DeiliverDate = dto.DeliveryDateAndTime;
            tender.PickUpAddress = dto.PickUpAddress;
            tender.DeliveryAddress = dto.DeliveryAddress;
            tender.PickUpContact = dto.ContactAtPickUpAddress;
            tender.DeliveryContact = dto.ContactAtDeliveryAddress;
            tender.Notes = dto.Notes;
            var listDocs = new List<Dictionary<string, string>>();
            if (dto.Documentations != null && dto.Documentations.Count > 0)
            {
                for (int i = 0; i < dto.Documentations.Count; i++)
                {
                    var newDoc = new Dictionary<string, string>()
                    {
                        {$"Document_{i}",dto.Documentations[i] }
                    };
                    listDocs.Add(newDoc);
                }
                tender.Documentations = JsonConvert.SerializeObject(listDocs);
            }
            try
            {
                _tenderRepository.Update(tender);
                _tenderConstructionMachineRepository.AddRange(tenderConstructionMachineList);
                _unitOfWork.SaveChanges();
                _unitOfWork.Commit();
                return new ResponseModel<Tender>
                {
                    Success = true,
                    Data = tender
                };
            }
            catch(Exception ex) 
            {
                _unitOfWork.Rollback();
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseModel<TenderDetailsDTO>> GetTenderById(int Id)
        {
            var result = (from t in _tenderRepository.AsNoTracking().Where(t => t.Id == Id)
                          join tc in _tenderConstructionMachineRepository.AsNoTracking() on t.Id equals tc.TenderId into tenderConstructionMachines
                         let constructionMachines = (from c in _contructionMachineRepository.AsNoTracking()
                                                     join tsResult in tenderConstructionMachines on c.Id equals tsResult.ContructionMachineId
                                                     where tsResult.TenderId == Id
                                                     select new ConstructionMachinesForTendersDTO
                                                     {
                                                         Id = c.Id,
                                                         Name = c.Name,
                                                         Brand = c.Brand,
                                                         SerialNumber = c.SerialNumber,
                                                         CompanyShipperId = c.CompanyShipperId,
                                                         Image = c.Image,
                                                         Length = c.Length,
                                                         Width = c.Width,
                                                         Height = c.Height,
                                                         Weight = c.Weight
                                                     }).ToList()
                         select new TenderDetailsDTO
                         {
                             TenderId = t.Id,
                             TenderName = t.Name,
                             TenderStatus = t.TenderStatus,
                             CompanyShipperId = t.CompanyShipperId,
                             CompanyCarrierId = t.CompanyCarrierId,
                             StartDate = t.StartDate,
                             EndDate = t.EndDate,
                             PickUpDate = t.PickUpDate,
                             DeiliverDate = t.DeiliverDate,
                             PickUpAddress = t.PickUpAddress,
                             PickUpContact = t.PickUpContact,
                             DeliveryAddress = t.DeliveryAddress,
                             DeliveryContact = t.DeliveryContact,
                             Notes = t.Notes,
                             Documentations = t.Documentations,
                             ConstructionMachines = constructionMachines,
                             IsCarrierConfirm = t.IsCarrierComfirm,
                             IsShipperConfirm = t.IsShipperComfirm
                             
                         }).FirstOrDefault();

            return new ResponseModel<TenderDetailsDTO>
            {
                Data = result!,
                Success = true
            };
        }

        public async Task<ResponseModel<List<Tender>>> GetTendersActiveForAuto()
        {
            var allTenderActive = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.TenderStatus == TenderStatuses.Active)
                .ToListAsync();

            var result = allTenderActive.ToList();
            if (allTenderActive != null)
            {
                foreach (var item in allTenderActive)
                {
                    var AnyBids = _tenderBidRepository
                        .AsNoTracking()
                        .Any(t => t.TenderId == item.Id);
                    //remove để không bị auto withdraw
                    if ((item.EndDate - DateTime.Now).TotalSeconds > 0 || 
                        (AnyBids && (item.EndDate - DateTime.Now).TotalSeconds < 0 && (DateTime.Now - item.EndDate).TotalSeconds < 24*60*60) )
                        result.Remove(item);
                }
            }
            else
            {
                return new ResponseModel<List<Tender>>
                {
                    Success = false
                };
            }    

            return new ResponseModel<List<Tender>>
            {
                Success = true,
                Data = result!
            };
        }

        /*public async Task<ResponseModel<List<TenderActiveDTO_Test>>> SearchActiveBy_Test(SearchTenderActiveDTO dto, ApplicationUser currentUser)
        {
            var tenders = _tenderRepository.AsNoTracking();
            var machines = _contructionMachineRepository.AsNoTracking();
            var tenderBids = _tenderBidRepository.AsNoTracking();
            var tenderConstructionMachines = _tenderConstructionMachineRepository.AsNoTracking();
            var companies = _companyRepository.AsNoTracking();
            var queryResult = (from t in tenders
                         join comShip in companies on t.CompanyShipperId equals comShip.Id
                         join tc in tenderConstructionMachines on t.Id equals tc.TenderId into ttc

                         from tenderConstructionMachine in ttc.DefaultIfEmpty()
                         join m in machines on tenderConstructionMachine.ContructionMachineId equals m.Id into tMachines

                         from machine in tMachines.DefaultIfEmpty()
                         join tb in tenderBids on t.Id equals tb.TenderId into ttb

                         from bids in ttb.DefaultIfEmpty()
                         group new {Tender = t, ConstructionMachines = tenderConstructionMachine,  machines = machine, bids = bids , companyShipper = comShip } by t into tenderGroup
                         select new
                         {
                             Tender = tenderGroup.Key,
                             ConstructionMachines = tenderGroup.Select(t => t.machines).ToList(),
                             Bids = tenderGroup.Select(tb => tb.bids).ToList(),
                             CompanyShipper = tenderGroup.Select(c => c.companyShipper).ToList(),
                         }).ToList();

            var result = new List<TenderActiveDTO_Test>();
            foreach(var item in queryResult) 
            {
                var timeRemains = (item.Tender.EndDate - DateTime.Now).TotalSeconds;
                result.Add(new TenderActiveDTO_Test
                {
                    TenderId = item.Tender.Id,
                    TenderName = item.Tender.Name,
                    ConstructionMachines = item.ConstructionMachines,
                    TenderBids = item.Bids,
                    From = item.Tender.PickUpAddress,
                    To = item.Tender.DeliveryAddress,
                    PickUpDate = (DateTime)item.Tender.PickUpDate,
                    DeliveryDate = (DateTime)item.Tender.DeiliverDate,
                    Status = item.Tender.TenderStatus.ToString(),
                    TimeRemaining = timeRemains > 0?timeRemains:0,
                    CompanyShipperId = item.Tender.CompanyShipperId,
                    CompanyShipperName = item.CompanyShipper.FirstOrDefault().CompanyName,
                });
            }

            return new ResponseModel<List<TenderActiveDTO_Test>>
            {
                Data = result,
                Success = true
            };
        }*/

        public async Task<ResponseModel<List<TenderActiveDTO>>> SearchActiveBy(SearchTenderActiveDTO dto, ApplicationUser currentUser)
        {
            var listActive = _tenderRepository
            .AsNoTracking()
            .Include(t => t.CompanyShipper)
            .Where(t => t.TenderStatus == TenderStatuses.Active)
            .ToList();

            listActive = listActive.Where(t => (t.EndDate - DateTime.Now).TotalNanoseconds > 0).ToList();

            var currentUserCompany = _companyRepository
                                    .AsNoTracking()
                                    .Where(c => c.Id == currentUser.CompanyId)
                                    .FirstOrDefault();

            if (currentUserCompany!.Role == CompanyRoleEnum.Shipper)
            {
                listActive = listActive.Where(t =>
                    t.CompanyShipperId! == currentUser.CompanyId).ToList();
            }

            var listTenderActiveDTO = new List<TenderActiveDTO>();
            foreach (var item in listActive)
            {
                var timeRemains = (item.EndDate - DateTime.Now).TotalSeconds;
                var newTenderActiveDTO = new TenderActiveDTO
                {
                    TenderId = item.Id,
                    TenderName = item.Name,
                    From = item.PickUpAddress,
                    To = item.DeliveryAddress,
                    PickUpDate = (DateTime)item.PickUpDate,
                    DeliveryDate = (DateTime)item.DeiliverDate,
                    Status = item.TenderStatus.ToString(),
                    TimeRemaining = timeRemains > 0 ? timeRemains : 0,
                    CompanyShipperId = item.CompanyShipperId,
                    CompanyShipperName = item.CompanyShipper!.CompanyName
                };

                var constructionMachines = await _machineService.GetMachinesForTenderOverviewByIds(item.Id);
                newTenderActiveDTO.ConstructionMachines = constructionMachines.Data;
                var Bids = await _tenderBidService.GetTenderBidsByTenderId(item.Id);
                newTenderActiveDTO.Bids = Bids.Data;
                var hasChoose = false;
                foreach(var bid in newTenderActiveDTO.Bids)
                {
                    if(bid.CompanyCarrierId == currentUser.CompanyId)
                    {
                        hasChoose = true;
                        break;
                    }    
                }   
                  
                if(!hasChoose)
                    listTenderActiveDTO.Add(newTenderActiveDTO);
            }

            var listActiveNotPaging = listTenderActiveDTO.Where(c => dto.SearchText.IsNullOrEmpty()
                        || c.TenderName.Contains(dto.SearchText!)
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!))); 

            switch(dto.searchLoads)
            {
                case SearchActiveByMachines.All:
                    break;
                case SearchActiveByMachines.LessThan8Tons:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight < 8000));
                    break;
                case SearchActiveByMachines.From8To22Tons:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 8000 &&  c.Weight < 22000));
                    break;
                case SearchActiveByMachines.From22Tons:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 22000));
                    break;
            }    

            switch(dto.searchOffers)
            {
                case SearchActiveByOffers.All:
                    break;
                case SearchActiveByOffers.NoOffers:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.Bids.Count == 0);
                    break;
                case SearchActiveByOffers.MoreThan5Offers:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.Bids.Count > 5);
                    break;
                case SearchActiveByOffers.Max5Offers:
                    listActiveNotPaging = listActiveNotPaging.Where(l => l.Bids.Count <= 5);
                    break;
            }    
            var listActivePaging = listActiveNotPaging
                        .Skip((dto.pageIndex - 1) * dto.pageSize)
                        .Take(dto.pageSize);

            return new ResponseModel<List<TenderActiveDTO>>
            {
                Data = listActivePaging.ToList(),
                Success = true,
                Total = listActiveNotPaging.Count(),
                PageCount = listActiveNotPaging.Count() / 10 + 1
            };    
        }

        public async Task<ResponseModel<List<TenderInExecutionDTO>>> SearchCompleted(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            var companyRole = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == currentUser!.CompanyId)
                .Select(c => c.Role)
                .FirstOrDefaultAsync();

            var tenderInExecution = (from t in _tenderRepository.AsNoTracking()
                                    .Where(t => t.TenderStatus == TenderStatuses.Completed
                                    && (t.CompanyCarrierId == currentUser!.CompanyId || t.CompanyShipperId == currentUser!.CompanyId))
                                     join tb in _tenderBidRepository.AsNoTracking() on t.Id equals tb.TenderId
                                     join tc in _tenderConstructionMachineRepository.AsNoTracking() on t.Id equals tc.TenderId
                                     join c in _contructionMachineRepository.AsNoTracking() on tc.ContructionMachineId equals c.Id
                                     select new
                                     {
                                         TenderId = t.Id,
                                         TenderStatus = t.TenderStatus,
                                         CompanyShipperId = t.CompanyShipperId,
                                         FinalCarrierId = t.CompanyCarrierId,
                                         StartDate = t.StartDate,
                                         EndDate = t.EndDate,
                                         FinalPrice = t.FinalPrice,
                                         IsShipperComfirm = t.IsShipperComfirm,
                                         IsCarrierComfirm = t.IsCarrierComfirm,
                                         Notes = t.Notes,
                                         DeiliverDate = t.DeiliverDate,
                                         DeliveryAddress = t.DeliveryAddress,
                                         DeliveryContact = t.DeliveryContact,
                                         Documentations = t.Documentations,
                                         Name = t.Name,
                                         PickUpAddress = t.PickUpAddress,
                                         PickUpContact = t.PickUpContact,
                                         PickUpDate = t.PickUpDate,
                                         TenderBidId = tb.Id,
                                         SubmitedCompanyCarrierId = tb.CompanyCarrierId,
                                         TransportPrice = tb.TransportPrice,
                                         ShipperFee = tb.ShipperFee,
                                         CarrierFee = tb.CarrierFee,
                                         IsSelected = tb.IsSelected,
                                         MachineId = c.Id,
                                         MachineName = c.Name,
                                         Brand = c.Brand,
                                         SerialNumber = c.SerialNumber,
                                         OwnerCompanyShipperId = c.CompanyShipperId,
                                         Image = c.Image,
                                         c.Length,
                                         c.Width,
                                         c.Height,
                                         c.Weight
                                     }).ToList();

            if (companyRole == CompanyRoleEnum.Shipper)
                tenderInExecution = tenderInExecution.Where(t => t.CompanyShipperId == currentUser!.CompanyId).ToList();
            else if (companyRole == CompanyRoleEnum.Carrier)
                tenderInExecution = tenderInExecution.Where(t => t.FinalCarrierId == currentUser!.CompanyId).ToList();

            var bids = tenderInExecution
                .Select(ta => new TenderBid
                {
                    Id = ta.TenderBidId,
                    TenderId = ta.TenderId,
                    CompanyCarrierId = ta.SubmitedCompanyCarrierId,
                    TransportPrice = ta.TransportPrice,
                    ShipperFee = ta.ShipperFee,
                    CarrierFee = ta.CarrierFee,
                    IsSelected = ta.IsSelected,
                })
                .GroupBy(ta => new { ta.TenderId, ta.Id, ta.CompanyCarrierId, ta.TransportPrice, ta.ShipperFee, ta.CarrierFee, ta.IsSelected })
                .Select(ta => new
                {
                    Key = ta.Key,
                    Bids = ta.FirstOrDefault()
                }).ToList();

            var machines = tenderInExecution
                .Select(ta => new
                {
                    TenderId = ta.TenderId,
                    ConstructionMachine = new ContructionMachine
                    {
                        Id = ta.MachineId,
                        Name = ta.MachineName,
                        Brand = ta.Brand,
                        SerialNumber = ta.SerialNumber,
                        CompanyShipperId = ta.OwnerCompanyShipperId,
                        Length = ta.Length,
                        Width = ta.Width,
                        Height = ta.Height,
                        Weight = ta.Weight,
                        Image = ta.Image
                    }
                })
                .GroupBy(ta => new {
                    ta.TenderId,
                    ta.ConstructionMachine.Id,
                    ta.ConstructionMachine.Name,
                    ta.ConstructionMachine.Brand,
                    ta.ConstructionMachine.SerialNumber,
                    ta.ConstructionMachine.CompanyShipperId,
                    ta.ConstructionMachine.Length,
                    ta.ConstructionMachine.Width,
                    ta.ConstructionMachine.Height,
                    ta.ConstructionMachine.Weight,
                    ta.ConstructionMachine.Image
                })
                .Select(ta => new
                {
                    Key = ta.Key,
                    Machines = ta.FirstOrDefault(),
                }).ToList();

            var inexeTenders = _tenderRepository.AsNoTracking().Include(t => t.CompanyCarrier).Include(t => t.CompanyShipper)
                                    .Where(t => t.TenderStatus == TenderStatuses.Completed
                                    && (t.CompanyCarrierId == currentUser!.CompanyId || t.CompanyShipperId == currentUser!.CompanyId)).ToList();

            if (companyRole == CompanyRoleEnum.Shipper)
                inexeTenders = inexeTenders.Where(t => t.CompanyShipperId == currentUser!.CompanyId).ToList();
            else if (companyRole == CompanyRoleEnum.Carrier)
                inexeTenders = inexeTenders.Where(t => t.CompanyCarrierId == currentUser!.CompanyId).ToList();

            var data = new List<TenderInExecutionDTO>();
            foreach (var item in inexeTenders)
            {
                var machinesForTender = machines.Where(m => m.Key.TenderId == item.Id).Select(m => m.Machines).ToList();
                var fmachines = machinesForTender!.Select(m => m.ConstructionMachine).ToList();
                var bidsForTender = bids.Where(b => b.Key.TenderId == item.Id).Select(b => b.Bids).ToList();
                var newTenderInExecutionDTO = new TenderInExecutionDTO
                {
                    TenderId = item.Id,
                    TenderName = item.Name,
                    ConstructionMachines = fmachines!,
                    From = item.PickUpAddress!,
                    To = item.DeliveryAddress!,
                    PickUpDate = (DateTime)item.PickUpDate!,
                    DeliveryDate = (DateTime)item.DeiliverDate!,
                    Status = item.TenderStatus.ToString(),
                    Bids = bidsForTender,
                    CompanyShipperId = item.CompanyShipperId,
                    CompanyCarrierId = (int)item.CompanyCarrierId,
                    CompanyShipperName = item.CompanyShipper!.CompanyName,
                    CompanyCarrierName = item.CompanyCarrier!.CompanyName,
                    Price = bidsForTender!.Where(b => b.IsSelected).Select(b => b.TransportPrice).FirstOrDefault()
                };
                data.Add(newTenderInExecutionDTO);
            }

            var listNotPaging = data.Where(c => dto.SearchText.IsNullOrEmpty()
                        || c.TenderName.Contains(dto.SearchText!)
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!)));

            switch (dto.searchLoads)
            {
                case SearchActiveByMachines.All:
                    break;
                case SearchActiveByMachines.LessThan8Tons:
                    listNotPaging = listNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight < 8000));
                    break;
                case SearchActiveByMachines.From8To22Tons:
                    listNotPaging = listNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 8000 && c.Weight < 22000));
                    break;
                case SearchActiveByMachines.From22Tons:
                    listNotPaging = listNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 22000));
                    break;
            }

            switch (dto.searchOffers)
            {
                case SearchActiveByOffers.All:
                    break;
                case SearchActiveByOffers.NoOffers:
                    listNotPaging = listNotPaging.Where(l => l.Bids.Count == 0);
                    break;
                case SearchActiveByOffers.MoreThan5Offers:
                    listNotPaging = listNotPaging.Where(l => l.Bids.Count > 5);
                    break;
                case SearchActiveByOffers.Max5Offers:
                    listNotPaging = listNotPaging.Where(l => l.Bids.Count <= 5);
                    break;
            }
            var listActivePaging = listNotPaging
                        .Skip((dto.pageIndex - 1) * dto.pageSize)
                        .Take(dto.pageSize);
            return new ResponseModel<List<TenderInExecutionDTO>>
            {
                Data = listActivePaging.ToList(),
                Success = true,
                Total = listNotPaging.Count(),
                PageCount = listNotPaging.Count() / 10 + 1
            };
        }

        public async Task<ResponseModel<List<TenderInExecutionDTO>>> SearchInExecution(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            var companyRole = await _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == currentUser!.CompanyId)
                .Select(c => c.Role)
                .FirstOrDefaultAsync();

            var tenderInExecution = (from t in _tenderRepository.AsNoTracking()
                                    .Where(t => t.TenderStatus == TenderStatuses.InExcecution
                                    && (t.CompanyCarrierId == currentUser!.CompanyId 
                                    || t.CompanyShipperId == currentUser!.CompanyId
                                    || companyRole.ToString() == CompanyRoleEnum.Admin.ToString()))
                                    join tb in _tenderBidRepository.AsNoTracking() on t.Id equals tb.TenderId
                                    join tc in _tenderConstructionMachineRepository.AsNoTracking() on t.Id equals tc.TenderId
                                    join c in _contructionMachineRepository.AsNoTracking() on tc.ContructionMachineId equals c.Id
                                    select new
                                    {
                                        TenderId = t.Id,
                                        TenderStatus = t.TenderStatus,
                                        CompanyShipperId = t.CompanyShipperId,
                                        FinalCarrierId = t.CompanyCarrierId,
                                        StartDate = t.StartDate,
                                        EndDate = t.EndDate,
                                        FinalPrice = t.FinalPrice,
                                        IsShipperComfirm = t.IsShipperComfirm,
                                        IsCarrierComfirm = t.IsCarrierComfirm,
                                        Notes = t.Notes,
                                        DeiliverDate = t.DeiliverDate,
                                        DeliveryAddress = t.DeliveryAddress,
                                        DeliveryContact = t.DeliveryContact,
                                        Documentations = t.Documentations,
                                        Name = t.Name,
                                        PickUpAddress = t.PickUpAddress,
                                        PickUpContact = t.PickUpContact,
                                        PickUpDate = t.PickUpDate,
                                        TenderBidId = tb.Id,
                                        SubmitedCompanyCarrierId = tb.CompanyCarrierId,
                                        TransportPrice = tb.TransportPrice,
                                        ShipperFee = tb.ShipperFee,
                                        CarrierFee = tb.CarrierFee,
                                        IsSelected = tb.IsSelected,
                                        MachineId = c.Id,
                                        MachineName = c.Name,
                                        Brand = c.Brand,
                                        SerialNumber = c.SerialNumber,
                                        OwnerCompanyShipperId = c.CompanyShipperId,
                                        Image = c.Image,
                                        c.Length,
                                        c.Width,
                                        c.Height,
                                        c.Weight
                                    }).ToList();

            if (companyRole == CompanyRoleEnum.Shipper)
                tenderInExecution = tenderInExecution.Where(t => t.CompanyShipperId == currentUser!.CompanyId).ToList();
            else if (companyRole == CompanyRoleEnum.Carrier)
                tenderInExecution = tenderInExecution.Where(t => t.FinalCarrierId == currentUser!.CompanyId).ToList();

            var bids = tenderInExecution
                .Select(ta => new TenderBid
                {
                    Id = ta.TenderBidId,
                    TenderId = ta.TenderId,
                    CompanyCarrierId = ta.SubmitedCompanyCarrierId,
                    TransportPrice = ta.TransportPrice,
                    ShipperFee = ta.ShipperFee,
                    CarrierFee = ta.CarrierFee,
                    IsSelected = ta.IsSelected,
                })
                .GroupBy(ta => new { ta.TenderId, ta.Id, ta.CompanyCarrierId, ta.TransportPrice, ta.ShipperFee, ta.CarrierFee, ta.IsSelected })
                .Select(ta => new
                {
                    Key = ta.Key,
                    Bids = ta.FirstOrDefault()
                }).ToList();

            var machines = tenderInExecution
                .Select(ta => new
                {
                    TenderId = ta.TenderId,
                    ConstructionMachine = new ContructionMachine
                    {
                        Id = ta.MachineId,
                        Name = ta.MachineName,
                        Brand = ta.Brand,
                        SerialNumber = ta.SerialNumber,
                        CompanyShipperId = ta.OwnerCompanyShipperId,
                        Length = ta.Length,
                        Width = ta.Width,
                        Height = ta.Height,
                        Weight = ta.Weight,
                        Image = ta.Image
                    }
                })
                .GroupBy(ta => new { ta.TenderId, ta.ConstructionMachine.Id, ta.ConstructionMachine.Name, ta.ConstructionMachine.Brand, 
                    ta.ConstructionMachine.SerialNumber, ta.ConstructionMachine.CompanyShipperId, ta.ConstructionMachine.Length, 
                    ta.ConstructionMachine.Width, ta.ConstructionMachine.Height, ta.ConstructionMachine.Weight, ta.ConstructionMachine.Image })
                .Select(ta => new
                {
                    Key = ta.Key,
                    Machines = ta.FirstOrDefault(),
                }).ToList();

            var inexeTenders = _tenderRepository.AsNoTracking().Include(t => t.CompanyCarrier).Include(t => t.CompanyShipper)
                                    .Where(t => t.TenderStatus == TenderStatuses.InExcecution
                                    && (t.CompanyCarrierId == currentUser!.CompanyId 
                                    || t.CompanyShipperId == currentUser!.CompanyId
                                    || companyRole.ToString() == CompanyRoleEnum.Admin.ToString())).ToList();

            if (companyRole == CompanyRoleEnum.Shipper)
                inexeTenders = inexeTenders.Where(t => t.CompanyShipperId == currentUser!.CompanyId).ToList();
            else if (companyRole == CompanyRoleEnum.Carrier)
                inexeTenders = inexeTenders.Where(t => t.CompanyCarrierId == currentUser!.CompanyId).ToList();

            var data = new List<TenderInExecutionDTO>();
            foreach(var item in inexeTenders)
            {
                var machinesForTender = machines.Where(m => m.Key.TenderId == item.Id).Select(m => m.Machines).ToList();
                var fmachines = machinesForTender!.Select(m => m.ConstructionMachine).ToList();
                var bidsForTender = bids.Where(b => b.Key.TenderId == item.Id).Select(b => b.Bids).ToList();
                var newTenderInExecutionDTO = new TenderInExecutionDTO
                {
                    TenderId = item.Id,
                    TenderName = item.Name,
                    ConstructionMachines = fmachines!,
                    From = item.PickUpAddress!,
                    To = item.DeliveryAddress!,
                    PickUpDate = (DateTime)item.PickUpDate!,
                    DeliveryDate = (DateTime)item.DeiliverDate!,
                    Status = item.TenderStatus.ToString(),
                    Bids = bidsForTender,
                    CompanyShipperId = item.CompanyShipperId,
                    CompanyCarrierId = (int)item.CompanyCarrierId,
                    CompanyShipperName = item.CompanyShipper!.CompanyName,
                    CompanyCarrierName = item.CompanyCarrier!.CompanyName,
                    Price = bidsForTender!.Where(b => b.IsSelected).Select(b => b.TransportPrice).FirstOrDefault()
                };
                data.Add(newTenderInExecutionDTO);
            }

            var listNotPaging = data.Where(c => dto.SearchText.IsNullOrEmpty()
                        || c.TenderName.Contains(dto.SearchText!)
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!)));

            switch (dto.searchLoads)
            {
                case SearchActiveByMachines.All:
                    break;
                case SearchActiveByMachines.LessThan8Tons:
                    listNotPaging = listNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight < 8000));
                    break;
                case SearchActiveByMachines.From8To22Tons:
                    listNotPaging = listNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 8000 && c.Weight < 22000));
                    break;
                case SearchActiveByMachines.From22Tons:
                    listNotPaging = listNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 22000));
                    break;
            }

            switch (dto.searchOffers)
            {
                case SearchActiveByOffers.All:
                    break;
                case SearchActiveByOffers.NoOffers:
                    listNotPaging = listNotPaging.Where(l => l.Bids.Count == 0);
                    break;
                case SearchActiveByOffers.MoreThan5Offers:
                    listNotPaging = listNotPaging.Where(l => l.Bids.Count > 5);
                    break;
                case SearchActiveByOffers.Max5Offers:
                    listNotPaging = listNotPaging.Where(l => l.Bids.Count <= 5);
                    break;
            }
            var listActivePaging = listNotPaging
                        .Skip((dto.pageIndex - 1) * dto.pageSize)
                        .Take(dto.pageSize);
            return new ResponseModel<List<TenderInExecutionDTO>>
            {
                Data = listActivePaging.ToList(),
                Success = true,
                Total = listNotPaging.Count(),
                PageCount = listNotPaging.Count() / 10 + 1
            }; 
        }

        public async Task<ResponseModel<List<TenderActiveDTO>>> SearchToAssignBy(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            var listToAssign = _tenderRepository
            .AsNoTracking()
            .Include(t => t.CompanyShipper)
            .Where(t => t.TenderStatus == TenderStatuses.Active)
            .ToList();

            var currentUserCompany = _companyRepository
                                    .AsNoTracking()
                                    .Where(c => c.Id == currentUser.CompanyId)
                                    .FirstOrDefault();

            if (currentUserCompany!.Role == CompanyRoleEnum.Shipper)
            {
                listToAssign = listToAssign.Where(t =>
                    t.CompanyShipperId! == currentUser.CompanyId).ToList();

                listToAssign = listToAssign.Where(t => (t.EndDate - DateTime.Now).TotalNanoseconds <= 0).ToList();
            }

            var listTenderToAssignDTO = new List<TenderActiveDTO>();
            foreach (var item in listToAssign)
            {
                var timeRemains = (item.EndDate - DateTime.Now).TotalSeconds > 0 ? (item.EndDate - DateTime.Now).TotalSeconds:0;
                var newTenderToAssignDTO = new TenderActiveDTO
                {
                    TenderId = item.Id,
                    TenderName = item.Name,
                    From = item.PickUpAddress,
                    To = item.DeliveryAddress,
                    PickUpDate = (DateTime)item.PickUpDate,
                    DeliveryDate = (DateTime)item.DeiliverDate,
                    Status = item.TenderStatus.ToString(),
                    TimeRemaining = timeRemains,
                    CompanyShipperId = item.CompanyShipperId,
                    CompanyShipperName = item.CompanyShipper!.CompanyName
                };

                var constructionMachines = await _machineService.GetMachinesForTenderOverviewByIds(item.Id);
                newTenderToAssignDTO.ConstructionMachines = constructionMachines.Data;
                var Bids = await _tenderBidService.GetTenderBidsByTenderId(item.Id);
                newTenderToAssignDTO.Bids = Bids.Data;
                if (newTenderToAssignDTO.Bids.Count > 0)
                {
                    if((currentUserCompany!.Role == CompanyRoleEnum.Carrier && newTenderToAssignDTO.Bids.Any(tb => tb.CompanyCarrierId == currentUser!.CompanyId))
                        || (currentUserCompany!.Role == CompanyRoleEnum.Shipper) 
                        || (currentUserCompany!.Role == CompanyRoleEnum.Admin && newTenderToAssignDTO.TimeRemaining <= 0 && newTenderToAssignDTO.Bids.Any()))
                        listTenderToAssignDTO.Add(newTenderToAssignDTO);
                }
            }

            var listToAssignNotPaging = listTenderToAssignDTO.Where(c => dto.SearchText.IsNullOrEmpty()
                        || c.TenderName.Contains(dto.SearchText!)
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!)));

            switch (dto.searchLoads)
            {
                case SearchActiveByMachines.All:
                    break;
                case SearchActiveByMachines.LessThan8Tons:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight < 8000));
                    break;
                case SearchActiveByMachines.From8To22Tons:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 8000 && c.Weight < 22000));
                    break;
                case SearchActiveByMachines.From22Tons:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 22000));
                    break;
            }

            switch (dto.searchOffers)
            {
                case SearchActiveByOffers.All:
                    break;
                case SearchActiveByOffers.NoOffers:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.Bids.Count == 0);
                    break;
                case SearchActiveByOffers.MoreThan5Offers:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.Bids.Count > 5);
                    break;
                case SearchActiveByOffers.Max5Offers:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.Bids.Count <= 5);
                    break;
            }
            var listActivePaging = listToAssignNotPaging
                        .Skip((dto.pageIndex - 1) * dto.pageSize)
                        .Take(dto.pageSize);

            return new ResponseModel<List<TenderActiveDTO>>
            {
                Data = listActivePaging.ToList(),
                Success = true,
                Total = listToAssignNotPaging.Count(),
                PageCount = listToAssignNotPaging.Count() / 10 + 1
            };
        }

        public async Task<ResponseModel<List<TenderActiveDTO>>> SearchWithdrawBy(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            var listToAssign = _tenderRepository
            .AsNoTracking()
            .Include(t => t.CompanyShipper)
            .Where(t => t.TenderStatus == TenderStatuses.Withdrawn)
            .ToList();

            var currentUserCompany = _companyRepository
                                    .AsNoTracking()
                                    .Where(c => c.Id == currentUser.CompanyId)
                                    .FirstOrDefault();

            if (currentUserCompany!.Role == CompanyRoleEnum.Shipper)
            {
                listToAssign = listToAssign.Where(t =>
                    t.CompanyShipperId! == currentUser.CompanyId).ToList();

                listToAssign = listToAssign.Where(t => (t.EndDate - DateTime.Now).TotalNanoseconds <= 0).ToList();
            }

            var listTenderToAssignDTO = new List<TenderActiveDTO>();
            foreach (var item in listToAssign)
            {
                var timeRemains = (item.EndDate - DateTime.Now).TotalSeconds > 0 ? (item.EndDate - DateTime.Now).TotalSeconds : 0;
                var newTenderToAssignDTO = new TenderActiveDTO
                {
                    TenderId = item.Id,
                    TenderName = item.Name,
                    From = item.PickUpAddress,
                    To = item.DeliveryAddress,
                    PickUpDate = (DateTime)item.PickUpDate,
                    DeliveryDate = (DateTime)item.DeiliverDate,
                    Status = item.TenderStatus.ToString(),
                    TimeRemaining = timeRemains,
                    CompanyShipperId = item.CompanyShipperId,
                    CompanyShipperName = item.CompanyShipper!.CompanyName,
                    WithdrawReason = item.WithdrawReason!
                };

                var constructionMachines = await _machineService.GetMachinesForTenderOverviewByIds(item.Id);
                newTenderToAssignDTO.ConstructionMachines = constructionMachines.Data;
                var Bids = await _tenderBidService.GetTenderBidsByTenderId(item.Id);
                newTenderToAssignDTO.Bids = Bids.Data;
                if (newTenderToAssignDTO.Bids.Count > 0)
                {
                    if ((currentUserCompany!.Role == CompanyRoleEnum.Carrier && newTenderToAssignDTO.Bids.Any(tb => tb.CompanyCarrierId == currentUser!.CompanyId))
                        || (currentUserCompany!.Role == CompanyRoleEnum.Shipper))
                        listTenderToAssignDTO.Add(newTenderToAssignDTO);
                }
            }

            var listToAssignNotPaging = listTenderToAssignDTO.Where(c => dto.SearchText.IsNullOrEmpty()
                        || c.TenderName.Contains(dto.SearchText!)
                        || c.ConstructionMachines.Any(cm => cm.Name.Contains(dto.SearchText!)));

            switch (dto.searchLoads)
            {
                case SearchActiveByMachines.All:
                    break;
                case SearchActiveByMachines.LessThan8Tons:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight < 8000));
                    break;
                case SearchActiveByMachines.From8To22Tons:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 8000 && c.Weight < 22000));
                    break;
                case SearchActiveByMachines.From22Tons:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.ConstructionMachines.Any(c => c.Weight >= 22000));
                    break;
            }

            switch (dto.searchOffers)
            {
                case SearchActiveByOffers.All:
                    break;
                case SearchActiveByOffers.NoOffers:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.Bids.Count == 0);
                    break;
                case SearchActiveByOffers.MoreThan5Offers:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.Bids.Count > 5);
                    break;
                case SearchActiveByOffers.Max5Offers:
                    listToAssignNotPaging = listToAssignNotPaging.Where(l => l.Bids.Count <= 5);
                    break;
            }
            var listActivePaging = listToAssignNotPaging
                        .Skip((dto.pageIndex - 1) * dto.pageSize)
                        .Take(dto.pageSize);

            return new ResponseModel<List<TenderActiveDTO>>
            {
                Data = listActivePaging.ToList(),
                Success = true,
                Total = listToAssignNotPaging.Count(),
                PageCount = listToAssignNotPaging.Count() / 10 + 1
            };
        }

        public async Task<ResponseModel<Tender>> StartTender(int TenderId)
        {
            var tender = await _tenderRepository
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == TenderId);

            if(tender == null )
            {
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = "Can't find tender"
                };
            }    

            if(tender.TenderStatus != TenderStatuses.Draft)
            {
                return new ResponseModel<Tender>
                {
                    Success = false,
                    Message = "Tender status isn't draft"
                };
            }    

            tender.TenderStatus = TenderStatuses.Active;
            _tenderRepository.Update(tender);
            _tenderRepository.SaveChange();
            return new ResponseModel<Tender>
            {
                Data = tender,
                Success = true
            };
        }

        public async Task<GeneralResponse> UpdateStatusAuto(List<int> TenderIds)
        {
            var listTenders = await _tenderRepository
                .Queryable()
                .Where(t => TenderIds.Contains(t.Id))
                .ToListAsync();

            foreach(var item in listTenders)
            {
                item.TenderStatus = TenderStatuses.Withdrawn;
                item.WithdrawReason = "Auto withdraw by system";
            }

            _tenderRepository.UpdateRange(listTenders);
            _tenderRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật thành công");
        }

        public Task<GeneralResponse> UpdateStatusToAssign(int TenderId)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> UpdateWithdrawTender(WithdrawTenderDTO withdrawTenderDTO)
        {
            var tender = await _tenderRepository
                .Queryable()
                .Where(t => t.Id == withdrawTenderDTO.TenderID)
                .FirstOrDefaultAsync();

            if(tender == null)
            {
                return new GeneralResponse(false, "Không tìm thấy Tender");
            }

            tender.TenderStatus = TenderStatuses.Withdrawn;
            tender.WithdrawReason = withdrawTenderDTO.WithdrawReason;
            _tenderRepository.Update(tender);
            _tenderRepository.SaveChange();
            return new GeneralResponse(true, "Cập nhật thành công");
        }
    }
}
