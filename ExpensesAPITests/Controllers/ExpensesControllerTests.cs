using AutoMapper;
using ExpensesAPI.Controllers;
using ExpensesAPI.Mapping;
using ExpensesAPI.Models;
using ExpensesAPI.Persistence;
using ExpensesAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAPITests.Controllers
{
    [TestFixture]
    class ExpenseControllerTests
    {
        private Mock<IExpenseRepository> repository;
        private Mock<IUserRepository> userRepository;
        private Mock<IUnitOfWork> unitOfWork;
        private IMapper mapper;

        [SetUp]
        public void Setup()
        {
            repository = new Mock<IExpenseRepository>();
            userRepository = new Mock<IUserRepository>();
            unitOfWork = new Mock<IUnitOfWork>();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));
        }

        #region GetExpenses
        [Test]
        public async Task GetExpensesReturns4RowsAsync()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                //var repository = new ExpenseRepository(context);
                //var userRepository = new UserRepository(context);
                //var unitOfWork = new EFUnitOfWork(context);

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, new FakeHttpContextAccessor(context));

                var request = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okResult = request as OkObjectResult;

                var results = okResult.Value as List<ExpenseResource>;

                Assert.AreEqual(4, results.Count);
                Assert.IsNotNull(results.First(r => r.Comment == "Test1"));
                Assert.IsNotNull(results.First(r => r.Comment == "Test2"));
                Assert.IsNotNull(results.First(r => r.Comment == "Test3"));
                Assert.IsNotNull(results.First(r => r.Comment == "Test4"));

                Assert.Throws<InvalidOperationException>(() => results.First(r => r.Comment == "Test5"));
                Assert.Throws<InvalidOperationException>(() => results.First(r => r.Comment == "Test6"));

                Assert.IsTrue(results.First(r => r.Comment == "Test1").IsDuplicate);
                Assert.IsTrue(results.First(r => r.Comment == "Test2").IsDuplicate);

                Assert.IsFalse(results.First(r => r.Comment == "Test3").IsDuplicate);
                Assert.IsFalse(results.First(r => r.Comment == "Test4").IsDuplicate);
            }
        }

        [Test]
        public async Task GetExpensesThrowsWhenUserIsNullAsync()
        {
            int category;
            using (var context = GetContextWithData(out category, noUser: true))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var request = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var notFoundResult = request as NotFoundObjectResult;

                Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task GetExpensesThrowsWhenScopeIsNullAsync()
        {
            int category;
            using (var context = GetContextWithData(out category, noScope: true))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var request = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var notFoundResult = request as NotFoundObjectResult;

                Assert.AreEqual("Brak wybranego zeszytu.", notFoundResult.Value.ToString());
            }
        }
        #endregion

        #region CreateExpense

        [Test]
        public async Task CreateExpenseRetrunsOk()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var date = DateTime.Now;


                controller.ModelState.Clear();
                var request = await controller.CreateExpense(new ExpenseResourceBase
                {
                    CategoryId = category,
                    Comment = "TestAdd",
                    Date = date.ToString(),
                    Details = "Details test",
                    //ScopeId = 1,
                    Value = -23.54F
                });

                var okResult = request as OkObjectResult;
                var result = okResult.Value as ExpenseResource;

                Assert.AreEqual(category, result.CategoryId);
                Assert.AreEqual("TestAdd", result.Comment);
                Assert.AreEqual(date.ToString("yyyy-MM-dd"), result.Date);
                Assert.AreEqual("Details test", result.Details);
                //Assert.AreEqual(1, result.ScopeId);
                Assert.AreEqual(-23.54F, result.Value);
                Assert.IsFalse(result.IsDuplicate);
            }
        }

        [Test]
        public async Task CreateExpenseRetrunsBadRequestOnValidatonError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var date = DateTime.Now;

                controller.ModelState.AddModelError("Test", "Test");
                var request = await controller.CreateExpense(new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = null,
                    Date = date.ToString(),
                    Details = "Details test",
                    //ScopeId = 1,
                    Value = -23.54F
                });

                Assert.IsInstanceOf<BadRequestObjectResult>(request);
            }
        }

        [Test]
        public void CreateExpenseTestValidationOK()
        {
            var expense = new ExpenseResourceBase
            {
                CategoryId = 1,
                Comment = "TestAdd",
                Date = DateTime.Now.ToString(),
                Details = "Details test",
                //ScopeId = 1,
                Value = -23.54F
            };
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(expense, new System.ComponentModel.DataAnnotations.ValidationContext(expense), results, true);

            Assert.IsTrue(isModelStateValid);
        }

        [Test]
        public void CreateExpenseTestValidationFalseNoComment()
        {
            var expense = new ExpenseResourceBase
            {
                CategoryId = 1,
                Comment = "",
                Date = DateTime.Now.ToString(),
                Details = "Details test",
                //ScopeId = 1,
                Value = -23.54F
            };
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(expense, new System.ComponentModel.DataAnnotations.ValidationContext(expense), results, true);

            Assert.IsFalse(isModelStateValid);
        }

        [Test]
        public void CreateExpenseTestValidationFalseNoCategoryId()
        {
            var expense = new ExpenseResourceBase
            {
                CategoryId = 0,
                Comment = "Test",
                Date = DateTime.Now.ToString(),
                Details = "Details test",
                //ScopeId = 1,
                Value = -23.54F
            };
            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(expense, new System.ComponentModel.DataAnnotations.ValidationContext(expense), results, true);

            Assert.IsFalse(isModelStateValid);
        }

        #endregion

        #region CreateExpenses

        [Test]
        public async Task CreateExpensesRetrunsOk()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var date = DateTime.Now;

                controller.ModelState.Clear();
                var resource = new SaveExpenseCollectionResource
                {
                    Expenses = new List<ExpenseResourceBase>
                    {
                        new ExpenseResourceBase
                        {
                            CategoryId = category,
                            Comment = "TestAdd",
                            Date = date.ToString(),
                            Details = "Details test",
                            //ScopeId = 1,
                            Value = -23.54F
                        },
                        new ExpenseResourceBase
                        {
                            CategoryId = category,
                            Comment = "TestAdd2",
                            Date = date.ToString(),
                            Details = "Details test 2",
                            //ScopeId = 1,
                            Value = -98.01F
                        },
                    }
                };

                var request = await controller.CreateExpenses(resource);

                var okResult = request as OkObjectResult;
                var result = okResult.Value as List<ExpenseResource>;

                var id1 = result.IndexOf(result.FirstOrDefault(e => e.Comment == "TestAdd"));
                var id2 = result.IndexOf(result.FirstOrDefault(e => e.Comment == "TestAdd2"));

                Assert.AreEqual(2, result.Count);

                Assert.AreEqual(category, result[id1].CategoryId);
                Assert.AreEqual("TestAdd", result[id1].Comment);
                Assert.AreEqual(date.ToString("yyyy-MM-dd"), result[id1].Date);
                Assert.AreEqual("Details test", result[id1].Details);
                //Assert.AreEqual(1, result[0].ScopeId);
                Assert.AreEqual(-23.54F, result[id1].Value);
                Assert.IsFalse(result[id1].IsDuplicate);

                Assert.AreEqual(category, result[id2].CategoryId);
                Assert.AreEqual("TestAdd2", result[id2].Comment);
                Assert.AreEqual(date.ToString("yyyy-MM-dd"), result[id2].Date);
                Assert.AreEqual("Details test 2", result[id2].Details);
                //Assert.AreEqual(1, result[1].ScopeId);
                Assert.AreEqual(-98.01F, result[id2].Value);
                Assert.IsFalse(result[id2].IsDuplicate);
            }
        }

        [Test]
        public async Task CreateExpensesRetrunsBadRequestOnValidatonError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var date = DateTime.Now;

                controller.ModelState.AddModelError("Test", "Test value");
                var resource = new SaveExpenseCollectionResource
                {
                    Expenses = new List<ExpenseResourceBase>
                    {
                        new ExpenseResourceBase
                        {
                            CategoryId = 1,
                            Comment = "TestAdd",
                            Date = date.ToString(),
                            Details = "Details test",
                            //ScopeId = 1,
                            Value = -23.54F
                        },
                        new ExpenseResourceBase
                        {
                            CategoryId = 1,
                            Comment = "TestAdd2",
                            Date = date.ToString(),
                            Details = "Details test 2",
                            //ScopeId = 1,
                            Value = -98.01F
                        },
                    }
                };

                var request = await controller.CreateExpenses(resource);

                var badRequestResult = request as BadRequestObjectResult;
                var error = badRequestResult.Value as SerializableError;
                var errorList = error["Test"] as string[];
                Assert.IsInstanceOf<BadRequestObjectResult>(request);
                Assert.AreEqual("Test", error.Keys.ToList()[0]);
                Assert.AreEqual("Test value", errorList[0]);
            }
        }

        [Test]
        public void CreateExpensesTestValidationOK()
        {
            var expenses = new List<ExpenseResourceBase> {
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test",
                    //ScopeId = 1,
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
                    //ScopeId = 1,
                    Value = -98.01F
                },
            }.ToArray();

            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(expenses, new System.ComponentModel.DataAnnotations.ValidationContext(expenses), results, true);

            Assert.IsTrue(isModelStateValid);
        }

        [Test]
        public void CreateExpensesTestValidationFalseNoComment()
        {
            var resource = new SaveExpenseCollectionResource { };
            var expenses = new List<ExpenseResourceBase> {
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test",
                    //ScopeId = 1,
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
                    //ScopeId = 1,
                    Value = -98.01F
                },
            };

            resource.Expenses = expenses;

            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(resource, new System.ComponentModel.DataAnnotations.ValidationContext(resource), results, true);

            Assert.IsFalse(isModelStateValid);
        }

        [Test]
        public void CreateExpensesTestValidationFalseNoScopeId()
        {
            var resource = new SaveExpenseCollectionResource { };
            var expenses = new List<ExpenseResourceBase> {
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd1",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test",
                    //ScopeId = 1,
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
                    //ScopeId = 0,
                    Value = -98.01F
                },
            };

            resource.Expenses = expenses;

            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(resource, new System.ComponentModel.DataAnnotations.ValidationContext(resource), results, true);

            Assert.IsFalse(isModelStateValid);
        }

        [Test]
        public void CreateExpensesTestValidationFalseNoCommentId()
        {
            var resource = new SaveExpenseCollectionResource { };
            var expenses = new List<ExpenseResourceBase> {
                new ExpenseResourceBase
                {
                    CategoryId = 0,
                    Comment = "TestAdd1",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test",
                    //ScopeId = 1,
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
                    //ScopeId = 1,
                    Value = -98.01F
                },
            };

            resource.Expenses = expenses;

            var results = new List<ValidationResult>();
            var isModelStateValid = Validator.TryValidateObject(resource, new System.ComponentModel.DataAnnotations.ValidationContext(resource), results, true);

            Assert.IsFalse(isModelStateValid);
        }

        #endregion

        [Test]
        public async Task DeleteExpensesReturnsOk()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var getAllExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetAllExpensesResult = getAllExpensesRequest as OkObjectResult;

                var resultsAll = okGetAllExpensesResult.Value as List<ExpenseResource>;

                var request = await controller.DeleteExpenses(resultsAll.Where(e => e.Comment == "Test2" || e.Comment == "Test4").Select(e => e.Id).ToList());

                var okResult = request as OkObjectResult;

                var getExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetExpensesResult = getExpensesRequest as OkObjectResult;

                var results = okGetExpensesResult.Value as List<ExpenseResource>;

                Assert.AreEqual(2, results.Count);
                Assert.IsNotNull(results.FirstOrDefault(r => r.Comment == "Test1"));
                Assert.IsNotNull(results.FirstOrDefault(r => r.Comment == "Test3"));
            }
        }

        [Test]
        public async Task DeleteExpensesReturnsOkWithZeroId()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var getAllExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetAllExpensesResult = getAllExpensesRequest as OkObjectResult;

                var resultsAll = okGetAllExpensesResult.Value as List<ExpenseResource>;
                var deletionList = resultsAll.Where(e => e.Comment == "Test2").Select(e => e.Id).ToList();
                deletionList.Add(0);

                var request = await controller.DeleteExpenses(deletionList);

                var okResult = request as OkObjectResult;
                var deletedItemsIds = okResult.Value as List<int>;

                var getExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetExpensesResult = getExpensesRequest as OkObjectResult;

                var results = okGetExpensesResult.Value as List<ExpenseResource>;

                Assert.AreEqual(3, results.Count);
                Assert.AreEqual(1, deletedItemsIds.Count);
                Assert.AreEqual(deletionList[0], deletedItemsIds[0]);
                Assert.IsNotNull(results.First(r => r.Comment == "Test1"));
                Assert.IsNotNull(results.First(r => r.Comment == "Test3"));
                Assert.IsNotNull(results.First(r => r.Comment == "Test4"));
            }
        }

        [Test]
        public async Task DeleteExpenseReturnsOK()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var getAllExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetAllExpensesResult = getAllExpensesRequest as OkObjectResult;

                var resultsAll = okGetAllExpensesResult.Value as List<ExpenseResource>;
                var itemToBeDeleted = resultsAll[0].Id;

                var request = await controller.DeleteExpense(itemToBeDeleted);

                var okResult = request as OkObjectResult;
                var deleteItemId = (int)okResult.Value;

                var getExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetExpensesResult = getExpensesRequest as OkObjectResult;

                var results = okGetExpensesResult.Value as List<ExpenseResource>;

                Assert.AreEqual(itemToBeDeleted, deleteItemId);
                Assert.AreEqual(3, results.Count);
            }
        }

        [Test]
        public async Task DeleteExpenseReturnsError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var getAllExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetAllExpensesResult = getAllExpensesRequest as OkObjectResult;

                var resultsAll = okGetAllExpensesResult.Value as List<ExpenseResource>;
                var itemToBeDeleted = 0;

                var request = await controller.DeleteExpense(itemToBeDeleted);

                var notFoundResult = request as NotFoundObjectResult;

                var getExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetExpensesResult = getExpensesRequest as OkObjectResult;

                var results = okGetExpensesResult.Value as List<ExpenseResource>;

                //Assert.AreEqual(itemToBeDeleted, deleteItemId);
                Assert.AreEqual(4, results.Count);
            }
        }

        [Test]
        public async Task UpdateExpenseReturnsOK()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var getAllExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetAllExpensesResult = getAllExpensesRequest as OkObjectResult;

                var resultsAll = okGetAllExpensesResult.Value as List<ExpenseResource>;
                var itemToBeUpdated = resultsAll[0];
                var id = itemToBeUpdated.Id;

                var result = await controller.UpdateExpense(id, new ExpenseResourceBase
                {
                    CategoryId = itemToBeUpdated.CategoryId,
                    Comment = "New comment",
                    Date = "2018-03-04",
                    Details = itemToBeUpdated.Details,
                    //ScopeId = itemToBeUpdated.ScopeId,
                    Value = itemToBeUpdated.Value
                });

                var okUpdateResult = result as OkObjectResult;
                var expense = okUpdateResult.Value as ExpenseResource;

                Assert.AreEqual(expense.Comment, "New comment");
                Assert.AreEqual(expense.Date, "2018-03-04");
            }
        }

        [Test]
        public async Task UpdateExpenseReturnsErrorOnNonExistentExpense()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new ExpenseRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ExpenseController(repository, userRepository, mapper, unitOfWork, new FakeHttpContextAccessor(context));

                var getAllExpensesRequest = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okGetAllExpensesResult = getAllExpensesRequest as OkObjectResult;

                var resultsAll = okGetAllExpensesResult.Value as List<ExpenseResource>;
                var itemToBeUpdated = resultsAll[0];
                var id = 0;

                var result = await controller.UpdateExpense(id, new ExpenseResourceBase
                {
                    CategoryId = itemToBeUpdated.CategoryId,
                    Comment = "New comment",
                    Date = "2018-03-04",
                    Details = itemToBeUpdated.Details,
                    //ScopeId = itemToBeUpdated.ScopeId,
                    Value = itemToBeUpdated.Value
                });

                var notFoundUpdateResult = result as NotFoundObjectResult;
                //var expense = okUpdateResult.Value as ExpenseResource;

                Assert.AreEqual("Żądany wydatek nie istnieje.", notFoundUpdateResult.Value.ToString());
                //Assert.AreEqual(expense.Date, "2018-03-04");
            }
        }

        private MainDbContext GetContextWithData(out int categoryId, bool noScope = false, bool noUser = false)
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
                context.SaveChanges();
            }

            context.Scopes.Add(new Scope { Name = "Test", Owner = noScope ? context.Users.FirstOrDefault() : null });
            context.Scopes.Add(new Scope { Name = "Test2", Owner = noScope ? context.Users.FirstOrDefault() : null });
            context.SaveChanges();

            var selectedScopeId = 1;
            if (!noScope)
            {
                var user = context.Users.FirstOrDefault();
                if (user != null)
                {
                    user.SelectedScope = context.Scopes.FirstOrDefault();
                    selectedScopeId = user.SelectedScope.Id;
                }
            }

            context.Add(new Category { Name = "Category1", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category2", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category3", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category4", ScopeId = selectedScopeId });

            context.SaveChanges();

            var category = context.Categories.FirstOrDefault();
            categoryId = category.Id;

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
