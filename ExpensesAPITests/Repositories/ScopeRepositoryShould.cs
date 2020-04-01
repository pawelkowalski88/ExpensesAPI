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
    class ScopeRepositoryShould
    {
        [Test]
        public async Task GetScopesByUserCorrectly()
        {
            using (var context = GetContextWithData())
            {
                var user = context.Users.First(u => u.FirstName == "Zenek");
                var sut = new ScopeRepository(context);
                var scopes = await sut.GetScopes(user);

                Assert.That(scopes.Count, Is.EqualTo(4));
                Assert.That(scopes.Any(s => s.Name == "Test"));
                Assert.That(scopes.Any(s => s.Name == "Test2"));
                Assert.That(scopes.Any(s => s.Name == "Test3"));
            }
        }

        [Test]
        public async Task ReturnNoScopesForScpecificUser()
        {
            using (var context = GetContextWithData())
            {
                var user = context.Users.First(u => u.FirstName == "Tadek");
                var sut = new ScopeRepository(context);
                var scopes = await sut.GetScopes(user);

                Assert.That(scopes.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ReturnNoScopesForNullUser()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ScopeRepository(context);
                var scopes = await sut.GetScopes(null);

                Assert.That(scopes.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task UpdateScopeCorrectly()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ScopeRepository(context);
                var scopeId = context.Scopes.First(s => s.Name == "Test2").Id;
                await sut.UpdateScope(new Scope { Name = "TESTTEST" }, scopeId);
                context.SaveChanges();

                Assert.That(context.Scopes.First(s => s.Id == scopeId).Name, Is.EqualTo("TESTTEST"));
            }
        }

        [Test]
        public void UpdateScopeThrowsExceptionForNonExistentUser()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ScopeRepository(context);

                Assert.That(async () => await sut.UpdateScope(new Scope { Name = "TESTTEST" }, 0), Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public async Task GetTheCorrectScope()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ScopeRepository(context);
                var scopeId = context.Scopes.First(s => s.Name == "Test").Id;
                var result = await sut.GetScope(scopeId);

                Assert.That(result.Name, Is.EqualTo("Test"));

                Assert.That(result.Categories.Count, Is.EqualTo(4));
                Assert.That(result.Categories.Any(c => c.Name == "Category1"));
                Assert.That(result.Categories.Any(c => c.Name == "Category2"));
                Assert.That(result.Categories.Any(c => c.Name == "Category3"));
                Assert.That(result.Categories.Any(c => c.Name == "Category4"));

                Assert.That(result.Expenses.Count, Is.EqualTo(4));
                Assert.That(result.Expenses.Any(c => c.Comment == "Test1"));
                Assert.That(result.Expenses.Any(c => c.Comment == "Test2"));
                Assert.That(result.Expenses.Any(c => c.Comment == "Test3"));
                Assert.That(result.Expenses.Any(c => c.Comment == "Test4"));

            }
        }

        [Test]
        public async Task GetNullOnNonExistentScopeId()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ScopeRepository(context);
                var scopeId = context.Scopes.First(s => s.Name == "Test").Id;
                var result = await sut.GetScope(0);

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

            context.Scopes.Add(new Scope { Name = "Test", Owner = noScope ? null: userForScopes });
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
