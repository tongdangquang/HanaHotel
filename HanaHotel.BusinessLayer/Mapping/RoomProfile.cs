using AutoMapper;
using HanaHotel.DtoLayer.DTOs.RoomDTO;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.BusinessLayer.Mapping
{
	public class RoomProfile : Profile
	{
		public RoomProfile()
		{
			// Add (ignore Id)
			CreateMap<RoomAddDTO, Room>()
				.ForMember(dest => dest.Id, opt => opt.Ignore());

			// Update: map fields (Id should be preserved)
			CreateMap<UpdateRoomDTO, Room>();

			// Room -> Result DTO (including ImagePaths list)
			CreateMap<Room, ResultRoomDTO>()
				.ForMember(dest => dest.ImagePaths, opt => opt.Ignore()); // ImagePaths filled by controller
		}
	}
}