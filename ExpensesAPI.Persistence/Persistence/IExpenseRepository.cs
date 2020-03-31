using ExpensesAPI.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetExpensesAsync(IEnumerable<int> ids);
        Task<List<Expense>> GetExpensesAsync(Query query, Scope scope);
        Task<Expense> GetExpenseAsync(int id);
        void AddExpense(Expense expense);
        void AddExpenses(IEnumerable<Expense> expenses);
        Task DeleteExpense(int expenseId);
        Task DeleteExpenses(List<int> expenseIds);
        Task UpdateExpenseAsync(int id, Expense expense);
        //Task<float> GetExpensesGrandTotal(Query query);
        //Task<float> GetExpensesGrandTotalIncome(Query query);
        //Task<float> GetExpensesGrandTotalCosts(Query query);
    }
}
