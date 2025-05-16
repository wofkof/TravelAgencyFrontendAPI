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
    }

    public class OrderDto // The main DTO for displaying a full order
    {
        public int OrderId { get; set; }
        public int MemberId { get; set; }

        // Orderer Snapshot Info
        //public string OrdererName { get; set; }
        //public string OrdererPhone { get; set; }
        //public string OrdererEmail { get; set; }

        public decimal TotalAmount { get; set; }
        public string? PaymentMethod { get; set; } // Enum to string
        public string OrderStatus { get; set; } // Enum to string
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }

        // Invoice Info (from Order model directly)
        public string InvoiceOption { get; set; } // Enum to string
        public string? InvoiceDeliveryEmail { get; set; }
        public string? InvoiceUniformNumber { get; set; }
        public string? InvoiceTitle { get; set; }
        public bool InvoiceAddBillingAddr { get; set; }
        public string? InvoiceBillingAddress { get; set; }
        // public bool IsInvoiceDonated { get; set; } (if you added this field)

        public List<OrderParticipantDto> Participants { get; set; } = new List<OrderParticipantDto>();
        public List<OrderDetailItemDto> OrderDetails { get; set; } = new List<OrderDetailItemDto>();

        // You might also want to include info from OrderInvoices if it's relevant here
        // public List<SomeOrderInvoiceDto> Invoices {get; set;}
    }
}