using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgency.Shared.Models
{
    public enum ProductCategory
    {
        GroupTravel,
        CustomTravel
    }
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }

        public ProductCategory Category { get; set; } 
        public int ItemId { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Note { get; set; }

        public Order Order { get; set; } = null!;


        [NotMapped]
        public GroupTravel? GroupTravel { get; set; } 
        [NotMapped]
        public CustomTravel? CustomTravel { get; set; }
        public ICollection<OrderParticipant> OrderParticipants { get; set; } = new List<OrderParticipant>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }

}
