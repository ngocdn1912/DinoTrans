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
        private readonly IRoleRepository _roleRepository;
        public DashboardService(ITenderRepository tenderRepository,
            ITenderBidRepository tenderBidRepository,
            IConstructionMachineRepository constructionMachineRepository,
            RoleManager<ApplicationRole> roleManager,
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository,
            IBillRepository billRepository,
            ICompanyRepository companyRepository,
            IRoleRepository roleRepository) 
        {
            _tenderRepository = tenderRepository;
            _tenderBidRepository = tenderBidRepository;
            _constructionMachineRepository = constructionMachineRepository;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _billRepository = billRepository;
            _companyRepository = companyRepository;
            _roleRepository = roleRepository;
        }

        public async Task<ResponseModel<DashboardForAdmin>> GetDashBoardForAdmin()
        {
            var companies = _companyRepository.Queryable();
            var companiesShipper = from c in companies.Where(c => c.Role == CompanyRoleEnum.Shipper)
                            join t in _tenderRepository.AsNoTracking().Where(t => t.TenderStatus == TenderStatuses.Completed) on c.Id equals t.CompanyShipperId
                            select new
                            {
                                CompanyId = c.Id,
                                CompanyName = c.CompanyName,                              
                                Price = t.FinalPrice,
                                Role = c.Role
                            };

            var companiesCarrier = from c in companies.Where(c => c.Role == CompanyRoleEnum.Carrier)
                                   join t in _tenderRepository.AsNoTracking().Where(t => t.TenderStatus == TenderStatuses.Completed) on c.Id equals t.CompanyCarrierId
                                   select new
                                   {
                                       CompanyId = c.Id,
                                       CompanyName = c.CompanyName,
                                       Price = t.FinalPrice,
                                       Role = c.Role
                                   };

            var usersInCompany = from c in companies
                                 join u in _userRepository.AsNoTracking() on c.Id equals u.CompanyId
                                 join ur in _userRoleRepository.AsNoTracking() on u.Id equals ur.UserId
                                 join r in _roleRepository.AsNoTracking() on ur.RoleId equals r.Id
                                 select new
                                 {
                                     CompanyId = c.Id,
                                     UserId = u.Id,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     Role = r.Name
                                 };

            var machinesInCompany = from c in companies
                                    join mc in _constructionMachineRepository.AsNoTracking() on c.Id equals mc.CompanyShipperId
                                    select new
                                    {
                                        CompanyId = c.Id,
                                        MachineId = c.Id
                                    };

            var totalMoneyGroupByShipper = companiesShipper
                .Select(c => new
                {
                    c.CompanyId,
                    c.Price
                })
                .GroupBy(c => c.CompanyId)
                .Select(c => new
                {
                    CompanyId = c.Key,
                    TotalPrice = c.Sum(c => c.Price)
                }).ToList();

            var totalMoneyGroupByCarrier = companiesCarrier
                .Select(c => new
                {
                    c.CompanyId,
                    c.Price
                })
                .GroupBy(c => c.CompanyId)
                .Select(c => new
                {
                    CompanyId = c.Key,
                    TotalPrice = c.Sum(c => c.Price)
                }).ToList();

            var usersGroup = usersInCompany
                .Select(u => new
                {
                    u.CompanyId,
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Role
                })
                .GroupBy(u => u.CompanyId)
                .Select(u => new
                {
                    CompanyId = u.Key,
                    Users = u.ToList(),
                    TotalUser = u.Count()
                }).ToList();

            var machinesGroup = machinesInCompany
                .Select(c => new
                {
                    c.CompanyId,
                    c.MachineId
                })
                .GroupBy(c => c.CompanyId)
                .Select(c => new
                {
                    CompanyId = c.Key,
                    TotalMachine = c.Count()
                }).ToList();

            var listCompanyReport = new List<CompanyReport>();
            foreach (var item in companies)
            {
                if (item.Role == CompanyRoleEnum.Shipper)
                {
                    var resultItem = new CompanyReport
                    {
                        CompanyId = item.Id,
                        CompanyName = item.CompanyName,
                        Amount = (totalMoneyGroupByShipper != null && (float)totalMoneyGroupByShipper
                                .Where(t => t.CompanyId == item.Id)
                                .Select(t => t.TotalPrice).FirstOrDefault() != null) ?
                                ((float)totalMoneyGroupByShipper
                                .Where(t => t.CompanyId == item.Id)
                                .Select(t => t.TotalPrice).FirstOrDefault() * item.ShipperFeePercentage /100) :
                                0,
                        UserCount = usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.TotalUser)
                                .FirstOrDefault(),
                        AdminName = usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).FirstName
                                + " " + u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).LastName)
                                .FirstOrDefault(),
                        MachineCount = machinesGroup
                                       .Where(u => u.CompanyId == item.Id)
                                       .Select(m => m.TotalMachine).FirstOrDefault(),
                        CompanyRole = CompanyRoleEnum.Shipper

                    };
                    listCompanyReport.Add(resultItem);
                }
                else if(item.Role == CompanyRoleEnum.Carrier)
                {
                    var resultItem = new CompanyReport
                    {
                        CompanyId = item.Id,
                        CompanyName = item.CompanyName,
                        Amount = (totalMoneyGroupByCarrier != null && (float)totalMoneyGroupByCarrier
                                .Where(t => t.CompanyId == item.Id)
                                .Select(t => t.TotalPrice).FirstOrDefault() != null) ?
                                ((float)totalMoneyGroupByCarrier
                                .Where(t => t.CompanyId == item.Id)
                                .Select(t => t.TotalPrice).FirstOrDefault() * item.CarrierFeePercentage /100) :
                                0,
                        UserCount = usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.TotalUser)
                                .FirstOrDefault(),
                        AdminName = usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).FirstName
                                + " " + u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).LastName)
                                .FirstOrDefault(),
                        MachineCount = 0,
                        CompanyRole = CompanyRoleEnum.Carrier
                    };
                    listCompanyReport.Add(resultItem);
                }    
            }

            return new ResponseModel<DashboardForAdmin>
            {
                Data = new DashboardForAdmin
                {
                    ListCompanyReports = listCompanyReport
                },
                Success = true
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

            var TotalMoneyForAdmin = (tenderCompleted.Sum(t => t.FinalPrice)) * user!.Company!.ShipperFeePercentage / 100;

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
                    TotalMoneyForAdmin = (float)TotalMoneyForAdmin
                },
                Success = true
            };
        }

        Task<ResponseModel<DashboardForCarrier>> IDashboardService.GetDashBoardForCarrier(ApplicationUser _currentUser)
        {
            throw new NotImplementedException();
        }
    }
}
