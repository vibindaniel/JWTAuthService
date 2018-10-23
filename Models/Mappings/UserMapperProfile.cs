using AutoMapper;
using DuploAuth.Models.Entities;
using DuploAuth.Models.ViewModels;

namespace DuploAuth.Models.Mappings
{
    public class UserMapperProfile : Profile
    {
        public UserMapperProfile()
        {
            CreateMap<UserViewModel, AppUser>().ForMember(au => au.UserName, map => map.MapFrom(vm => vm.Email));
        }
    }
}