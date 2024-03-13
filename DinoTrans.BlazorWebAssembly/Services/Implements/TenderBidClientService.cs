using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class TenderBidClientService : ITenderBidService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/TenderBid";

        // Constructor nhận các dependency thông qua dependency injection
        public TenderBidClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        public async Task<ResponseModel<List<TenderBid>>> GetTenderBidsByTenderId(int TenderId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetTenderBidsByTenderId?TenderId={TenderId}");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<TenderBid>>
                {
                    Message = "Có lỗi xảy ra",
                    Success = false
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<TenderBid>>>(apiResponse);
        }

        public async Task<ResponseModel<TenderBid>> SubmitTenderBid(TenderBidDTO dto, ApplicationUser currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/SubmitTenderBid",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<TenderBid>
                {
                    Message = "Có lỗi xảy ra",
                    Success = false
                };    

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<TenderBid>>(apiResponse);
        }

        public async Task<ServiceResponses.GeneralResponse> ChooseTenderBid(int TenderBidId, ApplicationUser currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/ChooseTenderBid",
                Generics.GenerateStringContent(Generics.SerializeObj(TenderBidId)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ServiceResponses.GeneralResponse(false, "Lỗi xảy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ServiceResponses.GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<TenderBid>> UpdateTenderBid(UpdateTenderBidDTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/UpdateTenderBid",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<TenderBid>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<TenderBid>>(apiResponse);
        }

        public async Task<ResponseModel<TenderBid>> DeleteTenderBid(int TenderBidId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .DeleteAsync($"{BaseUrl}/DeleteTenderBid?TenderBidId={TenderBidId}");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<TenderBid>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<TenderBid>>(apiResponse);
        }
    }
}
