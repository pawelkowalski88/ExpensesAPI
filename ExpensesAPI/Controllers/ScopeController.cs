using AutoMapper;
using ExpensesAPI.Models;
using ExpensesAPI.Persistence;
using ExpensesAPI.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Controllers
{
    public class ScopeController : ControllerBase
    {
        private readonly IScopeRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public ScopeController(
            IScopeRepository repository,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IMapper mapper)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.httpContextAccessor = httpContextAccessor;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }
        [HttpGet("api/scopes")]
        public async Task<IActionResult> GetScopes()
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserAsync(claim.Value);
            var scopes = await repository.GetScopes(user);
            return Ok(mapper.Map<List<ScopeResource>>(scopes));
        }

        [HttpGet("api/scopes/{id}")]
        public async Task<IActionResult> GetScope(int id)
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserAsync(claim.Value);

            var scope = await repository.GetScope(id);
            if (scope == null)
                return NotFound("Nie znaleziono żądanego zeszytu.");

            if (scope.OwnerId != user.Id)
                return Forbid();
            return Ok(mapper.Map<ScopeResource>(scope));
        }

        [HttpPost("api/scopes")]
        public async Task<IActionResult> CreateScope([FromBody] ScopeResource scopeResource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserAsync(claim.Value);

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
                //user.SelectedScope = newScope;
                //await unitOfWork.CompleteAsync();
            }
            return Ok(newScope);
        }


        [HttpDelete("/api/scopes/removeuser/{scopeId}")]
        public async Task<IActionResult> RemoveUserFromScope(int scopeId, string userId)
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserWithScopesAsync(claim.Value);

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
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserWithScopesAsync(claim.Value);

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

            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserWithScopesAsync(claim.Value);

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
        public async Task<IActionResult> AddUserToScope(int scopeId, string userId)
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.Single(c => c.Type == "id");
            var user = await userRepository.GetUserWithScopesAsync(claim.Value);

            var userToBeAdded = await userRepository.GetUserAsync(userId);

            if (userToBeAdded == null)
                return BadRequest("Nie znaleziono użytkownika");

            var scope = user.OwnedScopes.FirstOrDefault(s => s.Id == scopeId);

            if (scope == null)
                return NotFound("Nie znaleziono zeszytu.");

            //if (scope.Owner.Id != user.Id)
            //{
            //    return BadRequest("Zeszyt nie należy do aktualnie zalogowanego użytkownika.");
            //}

            scope.ScopeUsers.Add(new ScopeUser
            {
                Scope = scope,
                User = userToBeAdded
            });
            await unitOfWork.CompleteAsync();
            return Ok();
        }
    }
}
