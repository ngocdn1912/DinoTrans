using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.Company;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;
using System.ComponentModel.Design;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class CompanyClientService : ICompanyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/Company";

        public CompanyClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        public async Task<ResponseModel<Company>> GetCompanyByCurrentUserId(ApplicationUser user)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetCompanyByCurrentUserId");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
            return new ResponseModel<Company>
            {
                Success = false,
                Message = "Có lỗi xẩy ra"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<Company>>(apiResponse);
        }

        public async Task<ServiceResponses.GeneralResponse> UpdateCompanyInforByAdminOfCompany(UpdateCompanyDTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/UpdateCompanyInforByAdminOfCompany",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false,"Có lỗi xẩy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<Company>> GetCompanyByCompanyId(int CompanyId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetCompanyByCompanyId?CompanyId={CompanyId}");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<Company>
                {
                    Success = false,
                    Message = "Có lỗi xẩy ra"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<Company>> (apiResponse);
        }

        public async Task<ResponseModel<List<GetAllCompanyDTO>>> GetAllCompaniesByAdmin()
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetAllCompaniesByAdmin");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<GetAllCompanyDTO>>
                {
                    Success = false,
                    Message = "Có lỗi xẩy ra"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<GetAllCompanyDTO>>>(apiResponse);
        }
    }
}
