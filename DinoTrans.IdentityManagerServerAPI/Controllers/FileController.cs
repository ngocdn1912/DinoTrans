using DinoTrans.Shared;
using DinoTrans.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
    }
}
