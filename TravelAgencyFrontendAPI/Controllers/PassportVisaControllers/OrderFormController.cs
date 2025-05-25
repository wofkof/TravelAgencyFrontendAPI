using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data; // 引入 AppDbContext
using TravelAgency.Shared.Models; // 引入 OrderForm, Member, DocumentMenu 模型
using TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs; // 引入 OrderFormDto

namespace TravelAgencyFrontendAPI.Controllers.PassportVisaControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderFormController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderFormController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/OrderForm
        // 獲取所有訂單
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderFormDto>>> GetOrderForms()
        {
            var orderForms = await _context.OrderForms
                .Include(of => of.Member) // 包含相關的 Member 數據
                .Include(of => of.DocumentMenu) // 包含相關的 DocumentMenu 數據
                .Select(of => new OrderFormDto
                {
                    OrderId = of.OrderId,
                    MemberId = of.MemberId,
                    // 注意：在 DTO 中包含 Member 對象可能不是最佳實踐，
                    // 如果您只需要 Member 的部分信息，可以考慮創建一個 MemberDto
                    // 這裡為了簡化，直接包含 Member 物件，但通常我們會避免循環引用或傳輸過多數據。
                    Member = of.Member, // 直接包含 Member 模型，這可能需要調整
                    DocumentMenuId = of.DocumentMenuId,
                    DocumentMenu = of.DocumentMenu, // 直接包含 DocumentMenu 模型，這可能需要調整
                    DepartureDate = of.DepartureDate,
                    ProcessingQuantity = of.ProcessingQuantity,
                    ChineseSurname = of.ChineseSurname,
                    ChineseName = of.ChineseName,
                    EnglishSurname = of.EnglishSurname,
                    EnglishName = of.EnglishName,
                    Gender = of.Gender,
                    BirthDate = of.BirthDate,
                    ContactPersonName = of.ContactPersonName,
                    ContactPersonEmail = of.ContactPersonEmail,
                    ContactPersonPhoneNumber = of.ContactPersonPhoneNumber,
                    PickupMethodOption = of.PickupMethodOption,
                    MailingCity = of.MailingCity,
                    MailingDetailAddress = of.MailingDetailAddress,
                    StoreDetailAddress = of.StoreDetailAddress,
                    TaxIdOption = of.TaxIdOption,
                    CompanyName = of.CompanyName,
                    TaxIdNumber = of.TaxIdNumber,
                    OrderCreationTime = of.OrderCreationTime
                })
                .ToListAsync();

            return Ok(orderForms);
        }

        // GET: api/OrderForm/{id}
        // 根據ID獲取單個訂單
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderFormDto>> GetOrderForm(int id)
        {
            var orderForm = await _context.OrderForms
                .Include(of => of.Member)
                .Include(of => of.DocumentMenu)
                .FirstOrDefaultAsync(of => of.OrderId == id);

            if (orderForm == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 將模型映射到 DTO
            var orderFormDto = new OrderFormDto
            {
                OrderId = orderForm.OrderId,
                MemberId = orderForm.MemberId,
                Member = orderForm.Member,
                DocumentMenuId = orderForm.DocumentMenuId,
                DocumentMenu = orderForm.DocumentMenu,
                DepartureDate = orderForm.DepartureDate,
                ProcessingQuantity = orderForm.ProcessingQuantity,
                ChineseSurname = orderForm.ChineseSurname,
                ChineseName = orderForm.ChineseName,
                EnglishSurname = orderForm.EnglishSurname,
                EnglishName = orderForm.EnglishName,
                Gender = orderForm.Gender,
                BirthDate = orderForm.BirthDate,
                ContactPersonName = orderForm.ContactPersonName,
                ContactPersonEmail = orderForm.ContactPersonEmail,
                ContactPersonPhoneNumber = orderForm.ContactPersonPhoneNumber,
                PickupMethodOption = orderForm.PickupMethodOption,
                MailingCity = orderForm.MailingCity,
                MailingDetailAddress = orderForm.MailingDetailAddress,
                StoreDetailAddress = orderForm.StoreDetailAddress,
                TaxIdOption = orderForm.TaxIdOption,
                CompanyName = orderForm.CompanyName,
                TaxIdNumber = orderForm.TaxIdNumber,
                OrderCreationTime = orderForm.OrderCreationTime
            };

            return Ok(orderFormDto);
        }

        // POST: api/OrderForm
        // 創建新訂單
        [HttpPost]
        public async Task<ActionResult<OrderFormDto>> CreateOrderForm(OrderFormDto dto)
        {
            // 將 DTO 映射回模型
            var orderForm = new OrderForm
            {
                MemberId = dto.MemberId,
                DocumentMenuId = dto.DocumentMenuId,
                DepartureDate = dto.DepartureDate,
                ProcessingQuantity = dto.ProcessingQuantity,
                ChineseSurname = dto.ChineseSurname,
                ChineseName = dto.ChineseName,
                EnglishSurname = dto.EnglishSurname,
                EnglishName = dto.EnglishName,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                ContactPersonName = dto.ContactPersonName,
                ContactPersonEmail = dto.ContactPersonEmail,
                ContactPersonPhoneNumber = dto.ContactPersonPhoneNumber,
                PickupMethodOption = dto.PickupMethodOption,
                MailingCity = dto.MailingCity,
                MailingDetailAddress = dto.MailingDetailAddress,
                StoreDetailAddress = dto.StoreDetailAddress,
                TaxIdOption = dto.TaxIdOption,
                CompanyName = dto.CompanyName,
                TaxIdNumber = dto.TaxIdNumber,
                OrderCreationTime = DateTime.Now // 通常在創建時設置，而不是從 DTO 接收
            };

            _context.OrderForms.Add(orderForm);
            await _context.SaveChangesAsync(); // 保存到資料庫，此時 OrderId 會被填充

            // 重新加載相關的 Member 和 DocumentMenu 數據，以便在返回的 DTO 中包含它們
            await _context.Entry(orderForm).Reference(o => o.Member).LoadAsync();
            await _context.Entry(orderForm).Reference(o => o.DocumentMenu).LoadAsync();

            // 將新創建的模型數據（包括生成的 OrderId）更新回 DTO
            dto.OrderId = orderForm.OrderId;
            dto.OrderCreationTime = orderForm.OrderCreationTime;
            // 更新 DTO 中的導航屬性
            dto.Member = orderForm.Member;
            dto.DocumentMenu = orderForm.DocumentMenu;

            // 返回 201 Created 狀態碼，並在響應頭中包含新資源的 URI
            return CreatedAtAction(nameof(GetOrderForm), new { id = orderForm.OrderId }, dto);
        }

        // PUT: api/OrderForm/{id}
        // 更新現有的訂單
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderForm(int id, OrderFormDto dto)
        {
            if (id != dto.OrderId)
            {
                return BadRequest(); // 如果 URL 中的 ID 與 DTO 中的 ID 不匹配，返回 400 Bad Request
            }

            var orderForm = await _context.OrderForms.FindAsync(id);
            if (orderForm == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 更新模型的屬性
            orderForm.MemberId = dto.MemberId; // 可以更新外鍵，但要確保對應的 Member 存在
            orderForm.DocumentMenuId = dto.DocumentMenuId; // 可以更新外鍵，但要確保對應的 DocumentMenu 存在
            orderForm.DepartureDate = dto.DepartureDate;
            orderForm.ProcessingQuantity = dto.ProcessingQuantity;
            orderForm.ChineseSurname = dto.ChineseSurname;
            orderForm.ChineseName = dto.ChineseName;
            orderForm.EnglishSurname = dto.EnglishSurname;
            orderForm.EnglishName = dto.EnglishName;
            orderForm.Gender = dto.Gender;
            orderForm.BirthDate = dto.BirthDate;
            orderForm.ContactPersonName = dto.ContactPersonName;
            orderForm.ContactPersonEmail = dto.ContactPersonEmail;
            orderForm.ContactPersonPhoneNumber = dto.ContactPersonPhoneNumber;
            orderForm.PickupMethodOption = dto.PickupMethodOption;
            orderForm.MailingCity = dto.MailingCity;
            orderForm.MailingDetailAddress = dto.MailingDetailAddress;
            orderForm.StoreDetailAddress = dto.StoreDetailAddress;
            orderForm.TaxIdOption = dto.TaxIdOption;
            orderForm.CompanyName = dto.CompanyName;
            orderForm.TaxIdNumber = dto.TaxIdNumber;
            // OrderCreationTime 通常不允許修改，或者由後端自動處理

            try
            {
                await _context.SaveChangesAsync(); // 保存更改
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderFormExists(id))
                {
                    return NotFound(); // 如果在保存時發現並發衝突，但實體已不存在，則返回 404
                }
                else
                {
                    throw; // 否則重新拋出異常
                }
            }

            return NoContent(); // 更新成功，返回 204 No Content
        }

        // DELETE: api/OrderForm/{id}
        // 刪除訂單
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderForm(int id)
        {
            var orderForm = await _context.OrderForms.FindAsync(id);
            if (orderForm == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            _context.OrderForms.Remove(orderForm);
            await _context.SaveChangesAsync(); // 保存更改

            return NoContent(); // 刪除成功，返回 204 No Content
        }

        // 輔助方法：檢查訂單是否存在
        private bool OrderFormExists(int id)
        {
            return _context.OrderForms.Any(e => e.OrderId == id);
        }
    }
}