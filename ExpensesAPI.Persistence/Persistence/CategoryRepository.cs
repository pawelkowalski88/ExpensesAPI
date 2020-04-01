using ExpensesAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Persistence
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MainDbContext context;

        public CategoryRepository(MainDbContext context)
        {
            this.context = context;
        }

        public void AddCategory(Category category)
        {
            context.Categories.Add(category);
        }

        public void DeleteCategory(Category category)
        {
            context.Categories.Remove(category);
        }

        public async Task<List<Category>> GetCategories(int scopeId, bool includeExpenses)
        {
            if (includeExpenses)
                return await context.Categories
                    .Where(c => c.ScopeId == scopeId)
                    .Include(c => c.Expenses)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

            return await context.Categories
                .Where(c => c.ScopeId == scopeId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetCategory(int id)
        {
            return await GetCategory(id, false);
        }

        public async Task<Category> GetCategory(int id, bool includeExpenses)
        {
            if (includeExpenses)
                return await context.Categories
                    .Where(c => c.Id == id)
                    .Include(c => c.Expenses)
                    .SingleOrDefaultAsync();

            return await context.Categories
                .Where(c => c.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task UpdateCategory(int id, Category category)
        {
            var categoryToBeUpdated = await context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (categoryToBeUpdated == null)
                throw new ArgumentOutOfRangeException(message: "Żądana kategoria nie istnieje.", innerException: null);

            categoryToBeUpdated.Name = category.Name;

            context.Categories.Update(categoryToBeUpdated);
        }
    }
}
