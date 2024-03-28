using DinoTrans.Shared.DTOs;

namespace DinoTrans.BlazorWebAssembly.Services.Implements
{
    public interface IFileClientService
    {
        Task DownloadExcel(List<TenderActiveDTO> dto);
    }
}
