using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Report;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class DashboardServiceClient : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/Dashboard";
        public DashboardServiceClient(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        public async Task<ResponseModel<DashboardForShipper>> GetDashBoardForShipper(ApplicationUser _currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetDashBoardForShipper");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<DashboardForShipper>
                {
                    Message = "Có lỗi xảy ra",
                    Success = false
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<DashboardForShipper>>(apiResponse);
        }

        public async Task<ResponseModel<DashboardForAdmin>> GetDashBoardForAdmin()
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetDashBoardForAdmin");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<DashboardForAdmin>
                {
                    Message = "Có lỗi xảy ra",
                    Success = false
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<DashboardForAdmin>>(apiResponse);
        }

        public async Task<ResponseModel<DashboardForCarrier>> GetDashBoardForCarrier(ApplicationUser _currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetDashBoardForCarrier");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<DashboardForCarrier>
                {
                    Message = "Có lỗi xảy ra",
                    Success = false
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<DashboardForCarrier>>(apiResponse);
        }
    }
}
