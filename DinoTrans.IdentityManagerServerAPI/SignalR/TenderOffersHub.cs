using DinoTrans.Shared.DTOs.TendersActive;
using DinoTrans.Shared.Entities;
using Microsoft.AspNetCore.SignalR;
using DinoTrans.Shared;

namespace DinoTrans.IdentityManagerServerAPI.SignalR
{
    public class TenderOffersHub : Hub
    {
        public async Task SendNewBid(ActionTenderBid tenderBid)
        {
            await Clients.All.SendAsync("ReceiveNewBid", tenderBid);
        }
    }
}
