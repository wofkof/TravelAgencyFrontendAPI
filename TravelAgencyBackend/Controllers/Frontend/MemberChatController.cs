using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;

namespace TravelAgencyBackend.Controllers.Frontend
{
    public class MemberChatController : Controller
    {
        private readonly AppDbContext _context;

        public MemberChatController(AppDbContext context) 
        {
            _context = context;
        }

        //取訊息Json
        [HttpGet]
        public IActionResult GetMessages(int chatRoomId)
        {
            var messages = _context.Messages
                .Where(m => m.ChatRoomId == chatRoomId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    sender = m.SenderType.ToString(),
                    content = m.Content,
                    sentAt = m.SentAt.ToString("yyyy-MM-dd HH:mm"),
                    isRead = m.IsRead,
                })
                .ToList();

            return Json(messages);
        }

        //發送訊息
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(int chatRoomId, string content) 
        {
            int memberId = 4;

            var chatRoom = _context.ChatRooms
                .FirstOrDefault(c => c.ChatRoomId == chatRoomId && c.MemberId == memberId);
                
            if (chatRoom == null) return NotFound("聊天室已不存在");

            var message = new Message
            {
                ChatRoomId = chatRoomId,
                SenderType = SenderType.Member,
                SenderId = memberId,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 聊天室列表
        public IActionResult Index()
        {
            int memberId = 4;

            var chatRoom = _context.ChatRooms
                .Include(c => c.Employee)
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.MemberId == memberId);

            if (chatRoom == null) return NotFound("沒有與員工的聊天室");

            // 設為已讀
            var unreadMessage = chatRoom.Messages
                .Where(m => m.SenderType == SenderType.Employee && !m.IsRead)
                .ToList();

            foreach (var msg in unreadMessage)
            {
                msg.IsRead = true;
            }
            _context.SaveChanges();

            return View(chatRoom);
        }

        // GET: MemberChatController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MemberChatController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MemberChatController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MemberChatController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MemberChatController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MemberChatController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MemberChatController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
