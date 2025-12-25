using System.Collections.Generic;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.PromotionDTO;
using HanaHotel.WebUI.DTOs.ServiceDTO;
using HanaHotel.WebUI.DTOs.PromotionDetailDTO;

namespace HanaHotel.WebUI.DTOs.RoomDTO
{
    public class ResultRoomDTO
    {
        public int Id { get; set; }

        public required string RoomName { get; set; }

        public RoomStatus Status { get; set; }

        public required string Description { get; set; }

        public double Size { get; set; }

        public decimal Price { get; set; }

        public required int BedCount { get; set; }

        public List<string> ImagePaths { get; set; } = new List<string>();

        // New: hotel display name / address (optional)
        public string? HotelName { get; set; }

        // New: list of service names associated with the room (optional)
        public List<string> Services { get; set; } = new List<string>();
        public List<string> Promotions { get; set; } = new List<string>();

        // New: promotion info (optional)
        public string? PromotionName { get; set; }
        public decimal? PromotionDiscountAmount { get; set; }     // from Promotion.DiscountAmount (absolute)
        public decimal? PromotionDiscountPercent { get; set; }    // from PromotionDetail.DiscountPercent (percent)

        // New: number of available rooms for this room type (populated from HotelDetail.RoomCount)
        // If 0 => no rooms available
        public int RoomCount { get; set; }

        // New: corresponding HotelDetail.Id to allow booking to reference correct HotelDetail row
        // Nullable because in some flows we may not have a direct mapping
        public int? HotelDetailId { get; set; }
    }
}
