using Blazored.LocalStorage;
using DinoTrans.BlazorWebAssembly.Pages.Tender;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class ConstructionMachineClientService : IConstructionMachineService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/ConstructionMachine";

        // Constructor nhận các dependency thông qua dependency injection
        public ConstructionMachineClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }
        public async Task<GeneralResponse> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/CreateContructionMachine",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(true, "Tạo mới máy thành công");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachineForTender(SearchLoadForTenderDTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/SearchConstructionMachineForTender",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
            return new ResponseModel<SearchConstructionMachineDTO>
            {
                Success = false,
                Message = "Can't search construction machine"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<SearchConstructionMachineDTO>>(apiResponse);
        }
        public async Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachine(SearchLoadDTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/SearchConstructionMachine",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<SearchConstructionMachineDTO>
                {
                    Success = false,
                    Message = "Can't search construction machine"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<SearchConstructionMachineDTO>>(apiResponse);
        }


        public async Task<ResponseModel<List<ContructionMachine>>> GetMachinesForTenderOverviewByIds(int TenderId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetMachinesForTenderOverviewByIds?TenderId={TenderId}");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<ContructionMachine>>
                {
                    Success = false,
                    Message = "Can't search construction machine"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<ContructionMachine>>>(apiResponse);
        }

        public async Task<ResponseModel<List<ContructionMachine>>> GetMachinesByCurrentShipperId(SearchLoadDTO dto, ApplicationUser applicationUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/GetMachinesByCurrentShipperId",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<ContructionMachine>>
                {
                    Success = false,
                    Message = "Can't search construction machine"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<ContructionMachine>>>(apiResponse);
        }
    }
}
