using AutoMapper;
using ExpensesAPI.IdentityProvider.API.Models;
using ExpensesAPI.IdentityProvider.API.Resources;

namespace ExpensesAPI.IdentityProvider.API.Controllers
{ 
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResource>()
                    .ForMember(ur => ur.Email,
                        opt => opt.MapFrom(u => u.UserName));

            CreateMap<UserResource, User>()
                .ForMember(u => u.UserName,
                    opt => opt.MapFrom(ur => ur.Email));
        }
    }
}
