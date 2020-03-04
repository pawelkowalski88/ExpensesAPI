using ExpensesAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly MainDbContext context;

        public ExpenseRepository(MainDbContext context)
        {
            this.context = context;
        }

        public void AddExpense(Expense expense)
        {
            var date = expense.Date.ToShortDateString();
            context.Expenses.Add(expense);
        }

        public void AddExpenses(IEnumerable<Expense> expenses)
        {
            foreach (var expense in expenses)
            {
                var date = expense.Date.ToShortDateString();
                context.Expenses.Add(expense);
            }
        }

        public void DeleteExpense(Expense expense)
        {
            context.Expenses.Remove(expense);
        }

        public void DeleteExpenses(List<Expense> expenses)
        {
            foreach (var e in expenses)
                context.Expenses.Remove(e);
        }

        public async Task<Expense> GetExpenseAsync(int id)
        {
            return await context.Expenses
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Expense>> GetExpensesAsync(IEnumerable<int> ids = null)
        {
            if (ids != null)
            {
                return await context.Expenses
                    .Where(e => ids.Contains(e.Id))
                    .ToListAsync();
            }
            else
            {
                return await context.Expenses
                    .ToListAsync();
            }
        }

        public async Task<float> GetExpensesGrandTotal(Query query)
        {
            var timeFilteredResult = context.Expenses
                .Where(e => e.Date >= query.StartDate && e.Date < query.EndDate)
                .AsQueryable();

            return CalculateTotal(await timeFilteredResult.ToListAsync(), e => { return true; });
        }

        public async Task<float> GetExpensesGrandTotalIncome(Query query)
        {
            var timeFilteredResult = context.Expenses
                .Where(e => e.Date >= query.StartDate && e.Date < query.EndDate)
                .AsQueryable();

            return CalculateTotal(await timeFilteredResult.ToListAsync(), e => e > 0);
        }

        public async Task<float> GetExpensesGrandTotalCosts(Query query)
        {
            var timeFilteredResult = context.Expenses
                .Where(e => e.Date >= query.StartDate && e.Date < query.EndDate)
                .AsQueryable();

            return CalculateTotal(await timeFilteredResult.ToListAsync(), e => e < 0);
        }

        public Task<List<Expense>> GetExpensesAsync(Query query, Scope scope)
        {
            if (scope == null)
                throw new ArgumentOutOfRangeException(message: "Brak wybranego zeszytu.", innerException: null);

            return context.Expenses
                .Where(e => e.Date >= query.StartDate && e.Date < query.EndDate && e.ScopeId == scope.Id)
                .Include(e => e.Category).ToListAsync();
        }

        public async Task UpdateExpenseAsync(int id, Expense expense)
        {
            var expenseToBeUpdated = await context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

            if (expenseToBeUpdated == null)
                throw new ArgumentOutOfRangeException(message: "Żądany wydatek nie istnieje.", innerException: null);

            expenseToBeUpdated.CategoryId = expense.CategoryId;
            expenseToBeUpdated.Comment = expense.Comment;
            expenseToBeUpdated.Date = expense.Date;
            expenseToBeUpdated.Details = expense.Details;
            expenseToBeUpdated.ScopeId = expense.ScopeId;
            expenseToBeUpdated.Value = expense.Value;

            context.Expenses.Update(expenseToBeUpdated);
        }


        private float CalculateTotal(List<Expense> list, Func<float, bool> condition)
        {
            float result = 0;
            foreach (var e in list)
            {
                if (condition.Invoke(e.Value))
                    result += e.Value;
            }
            return result;
        }
    }
}
