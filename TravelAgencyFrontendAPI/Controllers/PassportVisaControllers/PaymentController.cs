using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data; // 引入 AppDbContext
using TravelAgency.Shared.Models; // 引入 Payment, OrderForm, DocumentMenu 模型
using TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs; // 引入 PaymentDTO

namespace TravelAgencyFrontendAPI.Controllers.PassportVisaControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Payment
        // 獲取所有付款記錄
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.OrderForm) // 包含相關的 OrderForm 數據
                .Include(p => p.DocumentMenu) // 包含相關的 DocumentMenu 數據
                .Select(p => new PaymentDTO
                {
                    PaymentId = p.PaymentId,
                    OrderFormId = p.OrderFormId,
                    OrderForm = p.OrderForm, // 直接包含 OrderForm 模型，請參考下面的注意事項
                    DocumentMenuId = p.DocumentMenuId,
                    DocumentMenu = p.DocumentMenu, // 直接包含 DocumentMenu 模型，請參考下面的注意事項
                    PaymentMethod = p.PaymentMethod
                })
                .ToListAsync();

            return Ok(payments);
        }

        // GET: api/Payment/{id}
        // 根據ID獲取單個付款記錄
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDTO>> GetPayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.OrderForm)
                .Include(p => p.DocumentMenu)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 將模型映射到 DTO
            var paymentDto = new PaymentDTO
            {
                PaymentId = payment.PaymentId,
                OrderFormId = payment.OrderFormId,
                OrderForm = payment.OrderForm,
                DocumentMenuId = payment.DocumentMenuId,
                DocumentMenu = payment.DocumentMenu,
                PaymentMethod = payment.PaymentMethod
            };

            return Ok(paymentDto);
        }

        // POST: api/Payment
        // 創建新的付款記錄
        [HttpPost]
        public async Task<ActionResult<PaymentDTO>> CreatePayment(PaymentDTO dto)
        {
            // 在創建付款記錄之前，驗證 OrderFormId 和 DocumentMenuId 是否有效
            var orderFormExists = await _context.OrderForms.AnyAsync(of => of.OrderId == dto.OrderFormId);
            if (!orderFormExists)
            {
                return BadRequest($"OrderForm with ID {dto.OrderFormId} does not exist.");
            }

            var documentMenuExists = await _context.DocumentMenus.AnyAsync(dm => dm.MenuId == dto.DocumentMenuId);
            if (!documentMenuExists)
            {
                return BadRequest($"DocumentMenu with ID {dto.DocumentMenuId} does not exist.");
            }

            // 將 DTO 映射回模型
            var payment = new Payment
            {
                OrderFormId = dto.OrderFormId,
                DocumentMenuId = dto.DocumentMenuId,
                PaymentMethod = dto.PaymentMethod
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(); // 保存到資料庫，此時 PaymentId 會被填充

            // 重新加載相關的 OrderForm 和 DocumentMenu 數據，以便在返回的 DTO 中包含它們
            await _context.Entry(payment).Reference(p => p.OrderForm).LoadAsync();
            await _context.Entry(payment).Reference(p => p.DocumentMenu).LoadAsync();

            // 將新創建的模型數據（包括生成的 PaymentId）更新回 DTO
            dto.PaymentId = payment.PaymentId;
            // 更新 DTO 中的導航屬性
            dto.OrderForm = payment.OrderForm;
            dto.DocumentMenu = payment.DocumentMenu;

            // 返回 201 Created 狀態碼，並在響應頭中包含新資源的 URI
            return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, dto);
        }

        // PUT: api/Payment/{id}
        // 更新現有的付款記錄
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, PaymentDTO dto)
        {
            if (id != dto.PaymentId)
            {
                return BadRequest(); // 如果 URL 中的 ID 與 DTO 中的 ID 不匹配，返回 400 Bad Request
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 在更新付款記錄之前，驗證 OrderFormId 和 DocumentMenuId 是否有效
            if (payment.OrderFormId != dto.OrderFormId) // 如果 OrderFormId 改變了，需要驗證
            {
                var orderFormExists = await _context.OrderForms.AnyAsync(of => of.OrderId == dto.OrderFormId);
                if (!orderFormExists)
                {
                    return BadRequest($"OrderForm with ID {dto.OrderFormId} does not exist.");
                }
            }

            if (payment.DocumentMenuId != dto.DocumentMenuId) // 如果 DocumentMenuId 改變了，需要驗證
            {
                var documentMenuExists = await _context.DocumentMenus.AnyAsync(dm => dm.MenuId == dto.DocumentMenuId);
                if (!documentMenuExists)
                {
                    return BadRequest($"DocumentMenu with ID {dto.DocumentMenuId} does not exist.");
                }
            }


            // 更新模型的屬性
            payment.OrderFormId = dto.OrderFormId;
            payment.DocumentMenuId = dto.DocumentMenuId;
            payment.PaymentMethod = dto.PaymentMethod;

            try
            {
                await _context.SaveChangesAsync(); // 保存更改
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(id))
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

        // DELETE: api/Payment/{id}
        // 刪除付款記錄
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync(); // 保存更改

            return NoContent(); // 刪除成功，返回 204 No Content
        }

        // 輔助方法：檢查付款記錄是否存在
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}
