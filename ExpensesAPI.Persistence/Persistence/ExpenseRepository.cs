using ExpensesAPI.Persistence.Models;
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
            context.Expenses.Add(expense);
        }

        public void AddExpenses(IEnumerable<Expense> expenses)
        {
            foreach (var expense in expenses)
            {
                context.Expenses.Add(expense);
            }
        }

        public async Task DeleteExpense(int expenseId)
        {
            var expense = await context.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId);

            if (expense == null)
                throw new ArgumentOutOfRangeException("Nie znaleziono wydatku.", innerException: null);

            context.Expenses.Remove(expense);
        }

        public async Task DeleteExpenses(List<int> expenseIds)
        {
            if (expenseIds == null)
                throw new ArgumentNullException(nameof(expenseIds));

            var expenses = await context.Expenses.Where(e => expenseIds.Contains(e.Id)).ToListAsync();
            foreach (var e in expenses)
                context.Expenses.Remove(e);
        }

        public async Task<Expense> GetExpenseAsync(int id)
        {
            return await context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id);
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

        public Task<List<Expense>> GetExpensesAsync(Query query, Scope scope)
        {
            if (scope == null)
                throw new ArgumentOutOfRangeException(message: "Brak wybranego zeszytu.", innerException: null);

            return context.Expenses
                .Where(e => e.Date >= query.StartDate && e.Date < query.EndDate && e.ScopeId == scope.Id)
                .Include(e => e.Category)
                .ToListAsync();
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
    }
}
