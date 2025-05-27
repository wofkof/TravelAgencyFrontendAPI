// File: DTOs/OrderDTOs/OrderDto.cs
using System;
using System.Collections.Generic;
using TravelAgency.Shared.Models; // For Enums like OrderStatus, PaymentMethod

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderDetailItemDto // DTO for OrderDetail
    {
        public int OrderDetailId { get; set; }
        public ProductCategory Category { get; set; }
        public int ItemId { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Note { get; set; }
        public string ProductType { get; set; } = string.Empty;
        public string OptionType { get; set; } = string.Empty;
    }

    public class OrderDto // The main DTO for displaying a full order
    {
        public int OrderId { get; set; }
        public int MemberId { get; set; }

        public string OrdererName { get; set; } = string.Empty;
        public string OrdererPhone { get; set; } = string.Empty;
        public string OrdererEmail { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; } // 這是訂單層級的備註

        public string InvoiceOption { get; set; } = string.Empty;
        public string? InvoiceDeliveryEmail { get; set; }
        public string? InvoiceUniformNumber { get; set; }
        public string? InvoiceTitle { get; set; }
        public string? InvoiceBillingAddress { get; set; }

        public List<OrderParticipantDto> Participants { get; set; } = new List<OrderParticipantDto>();
        public List<OrderDetailItemDto> OrderDetails { get; set; } = new List<OrderDetailItemDto>();

        public DateTime? ExpiresAt { get; set; }
        public string? MerchantTradeNo { get; set; }
    }
}