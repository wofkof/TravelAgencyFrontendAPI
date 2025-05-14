using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.ViewModels.Order
{
    public class OrderEditViewModel
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int MemberId { get; set; }
        [Required]
        public int ItemId { get; set; }
        [Required]
        public ProductCategory Category { get; set; }
        [Required]
        public int ParticipantId { get; set; }
        [Required]
        public int ParticipantsCount { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Note { get; set; }
        public SelectList? OfficialTravels { get; set; } // Add this
        public SelectList? CustomTravels { get; set; }    // Add this
    }
}