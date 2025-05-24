using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelAgency.Shared.Models
{
    public class CompletedOrderDetail
    {
        public int CompletedOrderDetailId { get; set; }

        public int DocumentMenuId { get; set; }
        public DocumentMenu DocumentMenu { get; set; } = null!;

        public int OrderFormId { get; set; }
        public OrderForm OrderForm { get; set; } = null!;
    }

}
