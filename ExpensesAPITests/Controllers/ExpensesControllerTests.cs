using AutoMapper;
using ExpensesAPI.Controllers;
using ExpensesAPI.Domain.Mapping;
using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain.Persistence;
using ExpensesAPI.Domain.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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
        private Mock<IHttpContextAccessor> httpContextAccessor;
        private Mock<HttpContext> httpContext;

        [SetUp]
        public void Setup()
        {
            repository = new Mock<IExpenseRepository>();
            userRepository = new Mock<IUserRepository>();
            unitOfWork = new Mock<IUnitOfWork>();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));
            httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContext = new Mock<HttpContext>();

            httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);
            httpContext.Setup(c => c.User)
                .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                        {
                            new ClaimsIdentity(new List<Claim>{ new Claim("id", "Zenek") })
                        })
                );
        }

        #region GetExpenses
        [Test]
        public async Task GetExpensesReturns4RowsAsync()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var request = await controller.GetExpenses(new Query { StartDate = DateTime.Parse("2018-03-01"), EndDate = DateTime.Parse("2018-03-31") });
                var okResult = request as OkObjectResult;

                var results = okResult.Value as List<ExpenseResource>;

                userRepository.Verify(r => r.GetUserAsync("Zenek"));
            }
        }

        [Test]
        public async Task GetExpensesThrowsWhenUserIsNullAsync()
        {
            int category;
            using (var context = GetContextWithData(out category, noUser: true))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } }));
                httpContext.Setup(c => c.User).Returns<ClaimsPrincipal>(null);

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

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
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

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
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.GetExpenseAsync(It.IsAny<int>())).Returns(Task.Run(() => new Expense { CategoryId = category, Comment = "TestAdd", Details = "Details test", Value = -23.54F }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var date = DateTime.Now;


                controller.ModelState.Clear();
                var request = await controller.CreateExpense(new ExpenseResourceBase
                {
                    CategoryId = category,
                    Comment = "TestAdd",
                    Date = date.ToString(),
                    Details = "Details test",
                    Value = -23.54F
                });

                var okResult = request as OkObjectResult;
                var result = okResult.Value as ExpenseResource;

                Assert.AreEqual(category, result.CategoryId);
                Assert.AreEqual("TestAdd", result.Comment);
                Assert.AreEqual("Details test", result.Details);
                Assert.AreEqual(-23.54F, result.Value);
                Assert.IsFalse(result.IsDuplicate);
            }
        }

        [Test]
        public async Task CreateExpenseOnNullInput()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.GetExpenseAsync(It.IsAny<int>())).Returns(Task.Run(() => new Expense { CategoryId = category, Comment = "TestAdd", Details = "Details test", Value = -23.54F }));
                repository.Setup(r => r.AddExpense(It.IsAny<Expense>())).Throws<ArgumentNullException>();
                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var request = await controller.CreateExpense(null);
                Assert.That(request, Is.TypeOf<BadRequestObjectResult>());
            }
        }

        [Test]
        public async Task CreateExpenseRetrunsBadRequestOnValidatonError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var date = DateTime.Now;

                controller.ModelState.AddModelError("Test", "Test");
                var request = await controller.CreateExpense(new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = null,
                    Date = date.ToString(),
                    Details = "Details test",
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
                var date = DateTime.Now;
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.GetExpensesAsync(It.IsAny<IEnumerable<int>>())).Returns(Task.Run(() => new List<Expense>
                {
                    new Expense
                    {
                        CategoryId = category,
                        Comment = "TestAdd",
                        Date = date,
                        Details = "Details test",
                        Value = -23.54F
                    },
                    new Expense
                    {
                        CategoryId = category,
                        Comment = "TestAdd2",
                        Date = date,
                        Details = "Details test 2",
                        Value = -98.01F
                    }
                }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);


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
                            Value = -23.54F
                        },
                        new ExpenseResourceBase
                        {
                            CategoryId = category,
                            Comment = "TestAdd2",
                            Date = date.ToString(),
                            Details = "Details test 2",
                            Value = -98.01F
                        },
                    }
                };

                var request = await controller.CreateExpenses(resource);

                repository.Verify(r => r.AddExpenses(It.IsAny<IEnumerable<Expense>>()));
            }
        }


        [Test]
        public async Task CreateExpensesOnNullInput()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.GetExpenseAsync(It.IsAny<int>())).Returns(Task.Run(() => new Expense { CategoryId = category, Comment = "TestAdd", Details = "Details test", Value = -23.54F }));
                repository.Setup(r => r.AddExpenses(null)).Throws<ArgumentNullException>();
                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var request = await controller.CreateExpenses(null);
                Assert.That(request, Is.TypeOf<BadRequestObjectResult>());
            }
        }

        [Test]
        public async Task CreateExpensesRetrunsBadRequestOnValidatonError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var date = DateTime.Now;
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

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
                            Value = -23.54F
                        },
                        new ExpenseResourceBase
                        {
                            CategoryId = 1,
                            Comment = "TestAdd2",
                            Date = date.ToString(),
                            Details = "Details test 2",
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
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
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
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
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
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
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
                    Value = -23.54F
                },
                new ExpenseResourceBase
                {
                    CategoryId = 1,
                    Comment = "TestAdd2",
                    Date = DateTime.Now.ToString(),
                    Details = "Details test 2",
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
                var date = DateTime.Now;
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);
                var request = await controller.DeleteExpenses(new List<int> { 2, 23 });

                repository.Verify(r => r.DeleteExpenses(new List<int> { 2, 23 }));
                Assert.That(request, Is.TypeOf<OkObjectResult>());
            }
        }

        [Test]
        public async Task DeleteExpensesReturnsBadRequestOnException()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var date = DateTime.Now;
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.DeleteExpenses(It.IsAny<List<int>>())).Throws<ArgumentNullException>();

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);
                var request = await controller.DeleteExpenses(new List<int> { 2, 23 });

                repository.Verify(r => r.DeleteExpenses(new List<int> { 2, 23 }));
                Assert.That(request, Is.TypeOf<BadRequestObjectResult>());
            }
        }


        [Test]
        public async Task DeleteExpenseReturnsOK()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var request = await controller.DeleteExpense(75);

                repository.Verify(r => r.DeleteExpense(75));
                Assert.That(request, Is.TypeOf<OkObjectResult>());
            }
        }

        [Test]
        public async Task DeleteExpenseReturnsError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.DeleteExpense(75)).Throws<ArgumentOutOfRangeException>();

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var request = await controller.DeleteExpense(75);

                repository.Verify(r => r.DeleteExpense(75));
                Assert.That(request, Is.TypeOf<NotFoundObjectResult>());
            }
        }

        [Test]
        public async Task UpdateExpenseReturnsOK()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => new User { FirstName = "Zenek" }));
                repository.Setup(r => r.GetExpenseAsync(54)).Returns(Task.Run(() => new Expense { Id = 54, CategoryId = 22, Comment = "New comment" }));

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);

                var result = await controller.UpdateExpense(54, new ExpenseResourceBase
                {
                    CategoryId = 22,
                    Comment = "New comment",
                    Date = "2018-03-04",
                    Details = "Details",
                    Value = 124
                });
                var okUpdateResult = result as OkObjectResult;
                var expense = okUpdateResult.Value as ExpenseResource;

                repository.Verify(r => r.UpdateExpenseAsync(54, It.IsAny<Expense>()));
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                Assert.That(expense.Comment, Is.EqualTo("New comment"));
            }
        }

        [Test]
        public async Task UpdateExpenseReturnsErrorOnNonExistentExpense()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                repository.Setup(r => r.UpdateExpenseAsync(23, It.IsAny<Expense>())).Throws<ArgumentOutOfRangeException>();

                var controller = new ExpenseController(repository.Object, userRepository.Object, mapper, unitOfWork.Object, httpContextAccessor.Object);


                var result = await controller.UpdateExpense(23, new ExpenseResourceBase
                {
                    CategoryId = 54,
                    Comment = "New comment",
                    Date = "2018-03-04",
                    Details = "Details",
                    Value = 454
                });

                repository.Verify(r => r.UpdateExpenseAsync(23, It.IsAny<Expense>()));
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
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
