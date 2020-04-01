using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAPITests.Repositories
{
    [TestFixture]
    public class UserRepositoryShould
    {
        [Test]
        public async Task AssignScopeToUserCorrectly()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var user = context.Users.First(u => u.FirstName == "Tadek");
                var scope = context.Scopes.First(s => s.Name == "Test4");
                await sut.SetSelectedScope(user.Id, scope.Id);
                context.SaveChanges();

                var userAfterChange = context.Users
                    .Include(u => u.SelectedScope)
                    .First(u => u.FirstName == "Tadek");

                Assert.That(userAfterChange.SelectedScope.Name, Is.EqualTo("Test4"));
            }
        }

        [Test]
        public void ThrowExceptionOnNonExistentScope()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var user = context.Users.First(u => u.FirstName == "Tadek");
                var scope = context.Scopes.First(s => s.Name == "Test4");

                Assert.That(async () => await sut.SetSelectedScope(user.Id, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public void ThrowExceptionOnNonExistentUser()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var user = context.Users.First(u => u.FirstName == "Tadek");
                var scope = context.Scopes.First(s => s.Name == "Test4");

                Assert.That(async () => await sut.SetSelectedScope("0", scope.Id), Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public async Task ReturnUsersBelongingToScope()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var results = await sut.GetUserListAsync("Zen", "0");

                Assert.That(results.Any(u => u.FirstName == "Zenek"));
            }
        }

        [Test]
        public async Task ReturnTwoUsers()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var results = await sut.GetUserListAsync("e", "0");

                Assert.That(results.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task ReturnNoUsers()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var results = await sut.GetUserListAsync("abc", "0");

                Assert.That(results.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ReturnNoUsersForWhiteSpaceQuery()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var results = await sut.GetUserListAsync("  ", "0");

                Assert.That(results.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ReturnUserWithScopesCorrectly()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var user = context.Users.First(u => u.FirstName == "Zenek");
                var result = await sut.GetUserWithScopesAsync(user.Id);

                Assert.That(result.OwnedScopes.Count, Is.EqualTo(4));
            }
        }


        [Test]
        public async Task ReturnNullOnNonExistentUser()
        {
            using (var context = GetContextWithData())
            {
                var sut = new UserRepository(context);
                var result = await sut.GetUserWithScopesAsync("0");

                Assert.That(result, Is.Null);
            }
        }

        private MainDbContext GetContextWithData(bool noScope = false, bool noUser = false)
        {
            var options = new DbContextOptionsBuilder<MainDbContext>()
                                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                                .Options;

            var context = new MainDbContext(options);

            foreach (var u in context.Users)
                context.Users.Remove(u);
            context.SaveChanges();

            foreach (var s in context.Scopes)
                context.Scopes.Remove(s);
            context.SaveChanges();

            foreach (var c in context.Categories)
                context.Categories.Remove(c);
            context.SaveChanges();

            foreach (var e in context.Expenses)
                context.Expenses.Remove(e);
            context.SaveChanges();

            if (!noUser)
            {
                context.Users.Add(new User { FirstName = "Zenek" });
                context.Users.Add(new User { FirstName = "Tadek" });
                context.SaveChanges();
            }

            var userForScopes = context.Users.First(u => u.FirstName == "Zenek");

            context.Scopes.Add(new Scope { Name = "Test", Owner = noScope ? null : userForScopes });
            context.Scopes.Add(new Scope { Name = "Test2", Owner = noScope ? null : userForScopes });
            context.Scopes.Add(new Scope { Name = "Test3", Owner = noScope ? null : userForScopes });
            context.Scopes.Add(new Scope { Name = "Test4", Owner = noScope ? null : userForScopes });
            context.SaveChanges();

            var selectedScope = context.Scopes.FirstOrDefault(s => s.Name == "Test");

            if (!noScope)
            {
                var user = context.Users.FirstOrDefault();
                if (user != null)
                {
                    user.SelectedScope = context.Scopes.FirstOrDefault();
                    //selectedScopeId = user.SelectedScope.Id;
                }
            }

            context.Add(new Category { Name = "Category1", ScopeId = selectedScope.Id });
            context.Add(new Category { Name = "Category2", ScopeId = selectedScope.Id });
            context.Add(new Category { Name = "Category3", ScopeId = selectedScope.Id });
            context.Add(new Category { Name = "Category4", ScopeId = selectedScope.Id });

            context.SaveChanges();

            var category = context.Categories.FirstOrDefault(c => c.Name == "Category1");
            //categoryId = category.Id;

            context.Add(new Expense { CategoryId = category.Id, Comment = "Test1", Date = DateTime.Parse("2018-03-04"), Value = 5.32F, Scope = selectedScope });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test2", Date = DateTime.Parse("2018-03-04"), Value = 5.32F, Scope = selectedScope });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test3", Date = DateTime.Parse("2018-03-04"), Value = 15.32F, Scope = selectedScope });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test4", Date = DateTime.Parse("2018-03-07"), Value = 12.32F, Scope = selectedScope });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test5", Date = DateTime.Parse("2018-03-07"), Value = 24.32F, Scope = context.Scopes.First(c => c.Name == "Test2") });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test6", Date = DateTime.Parse("2018-03-07"), Value = 56.32F, Scope = context.Scopes.First(c => c.Name == "Test2") });

            context.SaveChanges();

            return context;
        }
    }
}
