using DinoTrans.Shared;
using DinoTrans.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using OfficeOpenXml;
using System.Reflection;

namespace DinoTrans.IdentityManagerServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ITenderRepository _tenderRepository;
        public FileController(IWebHostEnvironment env, IConfiguration config, ITenderRepository tenderRepository)
        {
            _env = env;
            _config = config;
            _tenderRepository = tenderRepository;
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadConstructionMachineImages(List<IFormFile> files)
        {
            List<UploadResult> uploadResults = new List<UploadResult>();
            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                var uploadFolder = _config.GetSection("FEImagesLink").Value!.ToString();
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadFolder!, uniqueFileName);

                await using FileStream fs = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fs);

                uploadResult.FilePath = filePath.Replace(_config.GetSection("FEProject").Value!.ToString(), "");
                uploadResults.Add(uploadResult);
            }
            return uploadResults;
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadTenderDocuments(List<IFormFile> files, [FromQuery] int TenderId)
        {
            List<UploadResult> uploadResults = new List<UploadResult>();
            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                var uploadFolder = Path.Combine(_config.GetSection("FETenderDocuments").Value!.ToString(), "Tender_" + TenderId);
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                string filePath = Path.Combine(uploadFolder!, uniqueFileName);

                await using FileStream fs = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fs);

                uploadResult.FilePath = filePath.Replace(_config.GetSection("FEProject").Value!.ToString(), "");
                uploadResults.Add(uploadResult);
            }
            return uploadResults;
        }

        [HttpPost]
        public IActionResult DownloadExcel<T>(List<T> dataList)
        {
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                // Thêm một worksheet mới vào package
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Data");

                // Tạo tiêu đề cột từ các thuộc tính của kiểu dữ liệu generics
                SetColumnHeaders<T>(worksheet);

                // Ghi dữ liệu từ danh sách vào Excel
                WriteDataToExcel(dataList, worksheet);

                // Lưu tệp Excel và trả về cho người dùng để tải xuống
                return SaveExcelFile(excelPackage);
            }
        }

        // Phương thức để tạo tiêu đề cột từ các thuộc tính của kiểu dữ liệu generics
        private void SetColumnHeaders<T>(ExcelWorksheet worksheet)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
            }
        }

        // Phương thức để ghi dữ liệu từ danh sách vào Excel
        private void WriteDataToExcel<T>(List<T> dataList, ExcelWorksheet worksheet)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                for (int j = 0; j < properties.Length; j++)
                {
                    worksheet.Cells[i + 2, j + 1].Value = properties[j].GetValue(dataList[i]);
                }
            }
        }

        // Phương thức để lưu tệp Excel và trả về FileContentResult cho việc tải xuống
        private IActionResult SaveExcelFile(ExcelPackage excelPackage)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                excelPackage.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "output.xlsx");
            }
        }
    }
}
