using DinoTrans.Shared.Contracts;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.DTOs.ContructionMachine;
using DinoTrans.Shared.DTOs.SearchDTO;
using DinoTrans.Shared.Entities;
using DinoTrans.Shared.Repositories.Interfaces;
using DinoTrans.Shared.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using static DinoTrans.Shared.DTOs.ServiceResponses;

namespace DinoTrans.IdentityManagerServerAPI.Services.Implements
{
    public class ConstructionMachineService : IConstructionMachineService
    {
        private readonly IConstructionMachineRepository _constructionMachineRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ITenderConstructionMachineRepository _tenderConstructionMachineRepository;

        public ConstructionMachineService(IConstructionMachineRepository contructionMachineRepository,
            ICompanyRepository companyRepository,
            ITenderRepository tenderRepository,
            ITenderConstructionMachineRepository tenderConstructionMachineRepository)
        {
            _constructionMachineRepository = contructionMachineRepository;
            _companyRepository = companyRepository;
            _tenderRepository = tenderRepository;
            _tenderConstructionMachineRepository = tenderConstructionMachineRepository;
        }

        public async Task<GeneralResponse> CreateContructionMachine(CreateContructionMachineDTO dto)
        {
            if (dto is null) return new GeneralResponse(false, "Model is empty");
            try
            {
                // Kiểm tra xem máy đã có trong cơ sở dữ liệu hay chưa
                var existingContructionMachine = await _constructionMachineRepository
                    .AsNoTracking()
                    .Where(c => c.SerialNumber == dto.SerialNumber)
                    .FirstOrDefaultAsync();

                if (existingContructionMachine != null)
                {
                    return new GeneralResponse(false, "Máy đã tồn tại");
                }

                //Kiểm tra công ty có tồn tại không
                var findCompany = _companyRepository
                .AsNoTracking()
                .Where(c => c.Id == dto.CompanyShipperId)
                .FirstOrDefault();
                if (findCompany == null)
                {
                    return new GeneralResponse(false, "Company not found");
                }

                // Tiếp tục kiểm tra Role 
                if (findCompany.Role == CompanyRoleEnum.Carrier)
                {
                    return new GeneralResponse(false, "Forbidden");
                }

                // Thêm máy mới
                var newContructionMachine = new ContructionMachine
                {
                    Name = dto.Name,
                    Brand = dto.Brand,
                    SerialNumber = dto.SerialNumber,
                    Height = dto.Height,
                    Weight = dto.Weight,
                    Length = dto.Length,
                    Width = dto.Width,
                    CompanyShipperId = dto.CompanyShipperId
                };
                var listImage = new List<Dictionary<string,string>>();
                if (dto.Image != null && dto.Image.Count > 0)
                {
                    for(int i = 0; i <  dto.Image.Count; i++) 
                    {
                        var newImage = new Dictionary<string, string>()
                        {
                            {$"Image_{i}",dto.Image[i] }
                        };
                        listImage.Add(newImage);
                    }
                    newContructionMachine.Image = JsonConvert.SerializeObject(listImage);
                }
                _constructionMachineRepository.Add(newContructionMachine);
                _constructionMachineRepository.SaveChange();

                return new GeneralResponse(true, "Thêm mới máy thành công");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }

        public async Task<ResponseModel<List<ContructionMachine>>> GetMachinesForTenderOverviewByIds(int TenderId)
        {
            var constructionMachinesIds = await _tenderConstructionMachineRepository
                .AsNoTracking()
                .Where(t =>  t.TenderId == TenderId)
                .Select(t => t.ContructionMachineId)
                .ToListAsync();

            if(constructionMachinesIds == null)
            {
                return new ResponseModel<List<ContructionMachine>>
                {
                    Success = false,
                    Message = $"Không tìm thấy Tender với Id = {TenderId} để kiểm tra máy"
                };
            }

            var machines = await _constructionMachineRepository
                .AsNoTracking()
                .Where(c => constructionMachinesIds.Contains(c.Id))
                .ToListAsync();
            
            if(machines == null)
            {
                return new ResponseModel<List<ContructionMachine>>
                {
                    Success = false,
                    Message = "Máy không tồn tại"
                };
            }    
            return new ResponseModel<List<ContructionMachine>>()
            {
                Success = true,
                Data = machines
            };
        }

        public async Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachineForTender(SearchLoadForTenderDTO dto)
        {
            var tender = await _tenderRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.TenderId)
                .FirstOrDefaultAsync();

            if(tender == null)
            {
                return new ResponseModel<SearchConstructionMachineDTO>
                {
                    Success = false,
                    Message = "Can't find tender",
                    ResponseCode = "Not found"
                };
            }    

            var listActiveTenderIds = await _tenderRepository
                .AsNoTracking()
                .Where(
                t => ((t.PickUpDate <= dto.PickUpDate && t.DeiliverDate >= dto.PickUpDate)
                || (t.PickUpDate <= dto.DeliveryDate && t.DeiliverDate >= dto.DeliveryDate)
                || (t.PickUpDate >= dto.PickUpDate && t.DeiliverDate <= dto.DeliveryDate))
                && t.TenderStatus != TenderStatuses.Draft
                && t.TenderStatus != TenderStatuses.Withdrawn
                && t.TenderStatus != TenderStatuses.Completed
                && t.CompanyShipperId == tender.CompanyShipperId)
                .Select(t => t.Id)
                .ToListAsync();

            var listLoadsInUse = await _tenderConstructionMachineRepository
                .AsNoTracking()
                .Where(tm => listActiveTenderIds.Contains(tm.TenderId))
                .Select(tm => tm.ContructionMachineId)
                .Distinct()
                .ToListAsync();

            var loadsAvailable = _constructionMachineRepository
                .AsNoTracking()
                .Where(c => (dto.SearchText.IsNullOrEmpty() || c.Name.Contains(dto.SearchText!)) && !listLoadsInUse.Contains(c.Id));

            var data = await loadsAvailable
                .Skip((dto.pageIndex - 1) * dto.pageSize)
                .Take(dto.pageSize)
                .ToListAsync();

            return new ResponseModel<SearchConstructionMachineDTO>
            {
                Data = new SearchConstructionMachineDTO
                {
                    TotalPage = loadsAvailable.Count()/5 + 1,
                    contructionMachines = data
                },
                Success = true
            };
        }
        public async Task<ResponseModel<SearchConstructionMachineDTO>> SearchConstructionMachine(SearchLoadDTO dto)
        {
            var company = await _companyRepository
                .AsNoTracking()
                .Where(t => t.Id == dto.CompanyId)
                .FirstOrDefaultAsync();
            if (company == null)
            {
                return new ResponseModel<SearchConstructionMachineDTO>
                {
                    Success = false,
                    Message = "Company not found",
                    ResponseCode = "NotFound"
                };
            }
            if (company.Role != CompanyRoleEnum.Shipper )
            {
                return new ResponseModel<SearchConstructionMachineDTO>
                {
                    Success = false,
                    Message = "Not have permission",
                    ResponseCode = "PermissionDenied"
                };
            }
            var constructionmachines = await _constructionMachineRepository
            .AsNoTracking()
            .Where(c => c.CompanyShipperId == dto.CompanyId)
            .ToListAsync();

            var searchconstructionmachines = _constructionMachineRepository
                .AsNoTracking()
                .Where(c => dto.SearchText.IsNullOrEmpty() || c.Name.Contains(dto.SearchText!));

            var data = await searchconstructionmachines
                .Skip((dto.pageIndex - 1) * dto.pageSize)
                .Take(dto.pageSize)
                .ToListAsync();

            return new ResponseModel<SearchConstructionMachineDTO>
            {
                Data = new SearchConstructionMachineDTO
                {
                    TotalPage = searchconstructionmachines.Count() / 10 + 1,
                    contructionMachines = data
                },
                Success = true
            };
        }
    }
}
