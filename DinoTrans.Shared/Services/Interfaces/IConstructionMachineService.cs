using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.Shared.Services.Interfaces
{
    public interface IConstructionMachineService
    {
        Task<GeneralResponse> CreateContructionMachine(CreateContructionMachineDTO dto);
        Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachineForTender(SearchLoadForTenderDTO dto);
        Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachine(SearchLoadDTO dto);
        Task<ResponseModel<List<ContructionMachine>>> GetMachinesForTenderOverviewByIds(int TenderId);
        Task<ResponseModel<List<ContructionMachine>>> GetMachinesByCurrentShipperId(SearchLoadDTO dto, ApplicationUser applicationUser);
        Task<GeneralResponse> EditConstructionMachine(EditConstructionMachineDTO dto);
    }
}
