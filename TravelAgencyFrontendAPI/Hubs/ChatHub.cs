using Microsoft.AspNetCore.SignalR;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.DTOs.ChatRoomDTOs;
using TravelAgencyFrontendAPI.Models;
using System.Collections.Concurrent;

namespace TravelAgencyFrontendAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public static class ConnectedUsers
        {
            public static ConcurrentDictionary<string, string> UserToConnectionMap = new();
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

            await Clients.Group(dto.ChatRoomId.ToString()).SendAsync("ReceiveMessage", new MessageDto
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

        public async Task NotifyRead(int chatRoomId, int readerId, string readerType)
        {
            await Clients.Group(chatRoomId.ToString())
                         .SendAsync("MessageRead", chatRoomId, readerId, readerType);
        }

        public async Task SendCallOffer(string toConnectionId, object offer)
        {
            await Clients.Client(toConnectionId)
                         .SendAsync("ReceiveCallOffer", Context.ConnectionId, offer);
        }

        public async Task SendCallAnswer(string toConnectionId, object answer)
        {
            await Clients.Client(toConnectionId)
                         .SendAsync("ReceiveCallAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIceCandidate(string toConnectionId, object candidate)
        {
            await Clients.Client(toConnectionId)
                         .SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        public Task<string?> GetConnectionId(string userType, int userId)
        {
            var key = $"{userType}:{userId}";
            ConnectedUsers.UserToConnectionMap.TryGetValue(key, out var connId);
            return Task.FromResult(connId);
        }

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Request.Query["userId"];
            var userType = httpContext?.Request.Query["userType"];
            Console.WriteLine($"[Hub] {userType}:{userId} 已連線 {Context.ConnectionId}");
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(userType))
            {
                var key = $"{userType}:{userId}";
                ConnectedUsers.UserToConnectionMap[key] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var kv = ConnectedUsers.UserToConnectionMap
                .FirstOrDefault(x => x.Value == Context.ConnectionId);

            if (!string.IsNullOrEmpty(kv.Key))
            {
                ConnectedUsers.UserToConnectionMap.TryRemove(kv.Key, out _);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task EndCall(string toConnectionId)
        {
            Console.WriteLine($"[Hub] 來自 {Context.ConnectionId} 通知掛斷給 {toConnectionId}");
            await Clients.Client(toConnectionId).SendAsync("ReceiveEndCall", Context.ConnectionId);
        }

        public async Task RejectCall(string toConnectionId)
        {
            await Clients.Client(toConnectionId).SendAsync("CallRejected", Context.ConnectionId);
        }

    }
}
