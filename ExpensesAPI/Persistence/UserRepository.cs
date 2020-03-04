﻿using ExpensesAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.Persistence
{
    public class UserRepository : IUserRepository
    {
        private MainDbContext context;
        public UserRepository(MainDbContext context)
        {
            this.context = context;
        }

        public async Task<User> GetUserAsync(string id)
        {
            return await context.Users
                .Include(u => u.SelectedScope)
                .Include(u => u.ScopeUsers)
                    .ThenInclude(su => su.Scope)
                        .ThenInclude(s => s.Owner)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUserListAsync(string query, string myId)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var users = await context.Users
                    .Include(u => u.ScopeUsers)
                    .Where(u => (u.FirstName + " " + u.LastName).ToLower().Contains(query.ToLower()) && u.Id != myId)
                    .ToListAsync();
                return users;
            }
            return new List<User>();
        }

        public async Task<User> GetUserWithScopesAsync(string id)
        {
            return await context.Users
                .Include(u => u.SelectedScope)
                .Include(u => u.OwnedScopes)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        
    }
}
