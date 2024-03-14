using Microsoft.AspNetCore.SignalR;
using DinoTrans.Shared;
using DinoTrans.Shared.Services.Interfaces;

namespace DinoTrans.IdentityManagerServerAPI.SignalR
{
    public class TenderOffersHub : Hub
    {
        private readonly IUserService _userService;

        public TenderOffersHub(IUserService userService)
        {
            _userService = userService;
        }

        public async Task SendNewBid(ActionTenderBid tenderBid)
        {
            await Clients.All.SendAsync("ReceiveNewBid", tenderBid);
        }

        public override async Task OnConnectedAsync()
        {
            if (int.TryParse(Context.UserIdentifier, out int UserId))
            {
                var user = await _userService.GetUserById(UserId);
                var result = user.Data;
            }
        }
    }
}
