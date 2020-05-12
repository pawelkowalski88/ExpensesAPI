using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPITests.Repositories
{
    [TestFixture]
    class ExpenseRepositoryShould
    {
        [Test]
        public async Task ReturnNoErrorsOnDelete()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var expenseId = context.Expenses.First(e => e.Comment == "Test1").Id;
                await sut.DeleteExpense(expenseId);
                context.SaveChanges();

                Assert.That(context.Expenses.Any(e => e.Comment == "Test1"), Is.False);
            }
        }

        [Test]
        public async Task ThrowsOnDeleteWithNullExpense()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                Assert.That(async () => await sut.DeleteExpense(0), Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public async Task ReturnNoErrorsOnDeleteMultiple()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var expenseIds = context.Expenses.Where(e => e.Comment == "Test1" || e.Comment == "Test2").Select(e => e.Id).ToList();
                await sut.DeleteExpenses(expenseIds);
                context.SaveChanges();

                Assert.That(context.Expenses.Any(e => e.Comment == "Test1"), Is.False);
                Assert.That(context.Expenses.Any(e => e.Comment == "Test2"), Is.False);
                Assert.That(context.Expenses.Any(e => e.Comment == "Test3"), Is.True);
            }
        }

        [Test]
        public async Task ThrowsOnDeleteMultipleWithNullExpense()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                Assert.That(async () => await sut.DeleteExpenses(null), Throws.TypeOf<ArgumentNullException>());
            }
        }

        [Test]
        public async Task ReturnTheRightExpense()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var expenseId = context.Expenses.First(e => e.Comment == "Test1").Id;
                var result = await sut.GetExpenseAsync(expenseId);

                Assert.That(result.Id, Is.EqualTo(expenseId));
                Assert.That(result.Comment, Is.EqualTo("Test1"));
            }
        }

        [Test]
        public async Task ReturnNullExpenseOnWrongId()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var result = await sut.GetExpenseAsync(0);

                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public async Task ReturnAllExpenses()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var result = await sut.GetExpensesAsync();

                Assert.That(result.Count, Is.EqualTo(6));
            }
        }

        [Test]
        public async Task ReturnSelectedExpenses()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context); 
                var expenseIds = context.Expenses.Where(e => e.Comment == "Test1" || e.Comment == "Test2").Select(e => e.Id).ToList();
                var results = await sut.GetExpensesAsync(expenseIds);

                Assert.That(results.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task ReturnEmptyListOfExpenses()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var results = await sut.GetExpensesAsync(new List<int>());

                Assert.That(results.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ThrowExceptionOnNonExistentScope()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);

                Assert.That(async () => await sut.GetExpensesAsync(new Query { StartDate = DateTime.Parse("2018-03-04"), EndDate = DateTime.Parse("2018-03-05") }, null), Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [Test]
        public async Task ReturnCorrectExpensesBetweenGivenDates()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var scope = context.Scopes.First(s => s.Name == "Test");
                var result = await sut.GetExpensesAsync(new Query { StartDate = DateTime.Parse("2018-03-04"), EndDate = DateTime.Parse("2018-03-05") }, scope);
                
                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result.Any(e => e.Comment == "Test1"));
                Assert.That(result.Any(e => e.Comment == "Test2"));
                Assert.That(result.Any(e => e.Comment == "Test3"));
            }
        }

        [Test]
        public async Task ReturnZeroExpensesForScopeWithNoExpenses()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var scope = context.Scopes.First(s => s.Name == "Test3");
                var result = await sut.GetExpensesAsync(new Query { StartDate = DateTime.Parse("2018-03-04"), EndDate = DateTime.Parse("2018-03-05") }, scope);

                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task ThrowsOnUpdatingNonExistentExpense()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                Assert.That(async () => await sut.UpdateExpenseAsync(0, new Expense()), Throws.TypeOf<ArgumentOutOfRangeException>());
            }
        }


        [Test]
        public async Task UpdatesExpenseCorrectly()
        {
            using (var context = GetContextWithData())
            {
                var sut = new ExpenseRepository(context);
                var expense = context.Expenses.First(e => e.Comment == "Test1");
                var newExpense = new Expense 
                {
                    CategoryId = expense.CategoryId,
                    Comment = "Baba",
                    Date = expense.Date,
                    ScopeId = expense.ScopeId,
                    Value = expense.Value
                };

                await sut.UpdateExpenseAsync(expense.Id, newExpense);
                context.SaveChanges();

                Assert.That(context.Expenses.First(e => e.Id == expense.Id).Comment, Is.EqualTo("Baba"));

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
            context.Scopes.Add(new Scope { Name = "Test3", Owner = noScope ? context.Users.FirstOrDefault() : null });
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
