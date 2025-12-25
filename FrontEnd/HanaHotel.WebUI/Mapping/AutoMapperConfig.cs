using AutoMapper;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.AboutDTO;
using HanaHotel.WebUI.DTOs.BookingDTO;
using HanaHotel.WebUI.DTOs.ContactDTO;
using HanaHotel.WebUI.DTOs.GuestDTO;
using HanaHotel.WebUI.DTOs.LoginDTO;
using HanaHotel.WebUI.DTOs.MessageCategoryDTO;
using HanaHotel.WebUI.DTOs.RegisterDTO;
using HanaHotel.WebUI.DTOs.RoleDTO;
using HanaHotel.WebUI.DTOs.RoomDTO;
using HanaHotel.WebUI.DTOs.ServiceDTO;
using HanaHotel.WebUI.DTOs.ReviewDTO;
using HanaHotel.WebUI.DTOs.UserDTO;
using HanaHotel.WebUI.DTOs.PromotionDTO;
//using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.PromotionDetailDTO;


namespace HanaHotel.WebUI.Mapping
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<ResultServiceDTO, Service>().ReverseMap();
            CreateMap<CreateServiceDTO, Service>().ReverseMap();
            CreateMap<UpdateServiceDTO, Service>().ReverseMap();

            CreateMap<ResultRoomDTO, Room>().ReverseMap();
            CreateMap<AddRoomDTO, Room>().ReverseMap();
            CreateMap<UpdateRoomDTO, Room>().ReverseMap();

            CreateMap<ResultGuestDTO, Guest>().ReverseMap();
            CreateMap<CreateGuestDTO, Guest>().ReverseMap();
            CreateMap<UpdateGuestDTO, Guest>().ReverseMap();

            CreateMap<CreateUserDTO, User>().ReverseMap();
            CreateMap<LoginUserDTO, User>().ReverseMap();
            CreateMap<UpdateUserDTO, User>().ReverseMap();


            CreateMap<ResultAboutDTO, About>().ReverseMap();

            CreateMap<ResultReviewDTO, Review>().ReverseMap();
            CreateMap<AddReviewDTO, Review>().ReverseMap();
            CreateMap<UpdateReviewDTO, Review>().ReverseMap();

            CreateMap<CreateBookingDTO, Booking>().ReverseMap();
            CreateMap<UpdateBookingDTO, Booking>().ReverseMap();

            CreateMap<CreateContactDTO, Contact>().ReverseMap();
            CreateMap<DetailContactDTO, Contact>().ReverseMap();
            CreateMap<InboxContactDTO, Contact>().ReverseMap();
            CreateMap<SendMessageDTO, Contact>().ReverseMap();
            CreateMap<AdminDashboardContactDTO, Contact>().ReverseMap();

            CreateMap<CreateRoleDTO, Role>().ReverseMap();
            CreateMap<UpdateRoleDTO, Role>().ReverseMap();

            CreateMap<ResultMessageCategoryDTO, MessageCategory>().ReverseMap();
            CreateMap<CreateMessageCategoryDTO, MessageCategory>().ReverseMap();
            CreateMap<UpdateMessageCategoryDTO, MessageCategory>().ReverseMap();

            // ======================= PROMOTION ===========================
            CreateMap<ResultPromotionDTO, Promotion>().ReverseMap();
            CreateMap<PromotionCreateDTO, Promotion>().ReverseMap();
            CreateMap<UpdatePromotionDTO, Promotion>().ReverseMap();

            // Promotion Detail
            CreateMap<ResultPromotionDetailDTO, PromotionDetail>().ReverseMap();
            CreateMap<CreatePromotionDetailDTO, PromotionDetail>().ReverseMap();
            CreateMap<UpdatePromotionDetailDTO, PromotionDetail>().ReverseMap();

        }
    }
}
