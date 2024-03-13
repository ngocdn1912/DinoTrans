using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoTrans.Shared.DTOs
{
    // Định nghĩa một record để lưu trữ thông tin phiên người dùng sau khi đăng nhập.
    public record UserSession(string? Id, string? Name, string? Email, string? Role, string? CompanyId);

}
