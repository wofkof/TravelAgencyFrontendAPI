using System.ComponentModel.DataAnnotations;

namespace TravelAgencyFrontendAPI.DTOs.OrderDTOs
{
	public class OrdererInfoDto // 這些資訊應由後端預填，前端可能僅唯讀或允許有限修改
	{
		[Required(ErrorMessage = "訂購人姓名為必填")]
		[StringLength(100)]
		public string Name { get; set; }

		[Required(ErrorMessage = "訂購人手機為必填")]
		[Phone(ErrorMessage = "請輸入有效的手機號碼")]
		[StringLength(20)]
		public string MobilePhone { get; set; }

		[Required(ErrorMessage = "訂購人電子信箱為必填")]
		[EmailAddress(ErrorMessage = "請輸入有效的電子信箱")]
		[StringLength(255)]
		public string Email { get; set; }
	}
}