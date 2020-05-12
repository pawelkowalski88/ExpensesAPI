﻿using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ExpensesAPI.IdentityProvider.Data;
using ExpensesAPI.IdentityProvider.Repositories;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpensesAPI.IdentityProvider.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public AccountController(ApplicationDbContext context, IUserRepository userRepository, IMapper mapper)
        {
            this.context = context;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserNames(string query)
        {
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var users = await userRepository.GetUserListAsync(query, sub);

            var results = users.Select(u => {
                var userResource = mapper.Map<UserResource>(u);
                //userResource.Selected = u.ScopeUsers.Any(su => su.ScopeId == u.SelectedScopeId);
                return userResource;
            });

            return Ok(results);
        }

    }
}