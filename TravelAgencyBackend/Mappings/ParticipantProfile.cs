using AutoMapper;
using TravelAgency.Shared.Models;
using TravelAgencyBackend.ViewModels;

namespace TravelAgencyBackend.Mappings
{
    public class ParticipantProfile : Profile
    {
        public ParticipantProfile()
        {

            CreateMap<MemberFavoriteTraveler, ParticipantEditViewModel>().ReverseMap();
            CreateMap<MemberFavoriteTraveler, ParticipantCreateViewModel>().ReverseMap();

            // 詳細資料：Entity ➜ DetailViewModel
            CreateMap<MemberFavoriteTraveler, ParticipantDetailViewModel>()
                .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name));

            // 列表用：Entity ➜ ListItemViewModel
            CreateMap<MemberFavoriteTraveler, ParticipantListItemViewModel>()
                .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name))
                .ForMember(dest => dest.MemberAccount, opt => opt.MapFrom
                (src => !string.IsNullOrEmpty(src.Member.Email) ? src.Member.Email : src.Member.Phone));
        }
    }
}
