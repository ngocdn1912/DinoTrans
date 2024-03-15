using Blazored.LocalStorage;
using DinoTrans.BlazorWebAssembly.Pages.Tender;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.DTOs.TendersActive;
using DinoTrans.Shared.DTOs.TenderSteps;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public class TenderClientService : ITenderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/Tender";

        // Constructor nhận các dependency thông qua dependency injection
        public TenderClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }
        public async Task<ResponseModel<Shared.Entities.Tender>> CreateTenderStep1(CreateTenderStep1DTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/CreateTenderStep1",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<Shared.Entities.Tender>()
                {
                    Success = false,
                    Message = "Không thể tạo mới Tender"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<Shared.Entities.Tender>>(apiResponse);
        }

        public async Task<ResponseModel<Shared.Entities.Tender>> CreateTenderStep2(UpdateTenderStep2AndCreateTenderContructionMachineDTO dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/CreateTenderStep2",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<Shared.Entities.Tender>()
                {
                    Success = false,
                    Message = "Không thể hoàn tất bước 2"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<Shared.Entities.Tender>>(apiResponse);
        }

        public async Task<ResponseModel<Shared.Entities.Tender>> StartTender(int TenderId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/StartTender",
                Generics.GenerateStringContent(Generics.SerializeObj(TenderId)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<Shared.Entities.Tender>()
                {
                    Success = false,
                    Message = "Không thể hoàn tất đấu thầu"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<Shared.Entities.Tender>>(apiResponse);
        }

        public async Task<ResponseModel<List<TenderActiveDTO>>> SearchActiveBy(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .PostAsync($"{BaseUrl}/SearchActiveBy",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<TenderActiveDTO>>()
                {
                    Success = false,
                    Message = "Không thể tìm kiếm"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<TenderActiveDTO>>>(apiResponse);
        }

        public async Task<ResponseModel<List<Shared.Entities.Tender>>> GetTendersActiveForAuto()
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> UpdateStatusAuto(List<int> TenderIds)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .PostAsync($"{BaseUrl}/UpdateStatusAuto",
                Generics.GenerateStringContent(Generics.SerializeObj(TenderIds)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, "Fail to execute");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<TenderDetailsDTO>> GetTenderById(int Id)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .GetAsync($"{BaseUrl}/GetTenderById?TenderId={Id}");

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
            return new ResponseModel<TenderDetailsDTO>()
            {
                Success = false,
                Message = "Không thể tìm kiếm"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<TenderDetailsDTO>>(apiResponse);
        }

        public async Task<GeneralResponse> UpdateWithdrawTender(WithdrawTenderDTO withdrawTenderDTO)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .PostAsync($"{BaseUrl}/UpdateWithdrawTender",
            Generics.GenerateStringContent(Generics.SerializeObj(withdrawTenderDTO)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, "Có lỗi xảy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<List<TenderActiveDTO>>> SearchToAssignBy(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .PostAsync($"{BaseUrl}/SearchToAssignBy",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<TenderActiveDTO>>()
                {
                    Success = false,
                    Message = "Không thể tìm kiếm"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<TenderActiveDTO>>>(apiResponse);
        }

        public async Task<ResponseModel<List<TenderInExecutionDTO>>> SearchInExecution(SearchTenderActiveDTO dto, ApplicationUser? currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .PostAsync($"{BaseUrl}/SearchInExecution",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new ResponseModel<List<TenderInExecutionDTO>>()
                {
                    Success = false,
                    Message = "Không thể tìm kiếm"
                };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<TenderInExecutionDTO>>>(apiResponse);
        }

        public async Task<GeneralResponse> ConfirmCompleteTender(int TenderId, ApplicationUser? currentUser)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
            .PostAsync($"{BaseUrl}/ConfirmCompleteTender",
                Generics.GenerateStringContent(Generics.SerializeObj(TenderId)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, "Có lỗi chưa bắt xảy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }
    }
}
