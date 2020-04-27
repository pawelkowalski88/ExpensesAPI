using AutoMapper;
using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Mapping
{
    public class MainMappingProfile : Profile
    {
        public MainMappingProfile()
        {
            //Domain to API
            CreateMap<User, UserResource>()
                .ForMember(ur => ur.Email,
                    opt => opt.MapFrom(u => u.UserName));
            CreateMap<Category, CategoryResource>();
            CreateMap<Expense, ExpenseResourceBase>()
                .ForMember(er => er.Date,
                    opt => opt.MapFrom(e => e.Date.ToString("yyyy-MM-dd")));

            CreateMap<Expense, ExpenseResource>()
                .ForMember(er => er.Date,
                    opt => opt.MapFrom(e => e.Date.ToString("yyyy-MM-dd")));

            CreateMap<Scope, ScopeResource>();
            CreateMap<ScopeUser, ScopeUserResource>();


            //API to Domain
            CreateMap<SaveCategoryResource, Category>();
            CreateMap<CategoryResource, Category>();
            CreateMap<ExpenseResourceBase, Expense>()
                .ForMember(e => e.Date,
                    opt => opt.MapFrom(er => DateTime.Parse(er.Date)));
            CreateMap<ScopeResource, Scope>();
            CreateMap<UserResource, User>()
                .ForMember(u => u.UserName,
                    opt => opt.MapFrom(ur => ur.Email));
        }
    }
}
