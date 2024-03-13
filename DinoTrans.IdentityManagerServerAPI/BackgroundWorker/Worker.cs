using DinoTrans.IdentityManagerServerAPI.ServiceFactory;
using DinoTrans.IdentityManagerServerAPI.SignalR;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DinoTrans.IdentityManagerServerAPI.BackgroundWorker
{
    public class TenderBackgroundService : BackgroundService
    {
        private readonly ILogger<TenderBackgroundService> _logger;
        private readonly TenderServiceFactory _tenderServiceFactory;

        public TenderBackgroundService(ILogger<TenderBackgroundService> logger,
            TenderServiceFactory tenderServiceFactory)
        {
            _logger = logger;
            _tenderServiceFactory = tenderServiceFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Lấy danh sách các buổi đấu giá cần kiểm tra
                    var tenders = await _tenderServiceFactory.GetTendersActiveForAuto();
                    if (!tenders.Success) return;
                    var listTenderIds = new List<int>();
                    if (tenders.Data.Count > 0)
                    {
                        foreach (var tender in tenders.Data)
                        {
                            // Kiểm tra xem buổi đấu giá đã kết thúc chưa
                            if (DateTime.Now > tender.EndDate)
                            {
                                // Cập nhật trạng thái của buổi đấu giá từ active sang end
                                listTenderIds.Add(tender.Id);
                            }
                        }
                        await _tenderServiceFactory.UpdateStatusAuto(listTenderIds);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing auctions.");
                }

                // Chờ một khoảng thời gian trước khi kiểm tra lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
