using AutoMapper;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.ViewModels;

public class ChatRoomProfile : Profile
{
    public ChatRoomProfile()
    {
        CreateMap<ChatRoom, ChatRoomViewModel>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.UnreadCount, opt => opt.MapFrom(src =>
                src.Messages.Count(m => m.SenderType == SenderType.Member && !m.IsRead)
            ));

        CreateMap<ChatRoom, ChatRoomDetailViewModel>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name));

        CreateMap<Message, ChatMessageViewModel>()
            .ForMember(dest => dest.SenderType, opt => opt.MapFrom(src => src.SenderType.ToString()))
            .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt.ToString("yyyy-MM-dd HH:mm")));

        CreateMap<SendMessageViewModel, Message>(); 
    }
}

