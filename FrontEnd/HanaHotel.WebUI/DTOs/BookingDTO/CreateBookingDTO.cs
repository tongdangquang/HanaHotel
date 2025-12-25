using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.RoomDTO;

namespace HanaHotel.WebUI.DTOs.BookingDTO
{
    public class CreateBookingDTO
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Ngày nhận phòng là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Ngày trả phòng là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        // Optional: server can set BookingDate when creating
        public DateTime? BookingDate { get; set; }

        public string? AdditionalRequest { get; set; }

        // Keep for backward compatibility but not used in multi-room flow
        public int? RoomId { get; set; }

        public int? UserId { get; set; }

        public BookingStatus? Status { get; set; }

        // New: the client will post multiple room selections here
        public List<CreateRoomDetailDTO> RoomDetails { get; set; } = new List<CreateRoomDetailDTO>();

        // New: used to render available rooms on the left
        public List<ResultRoomDTO> AvailableRooms { get; set; } = new List<ResultRoomDTO>();
    }
}
