using AutoMapper;
using ExpensesAPI.Models;
using ExpensesAPI.Persistence;
using ExpensesAPI.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Controllers
{
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository repository;
        private readonly IScopeRepository scopeRepository;
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CategoryController(ICategoryRepository repository, IScopeRepository scopeRepository, IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            this.repository = repository;
            this.scopeRepository = scopeRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("/api/categories")]
        public async Task<IActionResult> GetCategories(bool includeExpenses)
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "id");
            var currentScope = (await userRepository.GetUserAsync(claim.Value)).SelectedScope;

            if (currentScope == null)
            {
                return NotFound("Brak wybranego zeszytu.");
            }
            var categories = await repository.GetCategories(currentScope.Id, includeExpenses);
            var result = mapper.Map<List<CategoryResource>>(categories);
            return Ok(result);
        }

        [HttpPost("/api/categories")]
        public async Task<IActionResult> CreateCategory([FromBody] SaveCategoryResource categoryResource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "id");
            var currentScope = (await userRepository.GetUserAsync(claim.Value)).SelectedScope;

            if (currentScope == null)
            {
                return NotFound("Brak wybranego zeszytu.");
            }

            var categories = await repository.GetCategories(currentScope.Id, false);
            if (categories.Exists(c => c.Name == categoryResource.Name))
            {
                return BadRequest("Kategoria o podanej nazwie już istnieje.");
            }

            categoryResource.ScopeId = currentScope.Id;

            var category = mapper.Map<Category>(categoryResource);
            repository.AddCategory(category);

            await unitOfWork.CompleteAsync();

            category = await repository.GetCategory(category.Id);

            var result = mapper.Map<CategoryResource>(category);
            return Ok(result);
        }

        [HttpDelete("/api/categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await repository.GetCategory(id, includeExpenses: true);

            if (category == null)
                return NotFound("Nie znaleziono kategorii. Mogła zostać usunięta przez innego uzytkownika.");

            if (category.Expenses.Count > 0)
                return BadRequest("Do kategorii \"" + category.Name + "\" są przyporządkowane wydatki. Zmień ich kategorię lub usuń je przed usunięciem kategorii.");

            repository.DeleteCategory(category);
            await unitOfWork.CompleteAsync();

            return Ok(id);
        }

        [HttpPut("/api/categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] SaveCategoryResource categoryResource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = mapper.Map<Category>(categoryResource);

            try
            {
                await repository.UpdateCategory(id, category);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return NotFound(e.Message);
            }

            await unitOfWork.CompleteAsync();

            category = await repository.GetCategory(id);
            var result = mapper.Map<CategoryResource>(category);

            return Ok(result);
        }
    }
}
