using AutoMapper;
using ExpensesAPI.Controllers;
using ExpensesAPI.Mapping;
using ExpensesAPI.Models;
using ExpensesAPI.Persistence;
using ExpensesAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    class CategoryControllerTests
    {
        [Test]
        public async Task GetCategoriesReturns4Categories()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var result = await controller.GetCategories(false);
                var okResult = result as OkObjectResult;
                var categories = okResult.Value as List<CategoryResource>;

                Assert.AreEqual(4, categories.Count);
            }
        }

        [Test]
        public async Task GetCategoriesReturnsNotFoundOnNoScopeSelected()
        {
            int category;
            using (var context = GetContextWithData(out category, noScope: true))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var result = await controller.GetCategories(false);
                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Brak wybranego zeszytu.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task GetCategoriesReturnsNotFoundOnNoUserSelected()
        {
            int category;
            using (var context = GetContextWithData(out category, noUser: true))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var result = await controller.GetCategories(false);
                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task CreateCategoryReturnsOk()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var result = await controller.CreateCategory(new SaveCategoryResource
                {
                    Name = "TestCategory"
                });

                var okResult = result as OkObjectResult;
                var createdCategory = okResult.Value as CategoryResource;

                var getCategoriesResult = await controller.GetCategories(false);
                var getCategoriesResultOkResult = getCategoriesResult as OkObjectResult;
                var allCategories = getCategoriesResultOkResult.Value as List<CategoryResource>;

                Assert.IsTrue(allCategories.Any(c => c.Name == "TestCategory"));
            }

        }


        [Test]
        public async Task CreateCategoryReturnsBadRequestOnExistingName()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var result = await controller.CreateCategory(new SaveCategoryResource
                {
                    Name = "Category1"
                });

                var badRequestResult = result as BadRequestObjectResult;

                Assert.AreEqual("Kategoria o podanej nazwie już istnieje.", badRequestResult.Value.ToString());
            }

        }


        [Test]
        public async Task CreateCategoryReturnsBadRequestOnValidationError()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                controller.ModelState.AddModelError("Test", "Test value");
                var result = await controller.CreateCategory(new SaveCategoryResource
                {
                    Name = "Category1"
                });

                var badRequestResult = result as BadRequestObjectResult;
                var error = badRequestResult.Value as SerializableError;
                var errorList = error["Test"] as string[];
                Assert.IsInstanceOf<BadRequestObjectResult>(result);
                Assert.AreEqual("Test", error.Keys.ToList()[0]);
                Assert.AreEqual("Test value", errorList[0]);
            }

        }

        [Test]
        public void CategoryResourceValidateReturnsOK()
        {
            var category = new SaveCategoryResource
            {
                Name = "ABC"
            };

            var results = new List<ValidationResult>();
            var isModelValid = Validator.TryValidateObject(category, new System.ComponentModel.DataAnnotations.ValidationContext(category), results);
            Assert.IsTrue(isModelValid);
        }

        [Test]
        public void CategoryResourceValidateReturnsErrorForNoName()
        {
            var category = new SaveCategoryResource
            {
                Name = ""
            };

            var results = new List<ValidationResult>();
            var isModelValid = Validator.TryValidateObject(category, new System.ComponentModel.DataAnnotations.ValidationContext(category), results);
            Assert.IsFalse(isModelValid);
        }

        [Test]
        public async Task DeleteCategoryReturnsOK()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var categoriesResult = await controller.GetCategories(false);

                var okCategoriesResult = categoriesResult as OkObjectResult;
                var categories = okCategoriesResult.Value as List<CategoryResource>;

                var categoryToBeDeleted = categories.FirstOrDefault(c => c.Name == "Category2");
                var deleteResult = await controller.DeleteCategory(categoryToBeDeleted.Id);
                var okDeleteResult = deleteResult as OkObjectResult;

                var result = await controller.GetCategories(false);
                var okResult = result as OkObjectResult;
                var categoriesAfterDeletion = okResult.Value as List<CategoryResource>;

                Assert.AreEqual(4, categories.Count);
                Assert.AreEqual(3, categoriesAfterDeletion.Count);
                Assert.IsNotNull(okDeleteResult);
            }

        }

        [Test]
        public async Task DeleteCategoryReturnsBadRequestOnExistingExpenses()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var categoriesResult = await controller.GetCategories(false);

                var okCategoriesResult = categoriesResult as OkObjectResult;
                var categories = okCategoriesResult.Value as List<CategoryResource>;

                var categoryToBeDeleted = categories.FirstOrDefault(c => c.Name == "Category1");
                var deleteResult = await controller.DeleteCategory(categoryToBeDeleted.Id);
                var badRequestDeleteResult = deleteResult as BadRequestObjectResult;

                var result = await controller.GetCategories(false);
                var okResult = result as OkObjectResult;
                var categoriesAfterDeletion = okResult.Value as List<CategoryResource>;

                Assert.AreEqual(4, categories.Count);
                Assert.AreEqual(4, categoriesAfterDeletion.Count);
                Assert.IsNotNull(badRequestDeleteResult);
                Assert.AreEqual("Do kategorii \"Category1\" są przyporządkowane wydatki. Zmień ich kategorię lub usuń je przed usunięciem kategorii.", badRequestDeleteResult.Value.ToString());
            }

        }


        [Test]
        public async Task DeleteCategoryReturnsNotFoundOnNotExistingCategory()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var categoriesResult = await controller.GetCategories(false);

                var okCategoriesResult = categoriesResult as OkObjectResult;
                var categories = okCategoriesResult.Value as List<CategoryResource>;

                var deleteResult = await controller.DeleteCategory(0);
                var notFoundDeleteResult = deleteResult as NotFoundObjectResult;

                var result = await controller.GetCategories(false);
                var okResult = result as OkObjectResult;
                var categoriesAfterDeletion = okResult.Value as List<CategoryResource>;

                Assert.AreEqual(4, categories.Count);
                Assert.AreEqual(4, categoriesAfterDeletion.Count);
                Assert.IsNotNull(notFoundDeleteResult);
                Assert.AreEqual("Nie znaleziono kategorii. Mogła zostać usunięta przez innego uzytkownika.", notFoundDeleteResult.Value.ToString());
            }

        }

        [Test]
        public async Task UpdateCategoryReturnsOK()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var categoriesResult = await controller.GetCategories(false);

                var okCategoriesResult = categoriesResult as OkObjectResult;
                var categoryToBeUpdated = (okCategoriesResult.Value as List<CategoryResource>).FirstOrDefault(c => c.Name == "Category3");

                var result = await controller.UpdateCategory(categoryToBeUpdated.Id, new SaveCategoryResource { Name = "NewName" });

                var okResult = result as OkObjectResult;
                var categoryAfterChange = okResult.Value as CategoryResource;

                var categoriesResultAfterUpdate = await controller.GetCategories(false);
                var okCategoriesResultAfterUpdate = categoriesResultAfterUpdate as OkObjectResult;
                var categoriesAfterUpdate = okCategoriesResultAfterUpdate.Value as List<CategoryResource>;

                Assert.AreEqual("NewName", categoryAfterChange.Name);
                Assert.NotNull(categoriesAfterUpdate.FirstOrDefault(c => c.Name == "NewName"));
                Assert.AreEqual(categoryToBeUpdated.Id, categoriesAfterUpdate.FirstOrDefault(c => c.Name == "NewName").Id);
            }
        }

        [Test]
        public async Task UpdateCategoryReturnsErrorOnNonExistentCategory()
        {
            int category;
            using (var context = GetContextWithData(out category))
            {
                var repository = new CategoryRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var userRepository = new UserRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new CategoryController(repository, scopeRepository, userRepository, mapper, unitOfWork, new FakeHttpContextAccesor(context));

                var result = await controller.UpdateCategory(0, new SaveCategoryResource { Name = "NewName" });
                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Żądana kategoria nie istnieje.", notFoundResult.Value.ToString());
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

            var category = context.Categories.FirstOrDefault(c => c.Name == "Category1");
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
