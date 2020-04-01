using AutoMapper;
using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain;
using ExpensesAPI.Domain.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExpensesAPI.Domain.Persistence;

namespace ExpensesAPI.Controllers
{
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly IScopeRepository scopeRepository;
        private readonly IHostingEnvironment host;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserRepository userRepository;

        public UserController(IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            IScopeRepository scopeRepository,
            IUnitOfWork unitOfWork)
        {
            this.userRepository = userRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.scopeRepository = scopeRepository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("api/user/details")]
        public async Task<IActionResult> GetUserDataAsync()
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");
            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");

            var user = await userRepository.GetUserAsync(claim.Value);
            return Ok(mapper.Map<UserResource>(user));
        }

        [HttpGet("api/user/list/{scopeId}")]
        public async Task<IActionResult> GetUserList(string query)
        {
            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");

            var users = await userRepository.GetUserListAsync(query, claim.Value);

            var results = users.Select(u => {
                var userResource = mapper.Map<UserResource>(u);
                userResource.Selected = u.ScopeUsers.Any(su => su.ScopeId == u.SelectedScopeId);
                return userResource;
            });

            return Ok(results);
        }

        [HttpGet("api/user/picture")]
        public async Task<IActionResult> GetUserPicture()
        {
            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserAsync(claim.Value);

            if (user.PictureUrl != null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(),
                            "PictureFiles", user.PictureUrl);
                return PhysicalFile(file, "image/jpeg");
            }
            return BadRequest("Dany użytkownik nie posiada przypisanego obrazu.");
        }

        [HttpPost("api/user/picture")]
        public async Task<IActionResult> SaveUserPicture(IFormFile file)
        {
            try
            {
                var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
                var user = await userRepository.GetUserAsync(claim.Value);

                var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PictureFiles");
                if (!Directory.Exists(uploadsFolderPath))
                    Directory.CreateDirectory(uploadsFolderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var oldPictureName = user.PictureUrl;
                user.PictureUrl = fileName;
                unitOfWork.CompleteAsync();

                if (oldPictureName != null)
                    System.IO.File.Delete(Path.Combine(uploadsFolderPath, oldPictureName));

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpGet("api/user/selectedscope")]
        public async Task<IActionResult> GetSelectedScope()
        {
            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserAsync(claim.Value);

            return Ok(mapper.Map<ScopeResource>(user.SelectedScope));
        }

        [HttpPost("api/user/selectedscope")]
        public async Task<IActionResult> SetSelectedScope([FromBody] ScopeId scopeId)
        {
            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserAsync(claim.Value);

            user.SelectedScope = await scopeRepository.GetScope(scopeId.Id);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpPut("api/user")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserResource user)
        {
            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            //var user = await mainDbContext.Users.SingleAsync(u => u.Id == claim.Value);
            var userCurrent = await userRepository.GetUserAsync(claim.Value);

            userCurrent.FirstName = user.FirstName;
            userCurrent.LastName = user.LastName;
            userCurrent.Email = user.Email;

            await unitOfWork.CompleteAsync();
            return Ok(await userRepository.GetUserAsync(claim.Value));
        }
    }

    public class ScopeId
    {
        public int Id { get; set; }
    }
}
