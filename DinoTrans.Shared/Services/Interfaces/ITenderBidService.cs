using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface ITenderBidService
    {
        Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId);
        Task<ResponseModel<TenderBid>> SubmitTenderBid(TenderBidDTO dto, ApplicationUser currentUser);
        Task<GeneralResponse> ChooseTenderBid(int TenderBidId, ApplicationUser currentUser);
        Task<ResponseModel<TenderBid>> UpdateTenderBid(UpdateTenderBidDTO dto);
        Task<ResponseModel<TenderBid>> DeleteTenderBid (int TenderBidId);
    }
}
