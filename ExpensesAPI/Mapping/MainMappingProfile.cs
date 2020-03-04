using AutoMapper;
using ExpensesAPI.Models;
using ExpensesAPI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Mapping
{
    public class MainMappingProfile : Profile
    {
        public MainMappingProfile()
        {
            //Domain to API
            CreateMap<User, UserResource>();
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
            CreateMap<ExpenseResourceBase, Expense>()
                .ForMember(e => e.Date,
                    opt => opt.MapFrom(er => DateTime.Parse(er.Date)));
            CreateMap<ScopeResource, Scope>();
        }
    }
}
