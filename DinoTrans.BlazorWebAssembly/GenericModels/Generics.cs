using DinoTrans.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace DinoTrans.Shared.GenericModels
{
    // Lớp chứa các phương thức chung và tiện ích cho việc xử lý JWT và JSON
    public class Generics
    {
        // Phương thức để thiết lập ClaimsPrincipal từ mô hình UserSession
        public static ClaimsPrincipal SetClaimPrinciple(UserSession model)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, model.Id!.ToString()),
                new(ClaimTypes.Name, model.Name!),
                new(ClaimTypes.Email, model.Email!),
                new(ClaimTypes.Role, model.Role!),
                new("CompanyId", model.CompanyId!)
            }, "JwtAuth"));
        }

        // Phương thức để lấy thông tin UserSession từ chuỗi JWT
        public static UserSession GetClaimsFromToken(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            DateTime expirationTime = token.ValidTo;

            if (expirationTime < DateTime.UtcNow)
            {
                // Token đã hết hạn, xử lý tùy ý (ví dụ: throw exception, return null, ...)
                return null;
            }
            var claims = token.Claims;
            string Id = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value!;
            string Name = claims.First(c => c.Type == ClaimTypes.Name).Value!;
            string Email = claims.First(c => c.Type == ClaimTypes.Email).Value!;
            string Role = claims.First(c => c.Type == ClaimTypes.Role).Value!;
            string CompanyId = claims.First(c => c.Type == "CompanyId").Value!;
            return new UserSession(Id, Name, Email, Role,CompanyId);
        }

        // Phương thức để chuyển đối tượng thành chuỗi JSON
        public static string SerializeObj<T>(T modelObject)
        {
            return JsonSerializer.Serialize(modelObject, JsonOptions());
        }

        // Phương thức để chuyển chuỗi JSON thành đối tượng
        public static T DeserializeJsonString<T>(string jsonString) => JsonSerializer.Deserialize<T>(jsonString, JsonOptions())!;

        // Phương thức để chuyển chuỗi JSON thành danh sách đối tượng
        public static IList<T> DeserializeJsonStringList<T>(string jsonString) => JsonSerializer.Deserialize<IList<T>>(jsonString, JsonOptions())!;

        // Phương thức để cấu hình tùy chọn cho quá trình chuyển đổi JSON
        public static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            };
        }

        // Phương thức để tạo đối tượng StringContent từ chuỗi JSON
        public static StringContent GenerateStringContent(string serializedObj)
        {
            return new StringContent(serializedObj, System.Text.Encoding.UTF8, "application/json");
        }
    }
}
