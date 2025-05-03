using Microsoft.AspNetCore.SignalR;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Models;

namespace TravelAgencyFrontendAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context) 
        {
            _context = context;
        }

        public async Task SendMessage(MessageDto dto) 
        {
            var message = new Message
            {
                ChatRoomId = dto.ChatRoomId,
                SenderType = Enum.Parse<SenderType>(dto.SenderType, true),
                SenderId = dto.SenderId,
                MessageType = Enum.Parse<MessageType>(dto.MessageType, true),
                Content = dto.Content,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.Group(dto.ChatRoomId.ToString()).SendAsync(
                "ReceiveMessage",
                new MessageDto
                {
                    MessageId = message.MessageId,
                    ChatRoomId = message.ChatRoomId,
                    SenderType = message.SenderType.ToString(),
                    SenderId = message.SenderId,
                    MessageType = message.MessageType.ToString(),
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead
                });
        }

        public async Task JoinGroup(string chatRoomId) 
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
        }

        public async Task LeaveGroup(string chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
        }
    }
}
