using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.DTOs.TendersActive;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ITenderService
    {
        Task<ResponseModel<Tender>> CreateTenderStep1(CreateTenderStep1DTO dto);
        Task<ResponseModel<Tender>> CreateTenderStep2(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto);
        Task<ResponseModel<Tender>> StartTender(int TenderId);
        Task<ResponseModel<List<TenderActiveDTO>>> SearchActiveBy (SearchTenderActiveDTO dto, ApplicationUser? currentUser);
        Task<ResponseModel<List<TenderActiveDTO>>> SearchToAssignBy(SearchTenderActiveDTO dto, ApplicationUser? currentUser);
        Task<ResponseModel<List<TenderActiveDTO>>> SearchWithdrawBy(SearchTenderActiveDTO dto, ApplicationUser? currentUser);
        Task<ResponseModel<List<TenderInExecutionDTO>>> SearchInExecution(SearchTenderActiveDTO dto, ApplicationUser? currentUser);
        Task<ResponseModel<List<TenderInExecutionDTO>>> SearchLost(SearchTenderActiveDTO dto, ApplicationUser? currentUser);

        /*      Task<ResponseModel<List<TenderActiveDTO_Test>>> SearchActiveBy_Test(SearchTenderActiveDTO dto, ApplicationUser? currentUser);
        */
        Task<ResponseModel<List<Tender>>> GetTendersActiveForAuto();
        Task<GeneralResponse> UpdateStatusAuto(List<int> TenderIds);
        Task<GeneralResponse> UpdateWithdrawTender(WithdrawTenderDTO withdrawTenderDTO);
        Task<ResponseModel<TenderDetailsDTO>> GetTenderById(int Id);
        Task<GeneralResponse> ConfirmCompleteTender(int TenderId, ApplicationUser? currentUser);
        Task<ResponseModel<List<TenderInExecutionDTO>>> SearchCompleted(SearchTenderActiveDTO dto, ApplicationUser? currentUser);
    }
}
