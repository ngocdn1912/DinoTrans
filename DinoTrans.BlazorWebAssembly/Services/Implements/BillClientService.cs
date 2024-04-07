using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs.Company;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using static DinoTrans.Shared.DTOs.ServiceResponses;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.DTOs.SearchDTO;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class BillClientService : IBillService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/Bill";

        public BillClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        public async Task<ResponseModel<List<BillDTO>>> GetAllBills(SearchBill model, ApplicationUser _currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/GetAllBills",
                Generics.GenerateStringContent(Generics.SerializeObj(model)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<BillDTO>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<BillDTO>>>(apiResponse);
        }

        public async Task<ResponseModel<BillDTO>> GetBillDetail(int BillId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetBillDetail?BillId={BillId}");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<BillDTO>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<BillDTO>>(apiResponse);
        }
    }
}
