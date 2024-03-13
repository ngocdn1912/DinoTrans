using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Services.Interfaces;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.IdentityManagerServerAPI.ServiceFactory
{
    public class TenderServiceFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TenderServiceFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<ResponseModel<List<Tender>>> GetTendersActiveForAuto()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var result = new ResponseModel<List<Tender>>();
                var serviceProvider = scope.ServiceProvider;
                var tenderService = serviceProvider.GetRequiredService<ITenderService>();
                result = await tenderService.GetTendersActiveForAuto();
                if (result.Success)
                return new ResponseModel<List<Tender>>
                {
                    Data = result.Data,
                    Success = true
                };
                else return new ResponseModel<List<Tender>>
                {
                    Success = false
                };
            }
        }

        public async Task<GeneralResponse> UpdateStatusAuto(List<int> TenderIds)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var tenderService = serviceProvider.GetRequiredService<ITenderService>();
                var result = await tenderService.UpdateStatusAuto(TenderIds);               
                return new GeneralResponse(result.Flag,result.Message);
            }
        }
    }
}
