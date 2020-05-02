using AutoMapper;
using ExpensesAPI.IdentityProvider.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.IdentityProvider.Controllers
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
