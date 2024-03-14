using Microsoft.AspNetCore.SignalR;
using DinoTrans.Shared;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using DinoTrans.Shared.DTOs.TendersActive;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Implements;

namespace DinoTrans.IdentityManagerServerAPI.SignalR
{
    public class TenderOffersHub : Hub
    {
        public async Task SendTenders()
        {
            await Clients.All.SendAsync("ReceiveTenders");
        }
    }
}
