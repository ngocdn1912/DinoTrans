using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [Authorize]
    [ApiController]
    public class ConstructionMachineController : ControllerBase
    {
        private readonly IConstructionMachineService _constructionMachineService;
        public ConstructionMachineController(IConstructionMachineService constructionMachineService)
        {
            _constructionMachineService = constructionMachineService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            var result =  await _constructionMachineService.CreateContructionMachine(dto);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> SearchConstructionMachineForTender(SearchLoadForTenderDTO dto)
        {
            var result = await _constructionMachineService.SearchConstructionMachineForTender(dto);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> SearchConstructionMachine(SearchLoadDTO dto)
        {
            var result = await _constructionMachineService.SearchConstructionMachine(dto);
            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetMachinesForTenderOverviewByIds([FromQuery] int TenderId)
        {
            var result = await _constructionMachineService.GetMachinesForTenderOverviewByIds(TenderId);
            return Ok(result);
        }
    }
}
