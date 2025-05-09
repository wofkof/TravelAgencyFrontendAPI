﻿namespace TravelAgencyFrontendAPI.Models
{
    public enum OrderStatus
    {
        Pending,
        Awaiting,
        Completed,
        Cancelled
    }
    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        Cash,
        Other
    }
    public enum InvoiceOption
    {
        Personal,
        Company
    }
    public class Order
    {
        public int OrderId { get; set; }
        public int MemberId { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod? PaymentMethod { get; set; } 
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PaymentDate { get; set; }
        public string? InvoiceDeliveryEmail { get; set; }
        public InvoiceOption InvoiceOption { get; set; } = InvoiceOption.Personal;
        public string? InvoiceUniformNumber { get; set; }
        public string? InvoiceTitle { get; set; }
        public bool InvoiceAddBillingAddr { get; set; } = false;
        public string? InvoiceBillingAddress { get; set; }
        public string? Note { get; set; }

        public Member Member { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<OrderInvoice> OrderInvoices { get; set; } = new List<OrderInvoice>();
        public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
    }
}
