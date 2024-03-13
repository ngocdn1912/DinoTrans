using DinoTrans.Shared.DTOs;

namespace DinoTrans.BlazorWebAssembly.DTOs_shared
{
    public class TenderStep2To3 : TenderChangeStepDTO
    {
        public List<InputFileData>? InputFileData { get; set; }
    }
}
