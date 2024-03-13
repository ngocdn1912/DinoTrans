using AutoMapper;
using DinoTrans.Shared.DTOs;
using DinoTrans.Shared.Entities;
namespace DinoTrans.Shared
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<TenderChangeStepDTO, Tender>();
        }
    }
}
