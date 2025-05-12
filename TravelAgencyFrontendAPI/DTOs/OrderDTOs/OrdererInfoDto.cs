using System.ComponentModel.DataAnnotations;

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
	public class OrdererInfoDto // �o�Ǹ�T���ѫ�ݹw��A�e�ݥi��Ȱ�Ū�Τ��\�����ק�
	{
		[Required(ErrorMessage = "�q�ʤH�m�W������")]
		[StringLength(100)]
		public string Name { get; set; }

		[Required(ErrorMessage = "�q�ʤH���������")]
		[Phone(ErrorMessage = "�п�J���Ī�������X")]
		[StringLength(20)]
		public string MobilePhone { get; set; }

		[Required(ErrorMessage = "�q�ʤH�q�l�H�c������")]
		[EmailAddress(ErrorMessage = "�п�J���Ī��q�l�H�c")]
		[StringLength(255)]
		public string Email { get; set; }
	}
}