using System;
using System.Collections.Generic;

namespace HanaHotel.WebUI.Models
{
    public class PaymentViewModel
    {
        public int BookingId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Nights { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public List<PaymentRoomDetail> RoomDetails { get; set; } = new();
    }

    public class PaymentRoomDetail
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }               // original unit price
        public decimal EffectiveUnitPrice { get; set; }  // after promotion
        public decimal Subtotal { get; set; }            // EffectiveUnitPrice * Quantity * Nights

        // New: services & promotion info for display
        public List<string> Services { get; set; } = new();
        public string? PromotionName { get; set; }
        public double? PromotionDiscountPercent { get; set; }
        public decimal? PromotionDiscountAmount { get; set; }
    }
}