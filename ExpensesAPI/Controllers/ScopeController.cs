using AutoMapper;
using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain;
using ExpensesAPI.Domain.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpensesAPI.Domain.Persistence;
using System.Security.Claims;
using IdentityModel;
using ExpensesAPI.Domain.ExternalAPIUtils;
using Microsoft.Net.Http.Headers;

namespace ExpensesAPI.Controllers
{
    [Authorize]
    public class ScopeController : ControllerBase
    {
        private readonly IScopeRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserRepository<User> userRepository;
        private readonly IUserRepository<IdentityServerUser> idsUserRepository;
        private readonly ITokenRepository tokenRepository;
        private readonly IMapper mapper;

        public ScopeController(
            IScopeRepository repository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository<User> userRepository,
            IUserRepository<IdentityServerUser> idsUserRepository,
            ITokenRepository tokenRepository,
            IMapper mapper)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.httpContextAccessor = httpContextAccessor;
            this.userRepository = userRepository;
            this.idsUserRepository = idsUserRepository;
            this.tokenRepository = tokenRepository;
            this.mapper = mapper;
        }
        [HttpGet("api/scopes")]
        public async Task<IActionResult> GetScopes()
        {
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            tokenRepository.SetToken(Request.Headers[HeaderNames.Authorization]);

            var scopes = await repository.GetScopes(user);
            var results = mapper.Map<List<ScopeResource>>(scopes);
            var userIds = new List<string>(); 
            foreach(var s in scopes)
                userIds.AddRange(s.ScopeUsers.Select(su => su.UserId).ToList());
            var userDetails = await idsUserRepository.GetUserDetails(userIds.ToList());
            foreach (var scope in results)
                foreach (var s in scope.ScopeUsers)
                {
                    s.User = mapper.Map<UserResource>(userDetails.FirstOrDefault(u => u.Id == s.UserId));
                }

            return Ok(results);
        }

        [HttpGet("api/scopes/{id}")]
        public async Task<IActionResult> GetScope(int id)
        {
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var scope = await repository.GetScope(id);
            if (scope == null)
                return NotFound("Nie znaleziono żądanego zeszytu.");

            if (scope.OwnerId != user.Id)
                return Forbid();

            var result = mapper.Map<ScopeResource>(scope);

            try
            {
                tokenRepository.SetToken(Request.Headers[HeaderNames.Authorization]);
                var userDetails = await idsUserRepository.GetUserDetails(scope.ScopeUsers.Select(su => su.UserId).ToList());
                foreach (var s in result.ScopeUsers)
                {
                    s.User = mapper.Map<UserResource>(userDetails.FirstOrDefault(u => u.Id == s.UserId));
                }
            }
            catch (Exception e)
            {
                foreach (var s in result.ScopeUsers)
                {
                    s.User = mapper.Map<UserResource>(new IdentityServerUser { Id = s.UserId, LastName = s.UserId });
                }
            }


            return Ok(result);
        }

        [HttpPost("api/scopes")]
        public async Task<IActionResult> CreateScope([FromBody] ScopeResource scopeResource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = this.User.FindFirstValue(JwtClaimTypes.Email);
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
            {
                userRepository.AddUser(sub, email);
                user = await userRepository.GetUserAsync(sub);
            }

            var scopes = await repository.GetScopes();

            if (scopes.Where(s => s.OwnerId == user.Id).Any(s => s.Name == scopeResource.Name))
            {
                return BadRequest("Zeszyt o podanej nazwie już istnieje.");
            }

            var scope = mapper.Map<Scope>(scopeResource);

            scope.Owner = user;
            await repository.AddScope(scope);
            await unitOfWork.CompleteAsync();

            var newScope = await repository.GetScope(scope.Id);
            if (user.SelectedScope == null)
            {
                userRepository.SetSelectedScope(user.Id, scope.Id);
                await unitOfWork.CompleteAsync();
            }
            return Ok(newScope);
        }


        [HttpDelete("/api/scopes/removeuser/{scopeId}")]
        public async Task<IActionResult> RemoveUserFromScope(int scopeId, string userId)
        {
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var scope = await repository.GetScope(scopeId);
            if (scope == null)
                return NotFound("Nie znaleziono danego zeszytu.");

            if (scope.Owner.Id != user.Id)
            {
                return BadRequest("Zeszyt nie należy do aktualnie zalogowanego użytkownika.");
            }

            var scopeUser = scope.ScopeUsers.FirstOrDefault(su => su.ScopeId == scopeId && su.UserId == userId);
            if (scopeUser == null)
                return NotFound("Nie znaleziono użytkownika o podanym Id w danym zeszycie.");
            repository.RemoveUserFromScope(scope, scopeUser);
            await unitOfWork.CompleteAsync();
            return Ok();
        }

        [HttpDelete("api/scopes/{id}")]
        public async Task<IActionResult> DeleteScope(int id)
        {
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var scope = await repository.GetScope(id);

            if (scope == null)
                return NotFound("Nie znaleziono zeszytu. Mógł zostać wcześniej usunięty");

            if (scope.Owner.Id != user.Id)
            {
                return BadRequest("Zeszyt nie należy do aktualnie zalogowanego użytkownika.");
            }

            if (scope.Expenses.Count > 0 || scope.Categories.Count > 0)
                return BadRequest("Do zeszytu: " + scope.Name + " są przypisane inne elementy. Nie można usunąć");
            repository.DeleteScope(scope);
            await unitOfWork.CompleteAsync();
            return Ok(id);
        }

        [HttpPut("/api/scopes/{id}")]
        public async Task<IActionResult> UpdateScope(int id, [FromBody] ScopeResource scope)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var originalScope = await repository.GetScope(id);
            if (originalScope == null)
                return NotFound("Żądany zeszyt nie istnieje.");
            if (originalScope.Owner.Id != user.Id)
            {
                return BadRequest("Zeszyt nie należy do aktualnie zalogowanego użytkownika.");
            }

            try
            {
                var scopeToBeSaved = mapper.Map<Scope>(scope);
                await repository.UpdateScope(scopeToBeSaved, id);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return NotFound(e.Message);
            }

            await unitOfWork.CompleteAsync();

            var newScope = await repository.GetScope(id);

            return Ok(newScope);
        }

        [HttpPost("/api/scopes/{scopeId}")]
        public async Task<IActionResult> AddUserToScope(int scopeId, string userId, string userEmail)
        {
            var sub = this.User.FindFirstValue(JwtClaimTypes.Subject);
            var user = await userRepository.GetUserAsync(sub);

            if (user == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var userToBeAdded = await userRepository.GetUserAsync(userId);

            if (userToBeAdded == null)
            {
                //return BadRequest("Nie znaleziono użytkownika");
                await userRepository.AddUser(userId, userEmail);
                userToBeAdded = await userRepository.GetUserAsync(userId);
            }

            var scope = user.OwnedScopes.FirstOrDefault(s => s.Id == scopeId);

            if (scope == null)
                return NotFound("Nie znaleziono zeszytu.");

            if (scope.Owner.Id != user.Id)
            {
                return BadRequest("Zeszyt nie należy do aktualnie zalogowanego użytkownika.");
            }

            if (scope.ScopeUsers.Any(su => su.UserId == userToBeAdded.Id))
                return BadRequest("Użytkownik już dodany");

            scope.ScopeUsers.Add(new ScopeUser
            {
                ScopeId = scope.Id,
                UserId = userToBeAdded.Id
            });
            await unitOfWork.CompleteAsync();
            return Ok();
        }
    }
}
