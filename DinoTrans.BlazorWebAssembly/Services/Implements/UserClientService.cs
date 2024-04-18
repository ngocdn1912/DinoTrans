using Blazored.LocalStorage;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.GenericModels;
using DinoTrans.Shared.Services.Interfaces;
using DinoTrans.Shared.DTOs.UserResponse;
using static DinoTrans.Shared.DTOs.ServiceResponses;
using AutoMapper.Configuration;
using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.Entities;
using System.ComponentModel.Design;
using DinoTrans.Shared.DTOs.SearchDTO;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    // Lớp triển khai của IUserService cho ứng dụng Blazor WebAssembly
    public class UserClientService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorageService;
        private const string BaseUrl = "api/User";

        // Constructor nhận các dependency thông qua dependency injection
        public UserClientService(HttpClient httpClient, IConfiguration configuration, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _localStorageService = localStorageService;
        }

        // Phương thức để tạo tài khoản người dùng thông qua API
        public async Task<GeneralResponse> CreateAccount(UserDTO userDTO)
        {
            // Gửi yêu cầu POST đến endpoint API để đăng ký tài khoản
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/register",
                // Tạo đối tượng StringContent từ đối tượng userDTO sau khi chuyển thành chuỗi JSON để gửi qua mạng trong yêu cầu HTTP POST
                Generics.GenerateStringContent(Generics.SerializeObj(userDTO)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new GeneralResponse(false, "Error occurred. Try again later...");

            // Đọc nội dung của phản hồi từ yêu cầu HTTP POST
            var apiResponse = await response.Content.ReadAsStringAsync();

            // Chuyển đổi chuỗi JSON (apiResponse) thành đối tượng GeneralResponse sử dụng DeserializeJsonString
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        // Phương thức để đăng nhập vào hệ thống thông qua API
        public async Task<LoginResponse> LoginAccount(LoginDTO loginDTO)
        {
            // Gửi yêu cầu POST đến endpoint API để đăng nhập
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/login",
                Generics.GenerateStringContent(Generics.SerializeObj(loginDTO)));

            // Đọc phản hồi từ API
            if (!response.IsSuccessStatusCode)
                return new LoginResponse(false, null!, "Error occurred. Try again later...");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<LoginResponse>(apiResponse);
        }


        public async Task<ResponseModel<UserInfoResponseDTO>> GetAllUserInfo(GetAllUserInfoDTO userDTO)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetAllUserInfo?UserId={userDTO.UserId}&CompanyId={userDTO.CompanyId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<UserInfoResponseDTO>
            {
                Success = false,
                ResponseCode = "500",
                Message = "Internal Server Error"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<UserInfoResponseDTO>>(apiResponse);
        }

        public async Task<GeneralResponse> ChangeUserPassword(ChangePasswordDTO changePasswordDTO)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/ChangePassword",
                Generics.GenerateStringContent(Generics.SerializeObj(changePasswordDTO)));

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false, "Internal server error");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }
        public async Task<GeneralResponse> UpdateIsAdminConfirm()
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PutAsync($"{BaseUrl}/UpdateAdminConfirm",null);

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false, "Internal server error");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);

        }
        public async Task<ResponseModel<CompanyRoleEnum>> GetCompanyRole(int CompanyId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetCompanyRole?companyId={CompanyId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<CompanyRoleEnum>
            {
                Success = false,
                Message = $"Cant get company Role with Id = {CompanyId}"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<CompanyRoleEnum>>(apiResponse);

        }

        public async Task <ResponseModel<ApplicationUser>> GetUserByIdAsync(int UserId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetUserById?UserId={UserId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<ApplicationUser>
            {
                Success = false,
                Message = $"Cant get user with Id = {UserId}"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<ApplicationUser>>(apiResponse);
        }

        public ResponseModel<ApplicationUser> GetUserById(int UserId)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> CreateAccountForUserOfCompany(CreateAccountForUserOfCompany dto, ApplicationUser _currentCompanyShipperAdmin)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/CreateAccountForUserOfCompany",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false, "Internal server error");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<List<GetEmployeeOfACompany>>> GetAllEmployeesOfACompany(SearchModel dto, ApplicationUser _currentCompanyShipperAdmin)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/GetAllEmployeesOfACompany",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<List<GetEmployeeOfACompany>>
            {
                Success = false,
                Message = "Internal server error"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<GetEmployeeOfACompany>>>(apiResponse);
        }

        public async Task<string> GetUserRole(int userId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetAllEmployeesOfACompany?userId={userId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return "Có lỗi xảy ra";

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<string>(apiResponse);
        }

        public async Task<GeneralResponse> UpdateAccountForUserOfCompany(UpdateAccountForUserOfCompany dto)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .PostAsync($"{BaseUrl}/UpdateAccountForUserOfCompany",
                Generics.GenerateStringContent(Generics.SerializeObj(dto)));

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false, "Có lỗi xảy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<string> GetCurrentUserRole(ApplicationUser user)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetCurrentUserRole");

            //Read Response
            if (!response.IsSuccessStatusCode) return "Có lỗi xảy ra";

            var apiResponse = await response.Content.ReadAsStringAsync();
            return apiResponse;
        }

        public async Task<GeneralResponse> DeleteUserAccount(int UserId)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .DeleteAsync($"{BaseUrl}/DeleteUserAccount?userId={UserId}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new GeneralResponse(false,"Có lỗi xảy ra");

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<GeneralResponse>(apiResponse);
        }

        public async Task<ResponseModel<List<ApplicationUser>>> GetUserByRole(string role)
        {
            string token = await _localStorageService.GetItemAsStringAsync("token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient
                .GetAsync($"{BaseUrl}/GetUserByRole?role={role}");

            //Read Response
            if (!response.IsSuccessStatusCode) return new ResponseModel<List<ApplicationUser>>
            {
                Success = false,
                ResponseCode = "500",
                Message = "Internal Server Error"
            };

            var apiResponse = await response.Content.ReadAsStringAsync();
            return Generics.DeserializeJsonString<ResponseModel<List<ApplicationUser>>>(apiResponse);
        }
    }
}
