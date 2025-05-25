using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data; // 引入 AppDbContext
using TravelAgency.Shared.Models; // 引入 CompletedOrderDetail, DocumentMenu, OrderForm 模型
using TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs; // 引入 CompletedOrderDetailDto

namespace TravelAgencyFrontendAPI.Controllers.PassportVisaControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompletedOrderDetailController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompletedOrderDetailController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/CompletedOrderDetail
        // 獲取所有已完成訂單詳細信息
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompletedOrderDetailDto>>> GetCompletedOrderDetails()
        {
            var completedOrderDetails = await _context.CompletedOrderDetails
                .Include(cod => cod.DocumentMenu) // 包含相關的 DocumentMenu 數據
                .Include(cod => cod.OrderForm)    // 包含相關的 OrderForm 數據
                .Select(cod => new CompletedOrderDetailDto
                {
                    CompletedOrderDetailId = cod.CompletedOrderDetailId,
                    DocumentMenuId = cod.DocumentMenuId,
                    DocumentMenu = cod.DocumentMenu, // 直接包含 DocumentMenu 模型，請參考下面的注意事項
                    OrderFormId = cod.OrderFormId,
                    OrderForm = cod.OrderForm // 直接包含 OrderForm 模型，請參考下面的注意事項
                })
                .ToListAsync();

            return Ok(completedOrderDetails);
        }

        // GET: api/CompletedOrderDetail/{id}
        // 根據ID獲取單個已完成訂單詳細信息
        [HttpGet("{id}")]
        public async Task<ActionResult<CompletedOrderDetailDto>> GetCompletedOrderDetail(int id)
        {
            var completedOrderDetail = await _context.CompletedOrderDetails
                .Include(cod => cod.DocumentMenu)
                .Include(cod => cod.OrderForm)
                .FirstOrDefaultAsync(cod => cod.CompletedOrderDetailId == id);

            if (completedOrderDetail == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 將模型映射到 DTO
            var completedOrderDetailDto = new CompletedOrderDetailDto
            {
                CompletedOrderDetailId = completedOrderDetail.CompletedOrderDetailId,
                DocumentMenuId = completedOrderDetail.DocumentMenuId,
                DocumentMenu = completedOrderDetail.DocumentMenu,
                OrderFormId = completedOrderDetail.OrderFormId,
                OrderForm = completedOrderDetail.OrderForm
            };

            return Ok(completedOrderDetailDto);
        }

        // POST: api/CompletedOrderDetail
        // 創建新的已完成訂單詳細信息
        [HttpPost]
        public async Task<ActionResult<CompletedOrderDetailDto>> CreateCompletedOrderDetail(CompletedOrderDetailDto dto)
        {
            // 在創建前驗證 DocumentMenuId 和 OrderFormId 是否有效
            var documentMenuExists = await _context.DocumentMenus.AnyAsync(dm => dm.MenuId == dto.DocumentMenuId);
            if (!documentMenuExists)
            {
                return BadRequest($"DocumentMenu with ID {dto.DocumentMenuId} does not exist.");
            }

            var orderFormExists = await _context.OrderForms.AnyAsync(of => of.OrderId == dto.OrderFormId);
            if (!orderFormExists)
            {
                return BadRequest($"OrderForm with ID {dto.OrderFormId} does not exist.");
            }

            // 將 DTO 映射回模型
            var completedOrderDetail = new CompletedOrderDetail
            {
                DocumentMenuId = dto.DocumentMenuId,
                OrderFormId = dto.OrderFormId
            };

            _context.CompletedOrderDetails.Add(completedOrderDetail);
            await _context.SaveChangesAsync(); // 保存到資料庫，此時 CompletedOrderDetailId 會被填充

            // 重新加載相關的 DocumentMenu 和 OrderForm 數據，以便在返回的 DTO 中包含它們
            await _context.Entry(completedOrderDetail).Reference(cod => cod.DocumentMenu).LoadAsync();
            await _context.Entry(completedOrderDetail).Reference(cod => cod.OrderForm).LoadAsync();

            // 將新創建的模型數據（包括生成的 ID）更新回 DTO
            dto.CompletedOrderDetailId = completedOrderDetail.CompletedOrderDetailId;
            // 更新 DTO 中的導航屬性
            dto.DocumentMenu = completedOrderDetail.DocumentMenu;
            dto.OrderForm = completedOrderDetail.OrderForm;

            // 返回 201 Created 狀態碼，並在響應頭中包含新資源的 URI
            return CreatedAtAction(nameof(GetCompletedOrderDetail), new { id = completedOrderDetail.CompletedOrderDetailId }, dto);
        }

        // PUT: api/CompletedOrderDetail/{id}
        // 更新現有的已完成訂單詳細信息
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompletedOrderDetail(int id, CompletedOrderDetailDto dto)
        {
            if (id != dto.CompletedOrderDetailId)
            {
                return BadRequest(); // 如果 URL 中的 ID 與 DTO 中的 ID 不匹配，返回 400 Bad Request
            }

            var completedOrderDetail = await _context.CompletedOrderDetails.FindAsync(id);
            if (completedOrderDetail == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 在更新前驗證 DocumentMenuId 和 OrderFormId 是否有效 (如果它們被修改了)
            if (completedOrderDetail.DocumentMenuId != dto.DocumentMenuId)
            {
                var documentMenuExists = await _context.DocumentMenus.AnyAsync(dm => dm.MenuId == dto.DocumentMenuId);
                if (!documentMenuExists)
                {
                    return BadRequest($"DocumentMenu with ID {dto.DocumentMenuId} does not exist.");
                }
            }

            if (completedOrderDetail.OrderFormId != dto.OrderFormId)
            {
                var orderFormExists = await _context.OrderForms.AnyAsync(of => of.OrderId == dto.OrderFormId);
                if (!orderFormExists)
                {
                    return BadRequest($"OrderForm with ID {dto.OrderFormId} does not exist.");
                }
            }


            // 更新模型的屬性
            completedOrderDetail.DocumentMenuId = dto.DocumentMenuId;
            completedOrderDetail.OrderFormId = dto.OrderFormId;

            try
            {
                await _context.SaveChangesAsync(); // 保存更改
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompletedOrderDetailExists(id))
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

        // DELETE: api/CompletedOrderDetail/{id}
        // 刪除已完成訂單詳細信息
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompletedOrderDetail(int id)
        {
            var completedOrderDetail = await _context.CompletedOrderDetails.FindAsync(id);
            if (completedOrderDetail == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            _context.CompletedOrderDetails.Remove(completedOrderDetail);
            await _context.SaveChangesAsync(); // 保存更改

            return NoContent(); // 刪除成功，返回 204 No Content
        }

        // 輔助方法：檢查已完成訂單詳細信息是否存在
        private bool CompletedOrderDetailExists(int id)
        {
            return _context.CompletedOrderDetails.Any(e => e.CompletedOrderDetailId == id);
        }
    }
}