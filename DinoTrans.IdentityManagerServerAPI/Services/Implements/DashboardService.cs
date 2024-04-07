using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Report;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DinoTrans.Shared.Contracts;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using DinoTrans.Shared.Repositories.Implements;
using System.Reflection.PortableExecutable;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class DashboardService:IDashboardService
    {
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly IConstructionMachineRepository _constructionMachineRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBillRepository _billRepository;
        private readonly ICompanyRepository _companyRepository;
        public DashboardService(ITenderRepository tenderRepository,
            ITenderBidRepository tenderBidRepository,
            IConstructionMachineRepository constructionMachineRepository,
            RoleManager<ApplicationRole> roleManager,
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IBillRepository billRepository,
            ICompanyRepository companyRepository) 
        {
            _tenderRepository = tenderRepository;
            _tenderBidRepository = tenderBidRepository;
            _constructionMachineRepository = constructionMachineRepository;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _billRepository = billRepository;
            _companyRepository = companyRepository;
        }

        public async Task<ResponseModel<DashboardForAdmin>> GetDashBoardForAdmin()
        {
            var companiesShipper = (from c in _companyRepository.AsNoTracking().Where(c => c.Role == CompanyRoleEnum.Shipper)
                            join t in _tenderRepository.AsNoTracking() on c.Id equals t.CompanyShipperId
                            join u in _userRepository.AsNoTracking() on c.Id equals u.CompanyId
                            join m in _constructionMachineRepository.AsNoTracking() on c.Id equals m.CompanyShipperId
                            select new
                            {
                                Id = c.Id,
                                CompanyName = c.CompanyName,                              
                                TenderId = t.Id,
                                Price = t.FinalPrice,
                                UserId = u.Id,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                MachineId = m.Id
                            }).ToList();

            var totalShipperMoneyGroup = companiesShipper
                .Select(c => new
                {
                    c.Id,
                    c.Price
                })
                .GroupBy(c => c.Id)
                .Select(c => new
                {
                    Id = c.Key,
                    TotalPrice = c.Sum(c => c.Price)
                }).ToList();

            foreach(var item in companiesShipper)
            {
                var newCompanyReport = new CompanyReport
                {
                    CompanyId = item.Id,
                    CompanyName = item.CompanyName,

                };
            }    

            return new ResponseModel<DashboardForAdmin>
            {

            };
        }

        public async Task<ResponseModel<DashboardForShipper>> GetDashBoardForShipper(ApplicationUser _currentUser)
        {
            var tenders = await _tenderRepository
                .AsNoTracking()
                .Include(t => t.CompanyCarrier)
                .Where(t => t.CompanyShipperId == _currentUser.CompanyId
                && t.TenderStatus != TenderStatuses.Draft)
                .ToListAsync();

            var activeTenders = tenders
                .Where(t => t.TenderStatus == TenderStatuses.Active
                && (t.EndDate - DateTime.Now).TotalSeconds > 0)
                .Count();

            var toAssignTenders = tenders
                .Where(t => t.TenderStatus == TenderStatuses.Active
                && (t.EndDate - DateTime.Now).TotalSeconds <= 0
                && _tenderBidRepository.AsNoTracking().Any(tb => t.Id == tb.TenderId))
                .Count();

            var withdrawTenders = tenders.Where(t => t.TenderStatus == TenderStatuses.Withdrawn
            || (t.TenderStatus == TenderStatuses.Active
                && (t.EndDate - DateTime.Now).TotalSeconds <= 0
                && !_tenderBidRepository.AsNoTracking().Any(tb => t.Id == tb.TenderId))).Count();

            var inexecutionTenders = tenders.Where(t => t.TenderStatus == TenderStatuses.InExcecution).Count();

            var completedTenders = tenders.Where(t => t.TenderStatus == TenderStatuses.Completed).Count();

            var machines = _constructionMachineRepository
                .AsNoTracking()
                .Where(c => c.CompanyShipperId == _currentUser.CompanyId)
                .Count();

            var percentSubmitForTender = tenders.Count() != 0 ? ((float) _tenderRepository
                .AsNoTracking()
                .Where(t => _tenderBidRepository.AsNoTracking().Any(tb => tb.TenderId == t.Id))
                .Count() / tenders.Count() * 100) : 0;

            var percentWithdrawTender = tenders.Count() != 0 ?((float) withdrawTenders / tenders.Count() * 100):0;

            var moneyInCompletedTenders = tenders
                .Where(t => t.TenderStatus == TenderStatuses.Completed)
                .Sum(t => t.FinalPrice);

            var adminRole = await _roleManager.FindByNameAsync(Role.DinoTransAdmin);
            var adminId = await _userRoleRepository
                .AsNoTracking()
                .Where(ur => ur.RoleId == adminRole!.Id)
                .Select(ur => ur.UserId)
                .FirstOrDefaultAsync();
            var user = await _userRepository
                .AsNoTracking()
                .Include(u => u.Company)
                .Where(u => u.Id == adminId)
                .FirstOrDefaultAsync();

            var tenderCompleted = tenders.Where(t => t.TenderStatus == TenderStatuses.Completed).ToList();

            var Bids = _tenderBidRepository
                .AsNoTracking()
                .Where(tb => tenderCompleted
                    .Select(t => t.Id).Contains(tb.TenderId)
                    && tb.IsSelected == true)
                .ToList();
                
            var bills = _billRepository
                .AsNoTracking()
                .Where(b => Bids.Select(tb => tb.Id).Contains(b.TenderBidId)
                && b.BillType == BillTypeEnum.ShipperToAdminDinoTrans)
                .ToList();

            var totalTransferMoney = bills.Sum(b => b.vnp_Amount);

            var totalTransportPrice = Bids
                .Where(b => bills.Select(bs => bs.TenderBidId).Contains(b.Id))
                .Sum(tb => tb.TransportPrice);

            var statisticByCompany = tenders.Where(t => t.TenderStatus == TenderStatuses.Completed).GroupBy(t => new { t.CompanyCarrierId, t.CompanyCarrier!.CompanyName}).Select(t => new TotalMoneyByCompany
            {
                CompanyName = t.Key.CompanyName,
                TotalMoney = (float)t.Sum(t => t.FinalPrice)
            }).ToList();

            return new ResponseModel<DashboardForShipper>
            {
                Data = new DashboardForShipper
                {
                    ActiveTenderNumber = activeTenders,
                    ToAssignTenderNumber = toAssignTenders,
                    WithdrawTenderNumber = withdrawTenders,
                    InExecutionTenderNumber = inexecutionTenders,
                    ConstructionMachineNumber = machines,
                    PercentSubmitForTender = percentSubmitForTender,
                    PercentWithdrawTender = percentWithdrawTender,
                    TotalSuccessTenderMoney = (float) moneyInCompletedTenders,
                    CompletedTenderNumber = completedTenders,
                    AdminInfo = user!,
                    StatisticByCompany = statisticByCompany,
                    TotalMoneyForAdmin = (float)totalTransferMoney - totalTransportPrice
                },
                Success = true
            };
        }
    }
}
