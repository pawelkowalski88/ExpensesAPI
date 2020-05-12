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
    class CategoryRepositoryShould
    {
        [Test]
        public async Task ReturnAllCategories()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var scope = context.Scopes.FirstOrDefault(s => s.Name == "Test");
                var result = await sut.GetCategories(scope.Id, true);

                Assert.That(result.Count, Is.EqualTo(4));
                Assert.That(result.Any(c => c.Name == "Category1"));
                Assert.That(result.FirstOrDefault(c => c.Name == "Category1").Expenses.Count, Is.EqualTo(6));
            }
        }

        [Test]
        public async Task ReturnNoCategoriesOnNonExistentScope()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var scope = context.Scopes.FirstOrDefault(s => s.Name == "Test");
                var result = await sut.GetCategories(0, true);

                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ReturnCategoryWithExpenses()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var category = context.Categories.FirstOrDefault(c => c.Name == "Category2");
                var result = await sut.GetCategory(category.Id, true);

                Assert.That(result.Name, Is.EqualTo("Category2"));
                Assert.That(result.Scope.Name, Is.EqualTo("Test"));
            }
        }

        [Test]
        public async Task ReturnNullForNotExistetntCategoryWithExpenses()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var result = await sut.GetCategory(0, true);

                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public async Task ReturnCategory()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var category = context.Categories.FirstOrDefault(c => c.Name == "Category2");
                var result = await sut.GetCategory(category.Id);

                Assert.That(result.Name, Is.EqualTo("Category2"));
                Assert.That(result.Scope.Name, Is.EqualTo("Test"));
            }
        }

        [Test]
        public async Task ReturnNullForNotExistetntCategory()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var result = await sut.GetCategory(0);

                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public async Task AddCategory()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                sut.AddCategory(new Category { Name = "NEW" });
                context.SaveChanges();

                Assert.That(context.Categories.FirstOrDefault(c => c.Name == "NEW").Name, Is.EqualTo("NEW"));
            }
        }

        [Test]
        public async Task DeleteCategory()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var category = context.Categories.FirstOrDefault(c => c.Name == "Category1");
                sut.DeleteCategory(category);
                context.SaveChanges();

                Assert.That(context.Categories.Any(c => c.Name == "Category1"), Is.False);
            }
        }

        [Test]
        public async Task UpdateCategoryForCorrectData()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var category = context.Categories.FirstOrDefault(c => c.Name == "Category1");
                await sut.UpdateCategory(category.Id, new Category { Name = "NameChanged" });
                context.SaveChanges();

                var categoryAfterChange = context.Categories.FirstOrDefault(c => c.Id == category.Id);

                Assert.That(categoryAfterChange.Name, Is.EqualTo("NameChanged"));
                Assert.That(category.Id, Is.EqualTo(categoryAfterChange.Id));
            }
        }

        [Test]
        public async Task ThrowExceptionForIncorrectUpdateData()
        {
            using (var context = GetContextWithData())
            {
                var sut = new CategoryRepository(context);
                var category = new Category { Name = "NameChanged" };

                Assert.That(async () => await sut.UpdateCategory(0, new Category { Name = "NameChanged" }), Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
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
                context.Users.Add(new User { UserName = "Zenek" });
                context.SaveChanges();
            }

            context.Scopes.Add(new Scope { Name = "Test", Owner = noScope ? context.Users.FirstOrDefault() : null });
            context.Scopes.Add(new Scope { Name = "Test2", Owner = noScope ? context.Users.FirstOrDefault() : null });
            context.SaveChanges();

            var selectedScopeId = context.Scopes.FirstOrDefault(s => s.Name == "Test").Id;

            if (!noScope)
            {
                var user = context.Users.FirstOrDefault();
                if (user != null)
                {
                    user.SelectedScope = context.Scopes.FirstOrDefault();
                    //selectedScopeId = user.SelectedScope.Id;
                }
            }

            context.Add(new Category { Name = "Category1", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category2", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category3", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category4", ScopeId = selectedScopeId });

            context.SaveChanges();

            var category = context.Categories.FirstOrDefault(c => c.Name == "Category1");
            //categoryId = category.Id;

            context.Add(new Expense { CategoryId = category.Id, Comment = "Test1", Date = DateTime.Parse("2018-03-04"), Value = 5.32F, Scope = context.Scopes.FirstOrDefault() });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test2", Date = DateTime.Parse("2018-03-04"), Value = 5.32F, Scope = context.Scopes.FirstOrDefault() });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test3", Date = DateTime.Parse("2018-03-04"), Value = 15.32F, Scope = context.Scopes.FirstOrDefault() });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test4", Date = DateTime.Parse("2018-03-07"), Value = 12.32F, Scope = context.Scopes.FirstOrDefault() });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test5", Date = DateTime.Parse("2018-03-07"), Value = 24.32F, Scope = context.Scopes.First(c => c.Name == "Test2") });
            context.Add(new Expense { CategoryId = category.Id, Comment = "Test6", Date = DateTime.Parse("2018-03-07"), Value = 56.32F, Scope = context.Scopes.First(c => c.Name == "Test2") });

            context.SaveChanges();

            return context;
        }
    }
}
