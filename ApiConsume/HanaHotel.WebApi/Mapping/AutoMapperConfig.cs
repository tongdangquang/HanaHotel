using AutoMapper;
using HanaHotel.DtoLayer.DTOs.RoomDTO;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.DataAccessLayer.Abstract;
using System.Collections.Generic;
using System.Linq;

namespace HanaHotel.WebApi.Mapping
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Room, RoomAddDTO>().ReverseMap();
            CreateMap<Room, UpdateRoomDTO>().ReverseMap();

            // map list of image paths using resolver (resolver will be constructed via DI)
            CreateMap<Room, ResultRoomDTO>()
                .ForMember(dest => dest.ImagePaths, opt => opt.MapFrom<RoomImagesResolver>());
        }
    }

    // resolver uses IImageDal (injected by DI) to get image paths for a room
    public class RoomImagesResolver : IValueResolver<Room, ResultRoomDTO, List<string>>
    {
        private readonly IImageDal _imageDal;

        public RoomImagesResolver(IImageDal imageDal)
        {
            _imageDal = imageDal;
        }

        public List<string> Resolve(Room source, ResultRoomDTO destination, List<string> destMember, ResolutionContext context)
        {
            var images = _imageDal.GetList();
            return images
                .Where(i => i.RoomId == source.Id)
                .Select(i => i.ImagePath)
                .ToList();
        }
    }
}
