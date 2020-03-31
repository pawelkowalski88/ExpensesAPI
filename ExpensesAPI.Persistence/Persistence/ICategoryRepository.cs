using ExpensesAPI.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetCategories(int scopeId, bool includeExpenses);
        void AddCategory(Category category);
        Task<Category> GetCategory(int id);
        Task<Category> GetCategory(int id, bool includeExpenses);
        void DeleteCategory(Category category);
        Task UpdateCategory(int id, Category category);
    }
}
