using Blazored.LocalStorage;
using DinoTrans.Shared.GenericModels;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DinoTrans.BlazorWebAssembly.Authentication
{
    // Lớp triển khai của AuthenticationStateProvider cho ứng dụng Blazor WebAssembly
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private ClaimsPrincipal anonymous = new(new ClaimsIdentity());

        // Constructor nhận một đối tượng ILocalStorageService thông qua dependency injection
        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        // Phương thức này kiểm tra xem người dùng đã đăng nhập hay chưa và trả về AuthenticationState tương ứng
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Kiểm tra xem có JWT token được lưu trong local storage không
                string stringToken = await _localStorageService.GetItemAsStringAsync("token");
                if (string.IsNullOrWhiteSpace(stringToken))
                    return await Task.FromResult(new AuthenticationState(anonymous));

                // Lấy claims từ JWT token
                var claims = Generics.GetClaimsFromToken(stringToken);
                if (claims != null)
                {
                    var claimsPrincipal = Generics.SetClaimPrinciple(claims);
                    return await Task.FromResult(new AuthenticationState(claimsPrincipal));
                }
                else
                {
                    await _localStorageService.RemoveItemAsync("token");
                    return await Task.FromResult(new AuthenticationState(anonymous));
                };
            }
            catch
            {
                // Trong trường hợp lỗi, trả về trạng thái đăng nhập ẩn danh
                return await Task.FromResult(new AuthenticationState(anonymous));
            }
        }

        // Phương thức này được gọi khi người dùng đăng nhập hoặc đăng xuất để cập nhật AuthenticationState
        public async Task UpdateAuthenticationState(string? token)
        {
            ClaimsPrincipal claimsPrincipal = new();

            if (!string.IsNullOrWhiteSpace(token))
            {
                // Đăng nhập: Lấy thông tin từ JWT token, tạo ClaimsPrincipal và lưu token vào local storage
                var userSession = Generics.GetClaimsFromToken(token);
                claimsPrincipal = Generics.SetClaimPrinciple(userSession);
                await _localStorageService.SetItemAsStringAsync("token", token);
            }
            else
            {
                // Đăng xuất: Gán claimsPrincipal là anonymous và xóa token từ local storage
                claimsPrincipal = anonymous;
                await _localStorageService.RemoveItemAsync("token");
            }

            // Thông báo về sự thay đổi trong AuthenticationState
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
