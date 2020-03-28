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
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseRepository repository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserRepository userRepository;

        public ExpenseController(IExpenseRepository expensesRepository, IUserRepository userRepository, IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            this.repository = expensesRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.httpContextAccessor = httpContextAccessor;
            this.userRepository = userRepository;
        }

        [HttpGet("api/expenses")]
        public async Task<IActionResult> GetExpenses(Query query)
        {
            if (httpContextAccessor.HttpContext.User == null)
                return NotFound("Nie rozpoznano użytkownika.");

            var claim = httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "id");
            var currentScope = (await userRepository.GetUserAsync(claim.Value)).SelectedScope;

            if (currentScope == null)
            {
                return NotFound("Brak wybranego zeszytu.");
            }

            var result = mapper.Map<List<ExpenseResource>>(await repository.GetExpensesAsync(query, currentScope));
            checkForDuplicateExpenses(result);
            return Ok(result);
        }

        [HttpPost("api/expenses")]
        public async Task<IActionResult> CreateExpense([FromBody] ExpenseResourceBase expenseResource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (expenseResource == null)
                return BadRequest("Wydatek nie istnieje.");
            var expense = mapper.Map<Expense>(expenseResource);
            
            repository.AddExpense(expense);

            await unitOfWork.CompleteAsync();
            expense = await repository.GetExpenseAsync(expense.Id);

            var result = mapper.Map<ExpenseResource>(expense);
            return Ok(result);
        }

        [HttpPost("api/expenses/multi")]
        public async Task<IActionResult> CreateExpenses([FromBody] SaveExpenseCollectionResource expenseResources)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (expenseResources == null)
                return BadRequest("Wydatek nie istnieje.");

            var expenses = mapper.Map<List<Expense>>(expenseResources.Expenses);

            repository.AddExpenses(expenses);

            await unitOfWork.CompleteAsync();
            expenses = await repository.GetExpensesAsync(expenses.Select(e => e.Id).ToList());

            var result = expenses.Select(e => mapper.Map<ExpenseResource>(e)).ToList();
            return Ok(result);
        }


        [HttpDelete("api/expenses/multi")]
        public async Task<IActionResult> DeleteExpenses(List<int> ids)
        {
            try
            {
                repository.DeleteExpenses(ids);
                await unitOfWork.CompleteAsync();
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(ids);
        }

        [HttpDelete("api/expenses/{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                repository.DeleteExpense(id);
                await unitOfWork.CompleteAsync();
            }
            catch (ArgumentOutOfRangeException e)
            {
                return NotFound(e.Message);
            }

            return Ok(id);
        }

        [HttpPut("api/expenses/{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseResourceBase expenseResource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var expense = mapper.Map<Expense>(expenseResource);

            try
            {
                await repository.UpdateExpenseAsync(id, expense);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return NotFound(e.Message);
            }

            await unitOfWork.CompleteAsync();

            expense = await repository.GetExpenseAsync(id);
            var result = mapper.Map<ExpenseResource>(expense);

            return Ok(result);
        }

        private void checkForDuplicateExpenses(List<ExpenseResource> list)
        {
            var duplicates = list.GroupBy(e => new { e.Date, e.Value })
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var d in duplicates)
            {
                foreach (var e in d)
                {
                    e.IsDuplicate = true;
                }
            }
        }

    }
}
