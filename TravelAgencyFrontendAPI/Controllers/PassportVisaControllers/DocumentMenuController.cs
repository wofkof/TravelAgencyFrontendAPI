using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data; // 引入 AppDbContext
using TravelAgency.Shared.Models; // 引入 DocumentMenu 模型
using TravelAgencyFrontendAPI.DTOs.PassportVisaDTOs; // 引入 DocumentMenuDto

namespace TravelAgencyFrontendAPI.Controllers.PassportVisaControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentMenuController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentMenuController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DocumentMenu
        // 獲取所有文件菜單項目
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentMenuDto>>> GetDocumentMenus()
        {
            var documentMenus = await _context.DocumentMenus
                .Select(dm => new DocumentMenuDto
                {
                    MenuId = dm.MenuId,
                    RocPassportOption = dm.RocPassportOption,
                    ForeignVisaOption = dm.ForeignVisaOption,
                    ApplicationType = dm.ApplicationType,
                    ProcessingItem = dm.ProcessingItem,
                    CaseType = dm.CaseType,
                    ProcessingDays = dm.ProcessingDays,
                    DocumentValidityPeriod = dm.DocumentValidityPeriod,
                    StayDuration = dm.StayDuration,
                    Fee = dm.Fee
                })
                .ToListAsync();

            return Ok(documentMenus);
        }

        // GET: api/DocumentMenu/{id}
        // 根據ID獲取單個文件菜單項目
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentMenuDto>> GetDocumentMenu(int id)
        {
            var documentMenu = await _context.DocumentMenus.FindAsync(id);

            if (documentMenu == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 將模型映射到 DTO
            var documentMenuDto = new DocumentMenuDto
            {
                MenuId = documentMenu.MenuId,
                RocPassportOption = documentMenu.RocPassportOption,
                ForeignVisaOption = documentMenu.ForeignVisaOption,
                ApplicationType = documentMenu.ApplicationType,
                ProcessingItem = documentMenu.ProcessingItem,
                CaseType = documentMenu.CaseType,
                ProcessingDays = documentMenu.ProcessingDays,
                DocumentValidityPeriod = documentMenu.DocumentValidityPeriod,
                StayDuration = documentMenu.StayDuration,
                Fee = documentMenu.Fee
            };

            return Ok(documentMenuDto);
        }

        // POST: api/DocumentMenu
        // 創建新的文件菜單項目
        [HttpPost]
        public async Task<ActionResult<DocumentMenuDto>> CreateDocumentMenu(DocumentMenuDto dto)
        {
            // 將 DTO 映射回模型
            var documentMenu = new DocumentMenu
            {
                RocPassportOption = dto.RocPassportOption,
                ForeignVisaOption = dto.ForeignVisaOption,
                ApplicationType = dto.ApplicationType,
                ProcessingItem = dto.ProcessingItem,
                CaseType = dto.CaseType,
                ProcessingDays = dto.ProcessingDays,
                DocumentValidityPeriod = dto.DocumentValidityPeriod,
                StayDuration = dto.StayDuration,
                Fee = dto.Fee
            };

            _context.DocumentMenus.Add(documentMenu);
            await _context.SaveChangesAsync(); // 保存到資料庫，此時 MenuId 會被填充

            // 將新創建的模型數據（包括生成的 MenuId）更新回 DTO
            dto.MenuId = documentMenu.MenuId;

            // 返回 201 Created 狀態碼，並在響應頭中包含新資源的 URI
            return CreatedAtAction(nameof(GetDocumentMenu), new { id = documentMenu.MenuId }, dto);
        }

        // PUT: api/DocumentMenu/{id}
        // 更新現有的文件菜單項目
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocumentMenu(int id, DocumentMenuDto dto)
        {
            if (id != dto.MenuId)
            {
                return BadRequest(); // 如果 URL 中的 ID 與 DTO 中的 ID 不匹配，返回 400 Bad Request
            }

            var documentMenu = await _context.DocumentMenus.FindAsync(id);
            if (documentMenu == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            // 更新模型的屬性
            documentMenu.RocPassportOption = dto.RocPassportOption;
            documentMenu.ForeignVisaOption = dto.ForeignVisaOption;
            documentMenu.ApplicationType = dto.ApplicationType;
            documentMenu.ProcessingItem = dto.ProcessingItem;
            documentMenu.CaseType = dto.CaseType;
            documentMenu.ProcessingDays = dto.ProcessingDays;
            documentMenu.DocumentValidityPeriod = dto.DocumentValidityPeriod;
            documentMenu.StayDuration = dto.StayDuration;
            documentMenu.Fee = dto.Fee;

            try
            {
                await _context.SaveChangesAsync(); // 保存更改
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentMenuExists(id))
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

        // DELETE: api/DocumentMenu/{id}
        // 刪除文件菜單項目
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumentMenu(int id)
        {
            var documentMenu = await _context.DocumentMenus.FindAsync(id);
            if (documentMenu == null)
            {
                return NotFound(); // 如果找不到，返回 404 Not Found
            }

            _context.DocumentMenus.Remove(documentMenu);
            await _context.SaveChangesAsync(); // 保存更改

            return NoContent(); // 刪除成功，返回 204 No Content
        }

        // 輔助方法：檢查文件菜單是否存在
        private bool DocumentMenuExists(int id)
        {
            return _context.DocumentMenus.Any(e => e.MenuId == id);
        }
    }
}