using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Report;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DinoTrans.Shared.Contracts;
using Microsoft.AspNetCore.Identity;

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
        public DashboardService(ITenderRepository tenderRepository,
            ITenderBidRepository tenderBidRepository,
            IConstructionMachineRepository constructionMachineRepository,
            RoleManager<ApplicationRole> roleManager,
            IUserRoleRepository userRoleRepository,
            IUserRepository userRepository) 
        {
            _tenderRepository = tenderRepository;
            _tenderBidRepository = tenderBidRepository;
            _constructionMachineRepository = constructionMachineRepository;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
        }

        public async Task<ResponseModel<DashboardForShipper>> GetDashBoardForShipper(ApplicationUser _currentUser)
        {
            var tenders = await _tenderRepository
                .AsNoTracking()
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
                    AdminInfo = user!
                },
                Success = true
            };
        }
    }
}
