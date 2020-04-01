using AutoMapper;
using ExpensesAPI.Domain.Models;
using ExpensesAPI.SimpleJWTAuth.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpensesAPI.SimpleJWTAuth.Mapping
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<RegistrationResource, User>()
                .ForMember(u => u.UserName, map => map.MapFrom(s => s.Email));
        }
    }
}
