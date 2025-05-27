using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using TravelAgency.Shared.Models; // For PaymentMethod enum

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
    public class OrderCreateDto
    {
        [Required(ErrorMessage = "�������ѭq���`���B")]
        [Range(0.01, double.MaxValue, ErrorMessage = "�`���B�����j��0")]
        public decimal TotalAmount { get; set; }

        [StringLength(200, ErrorMessage = "�q��Ƶ��L��")]
        public string? OrderNotes { get; set; }

        [Required(ErrorMessage = "�q�ʤH��T������")]
        public OrdererInfoDto OrdererInfo { get; set; } = null!;

        [Required(ErrorMessage = "�ܤֻݭn�@��ȫ�")]
        [MinLength(1, ErrorMessage = "�ܤֻݭn�@��ȫ�")]
        public List<OrderParticipantDto> Participants { get; set; } = new List<OrderParticipantDto>();

        //[Required(ErrorMessage = "�o���ШD��T������")]
        //public OrderInvoiceRequestDto InvoiceRequestInfo { get; set; }

        //[Required(ErrorMessage = "������ܥI�ڤ覡")]
        //public PaymentMethod SelectedPaymentMethod { get; set; } // �ϥΪ̦b�e�ݿ�ܪ��I�ڤ覡

        [Required(ErrorMessage = "���������ʪ����ӫ~")]
        [MinLength(1, ErrorMessage = "�ʪ����ܤֻݭn�@���ӫ~")]
        public List<CartItemInputDto> CartItems { get; set; } = new();

        public List<TravelerProfileDto>? TravelerProfileActions { get; set; }

        [Required(ErrorMessage = "�������ѷ|��ID")]
        public int MemberId { get; set; }
    }
}