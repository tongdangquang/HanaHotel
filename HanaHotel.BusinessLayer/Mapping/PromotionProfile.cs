using AutoMapper;
using HanaHotel.DtoLayer;
using HanaHotel.DtoLayer.DTOs.PromotionDTO;
using HanaHotel.EntityLayer.Concrete;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HanaHotel.BusinessLayer.Mapping
{
    public class PromotionProfile : Profile
    {
        public PromotionProfile()
        {
            CreateMap<PromotionCreateDto, Promotion>()
                .ForMember(dest => dest.PromotionID, opt => opt.Ignore())
                .ForMember(dest => dest.PromotionDetails, opt => opt.Ignore());

            CreateMap<PromotionDetailCreateDto, PromotionDetail>()
                .ForMember(dest => dest.PromotionDetailID, opt => opt.Ignore());

            CreateMap<Promotion, PromotionListDto>()
                .ForMember(dest => dest.PromotionId, opt => opt.MapFrom(s => s.PromotionID))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(s => s.PromotionDetails));

            CreateMap<PromotionDetail, PromotionDetailListDto>();
        }
    }
}
