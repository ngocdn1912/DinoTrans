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
            var companiesShipper = (from c in companies.Where(c => c.Role == CompanyRoleEnum.Shipper)
                            join t in _tenderRepository.AsNoTracking().Where(t => t.TenderStatus == TenderStatuses.Completed) on c.Id equals t.CompanyShipperId
                            select new
                            {
                                CompanyId = c.Id,
                                CompanyName = c.CompanyName,                              
                                Price = t.FinalPrice,
                                Role = c.Role
                            }).ToList();

            var companiesCarrier = (from c in companies.Where(c => c.Role == CompanyRoleEnum.Carrier)
                                   join t in _tenderRepository.AsNoTracking().Where(t => t.TenderStatus == TenderStatuses.Completed) on c.Id equals t.CompanyCarrierId
                                   select new
                                   {
                                       CompanyId = c.Id,
                                       CompanyName = c.CompanyName,
                                       Price = t.FinalPrice,
                                       Role = c.Role
                                   }).ToList();

            var usersInCompany = (from c in companies
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
                                 }).ToList();

            var machinesInCompany = (from c in companies
                                    join mc in _constructionMachineRepository.AsNoTracking() on c.Id equals mc.CompanyShipperId
                                    select new
                                    {
                                        CompanyId = c.Id,
                                        MachineId = c.Id
                                    }).ToList();

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
                    var group = totalMoneyGroupByShipper
                                .Where(t => t.CompanyId == item.Id).FirstOrDefault();
                    var groupAmount = group != null ? group.TotalPrice * item.ShipperFeePercentage / 100 : 0;
                    var resultItem = new CompanyReport
                    {
                        CompanyId = item.Id,
                        CompanyName = item.CompanyName,
                        Amount = (float)groupAmount,
                        UserCount = usersGroup != null ? usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.TotalUser)
                                .FirstOrDefault() : 0,
                        AdminName = usersGroup != null ? usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).FirstName
                                + " " + u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).LastName)
                                .FirstOrDefault() : "",
                        MachineCount = machinesGroup != null ? machinesGroup
                                       .Where(u => u.CompanyId == item.Id)
                                       .Select(m => m.TotalMachine).FirstOrDefault() : 0,
                        CompanyRole = CompanyRoleEnum.Shipper

                    };
                    listCompanyReport.Add(resultItem);
                }
                else if(item.Role == CompanyRoleEnum.Carrier)
                {
                    var group = totalMoneyGroupByCarrier
                                .Where(t => t.CompanyId == item.Id).FirstOrDefault();
                    var groupAmount = group != null ? group.TotalPrice * item.CarrierFeePercentage/100 : 0;
                    var resultItem = new CompanyReport
                    {
                        CompanyId = item.Id,
                        CompanyName = item.CompanyName,
                        Amount = (float)groupAmount,
                        UserCount = usersGroup != null ? usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.TotalUser)
                                .FirstOrDefault() : 0,
                        AdminName = usersGroup != null ? usersGroup
                                .Where(u => u.CompanyId == item.Id)
                                .Select(u => u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).FirstName
                                + " " + u.Users.FirstOrDefault(us => us.Role == Role.CompanyAdministrator).LastName)
                                .FirstOrDefault() : "",
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

        public async Task<ResponseModel<DashboardForCarrier>> GetDashBoardForCarrier(ApplicationUser _currentUser)
        {
            var tenderBids = await _tenderBidRepository
                .AsNoTracking()
                .Where(tb => tb.CompanyCarrierId == _currentUser.CompanyId)
                .ToListAsync();

            var getCompany = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == _currentUser.CompanyId)
                .FirstOrDefault();

            if (getCompany == null)
            {
                return new ResponseModel<DashboardForCarrier>
                {
                    Success = false,
                    Message = "Không tìm thấy công ty vận chuyển"
                };
            }

            decimal givenBids = tenderBids.Count();
            decimal selectedBids = tenderBids.Where(tb => tb.IsSelected).Count();

            var tendersInSelection = (from t in _tenderRepository.AsNoTracking().Where(t => t.TenderStatus == TenderStatuses.Active)
                                     join tb in _tenderBidRepository.AsNoTracking().Where(tb => tb.CompanyCarrierId == _currentUser.CompanyId) on t.Id equals tb.TenderId
                                     select new
                                     {
                                         tenderId = t.Id
                                     }).Select(t => t.tenderId).Count();

            var tendersCompleted = _tenderRepository
                .AsNoTracking()
                .Where(t => t.TenderStatus == TenderStatuses.Completed
                && t.CompanyCarrierId == _currentUser.CompanyId)
                .ToList();

            var tendersInExecution = _tenderRepository
                .AsNoTracking()
                .Where(t => t.TenderStatus == TenderStatuses.InExcecution
                && t.CompanyCarrierId == _currentUser.CompanyId)
                .Count();

            var countTendersCompleted = tendersCompleted.Count();

            decimal successRate = Math.Round(selectedBids / givenBids * 100, 2);
            var totalMoneyForAdmin = tendersCompleted.Sum(t => t.FinalPrice) * getCompany.CarrierFeePercentage / 100;
            var totalSuccessMoney = tendersCompleted.Sum(t => t.FinalPrice) - totalMoneyForAdmin;

            var bidsGroup = tendersCompleted
                .GroupBy(tb => tb.CompanyShipperId)
                .Select(t => new TotalMoneyByCompany
                {
                    CompanyName = _companyRepository
                    .AsNoTracking()
                    .Where(c => c.Id == t.Key)
                    .Select(t => t.CompanyName)
                    .FirstOrDefault(),
                    TotalMoney = (float)t!.Sum(t => t.FinalPrice)
                }).ToList();

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
            return new ResponseModel<DashboardForCarrier>
            {
                Data = new DashboardForCarrier
                {
                    GivenBids = (int)givenBids,
                    TendersInSelection = tendersInSelection,
                    SuccessRate = successRate,
                    TendersInExecution = tendersInExecution,
                    TendersCompleted = countTendersCompleted,
                    TotalMoneyForAdmin = (float)totalMoneyForAdmin,
                    TotalSuccessTenderMoney = (float)totalSuccessMoney,
                    TotalMoneyByShipperCompanies = bidsGroup,
                    AdminInfo = user!
                },
                Success = true
            };
        }
    }
}
