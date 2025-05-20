using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using TravelAgency.Shared.Data;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.Services;
using TravelAgencyBackend.ViewModels;

namespace TravelAgencyBackend.Controllers
{
    public class ChatRoomController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly PermissionCheckService _perm;

        public ChatRoomController(AppDbContext context, IMapper mapper, PermissionCheckService perm)
            : base(perm)
        {
            _context = context;
            _mapper = mapper;
            _perm = perm;
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

        // 關閉聊天室
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Close(int id) 
        {
            var check = CheckPermissionOrForbid("管理聊天室");
            if (check != null) return check;

            var chatRoom = _context.ChatRooms
                .FirstOrDefault(c => c.ChatRoomId == id);
            if (chatRoom == null) return NotFound("聊天室已不存在");

            chatRoom.Status = ChatStatus.Closed;
            _context.SaveChanges();

            return RedirectToAction("Details", new { id });
        }

        // 發送訊息
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage(SendMessageViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理聊天室");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                TempData["SendError"] = "請輸入訊息內容（最多500字）";
                return RedirectToAction("Details", new { id = vm.ChatRoomId });
            }

            int employeeId = 1;

            var chatRoom = _context.ChatRooms
                .FirstOrDefault(c => c.ChatRoomId == vm.ChatRoomId && c.EmployeeId == employeeId);
            if (chatRoom == null) return NotFound("聊天室不存在");

            var message = new TravelAgency.Shared.Models.Message
            {
                ChatRoomId = vm.ChatRoomId,
                SenderType = SenderType.Employee,
                SenderId = employeeId,
                Content = vm.Content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = vm.ChatRoomId });
        }

        //聊天室列表
        public IActionResult Index()
        {
            var check = CheckPermissionOrForbid("管理聊天室");
            if (check != null) return check;

            // TODO: 目前登入員工 ID
            int emoployeeId = GetCurrentEmployeeId();
            //int.Parse(User.FindFirst("EmployeeId").Value);
            var chatRooms = _context.ChatRooms
                .Where(c => c.EmployeeId == emoployeeId)
                .Include(c => c.Member)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var vmList = _mapper.Map<List<ChatRoomViewModel>>(chatRooms);
            return View(vmList);
        }

        //查看聊天室
        public IActionResult Details(int id)
        {
            var check = CheckPermissionOrForbid("管理聊天室");
            if (check != null) return check;

            var chatRoom = _context.ChatRooms
                .Include(c => c.Member)
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.ChatRoomId == id);

            if (chatRoom == null) return NotFound("聊天室已不存在");

            // 將未讀訊息設為已讀
            var unread = chatRoom.Messages
                .Where(m => m.SenderType == SenderType.Member && !m.IsRead)
                .ToList();

            if (unread.Any())
            {
                foreach (var msg in unread)
                {
                    msg.IsRead = true;
                }
                _context.SaveChanges();
            }

            var vm = _mapper.Map<ChatRoomDetailViewModel>(chatRoom);
            vm.Messages = _mapper.Map<List<ChatMessageViewModel>>(chatRoom.Messages.OrderBy(m => m.SentAt).ToList());

            ViewBag.EmployeeId = GetCurrentEmployeeId();
            return View(vm);
        }


        // 建立聊天室
        public IActionResult Create()
        {
            var check = CheckPermissionOrForbid("管理聊天室");
            if (check != null) return check;

            var members = _context.Members
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem
                {
                    Value = m.MemberId.ToString(),
                    Text = m.Name
                })
                .ToList();

            var vm = new ChatRoomCreateViewModel
            {
                MemberList = members
            };

            return View(vm);
        }


        // 建立聊天室
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ChatRoomCreateViewModel vm)
        {
            var check = CheckPermissionOrForbid("管理聊天室");
            if (check != null) return check;

            if (!ModelState.IsValid)
            {
                // 重新填充會員選單
                vm.MemberList = _context.Members
                    .OrderBy(m => m.Name)
                    .Select(m => new SelectListItem
                    {
                        Value = m.MemberId.ToString(),
                        Text = m.Name
                    })
                    .ToList();

                return View(vm);
            }

            int employeeId = GetCurrentEmployeeId(); // TODO: 從登入取得

            var existingChat = _context.ChatRooms
                .FirstOrDefault(c => c.EmployeeId == employeeId && c.MemberId == vm.MemberId);

            if (existingChat != null)
            {
                return RedirectToAction("Details", new { id = existingChat.ChatRoomId });
            }

            var newChat = new ChatRoom
            {
                EmployeeId = employeeId,
                MemberId = vm.MemberId,
                CreatedAt = DateTime.Now,
                Status = ChatStatus.Opened
            };

            _context.ChatRooms.Add(newChat);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = newChat.ChatRoomId });
        }


        // GET: ChatRoomController/Edit/5
        public IActionResult Edit(int id)
        {
            return View();
        }

        // POST: ChatRoomController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, IFormCollection collection)
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

        // GET: ChatRoomController/Delete/5
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: ChatRoomController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, IFormCollection collection)
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
