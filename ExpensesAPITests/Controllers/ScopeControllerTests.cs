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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAPITests.Controllers
{
    [TestFixture]
    class ScopeControllerTests
    {
        [Test]
        public async Task GetScopesReturn2Scopes()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.GetScopes();
                var okResult = result as OkObjectResult;
                var scopes = okResult.Value as List<ScopeResource>;

                Assert.AreEqual(3, scopes.Count);
            }
        }

        [Test]
        public async Task GetScopesReturnErrorForNoUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds, noUser: true))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.GetScopes();
                var notFoundResult = result as NotFoundObjectResult;
                Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task GetScopeReturnOK()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);
                var scope = context.Scopes.FirstOrDefault(s => s.Name == "Test");
                var result = await controller.GetScope(scope.Id);
                var okResult = result as OkObjectResult;
                var scopeResult = okResult.Value as ScopeResource;

                Assert.AreEqual(scope.Id, scopeResult.Id);
                Assert.AreEqual("Test", scopeResult.Name);
                Assert.AreEqual(4, scopeResult.Categories.Count);
            }
        }


        [Test]
        public async Task GetScopeReturnsNotFoundOnId0()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.GetScope(0);
                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Nie znaleziono żądanego zeszytu.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task GetScopeReturnsForbiddenOnWrongOwner()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test3");
                var result = await controller.GetScope(scope.Id);
                var forbidResult = result as ForbidResult;

                Assert.NotNull(forbidResult);
            }
        }


        [Test]
        public async Task GetScopeReturnsNotFoundOnForNoUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds, noUser: true))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.GetScope(scopeIds.Last());
                var notFoundResult = result as NotFoundObjectResult;
                Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task CreateScopeReturnsOK()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.CreateScope(new ScopeResource { Name = "NewScope" });
                var okResult = result as OkObjectResult;
                var scope = okResult.Value as Scope;

                var scopeResult = await controller.GetScopes();
                var okScopesResult = scopeResult as OkObjectResult;
                var scopes = okScopesResult.Value as List<ScopeResource>;

                Assert.AreEqual("NewScope", scope.Name);
                Assert.AreEqual(4, scopes.Count);
            }
        }


        [Test]
        public async Task CreateScopeReturnsBadRequestOnExistingName()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.CreateScope(new ScopeResource { Name = "Test" });
                var badRequestResult = result as BadRequestObjectResult;

                Assert.AreEqual("Zeszyt o podanej nazwie już istnieje.", badRequestResult.Value.ToString());
            }
        }

        [Test]
        public async Task CreateScopeReturnsOKOnExistingNameOnDifferentOwner()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.CreateScope(new ScopeResource { Name = "Test3" });
                var okResult = result as OkObjectResult;
                var scope = okResult.Value as Scope;

                var scopeResult = await controller.GetScopes();
                var okScopesResult = scopeResult as OkObjectResult;
                var scopes = okScopesResult.Value as List<ScopeResource>;

                Assert.AreEqual("Test3", scope.Name);
                Assert.AreEqual(4, scopes.Count);
            }
        }

        [Test]
        public async Task CreateScopeReturnsBadRequestOnValidationError()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                controller.ModelState.AddModelError("Test", "Test value");
                var result = await controller.CreateScope(new ScopeResource { Name = "NewScope" });

                var badRequestResult = result as BadRequestObjectResult;
                var error = badRequestResult.Value as SerializableError;
                var errorList = error["Test"] as string[];
                Assert.IsInstanceOf<BadRequestObjectResult>(result);
                Assert.AreEqual("Test", error.Keys.ToList()[0]);
                Assert.AreEqual("Test value", errorList[0]);
            }
        }

        [Test]
        public async Task CreateScopeReturnsOKAndSetsSelectedScopeForUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds, noScope: true))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var user = context.Users.Include(u => u.SelectedScope).FirstOrDefault(u => u.FirstName == "Zenek");
                Assert.IsNull(user.SelectedScope);

                var result = await controller.CreateScope(new ScopeResource { Name = "NewlyCreatedScope" });
                var userAfterAddingScope = context.Users.Include(u => u.SelectedScope).FirstOrDefault(u => u.FirstName == "Zenek");

                Assert.IsNotNull(userAfterAddingScope.SelectedScope);
                Assert.AreEqual("NewlyCreatedScope", userAfterAddingScope.SelectedScope.Name);

            }
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsOK()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");
                var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.RemoveUserFromScope(scope.Id, user.Id);
                var okResult = result as OkResult;

                var scopeAfterDeletion = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");

                Assert.IsNotNull(okResult);
                Assert.AreEqual(0, scopeAfterDeletion.ScopeUsers.Count);
            }

        }

        [Test]
        public async Task RemoveUserFromScopeReturnsNotFoundOnNoUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds, noUser: true))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");
                var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.RemoveUserFromScope(scope.Id, "1");
                var notFoundResult = result as NotFoundObjectResult;

                var scopeAfterDeletion = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");

                Assert.IsNotNull(notFoundResult);
                Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsNotFoundOnNonExistentUserId()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");
                var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.RemoveUserFromScope(scope.Id, "1");
                var notFoundResult = result as NotFoundObjectResult;

                var scopeAfterDeletion = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");

                Assert.IsNotNull(notFoundResult);
                Assert.AreEqual("Nie znaleziono użytkownika o podanym Id w danym zeszycie.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsBadRequestWrongUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test3");
                var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.RemoveUserFromScope(scope.Id, user.Id);
                var badRequestResult = result as BadRequestObjectResult;
                var scopeAfterDeletion = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");

                Assert.IsNotNull(badRequestResult);
                Assert.AreEqual("Zeszyt nie należy do aktualnie zalogowanego użytkownika.", badRequestResult.Value.ToString());
            }
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsNotFoundOnNonExistentScopeId()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");
                var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.RemoveUserFromScope(0, user.Id);
                var notFoundResult = result as NotFoundObjectResult;

                var scopeAfterDeletion = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");

                Assert.IsNotNull(notFoundResult);
                Assert.AreEqual("Nie znaleziono danego zeszytu.", notFoundResult.Value.ToString());
            }
        }

        [Test]
        public async Task DeleteScopeReturnsOK()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.DeleteScope(scopeIds[3]);
                var okResult = result as OkObjectResult;

                Assert.AreEqual(scopeIds[3], okResult.Value);
            }
        }

        [Test]
        public async Task DeleteScopeReturnsBadRequestForScopeContainingExpenses()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.DeleteScope(scopeIds[1]);
                var badRequestResult = result as BadRequestObjectResult;

                Assert.AreEqual("Do zeszytu: Test2 są przypisane inne elementy. Nie można usunąć", badRequestResult.Value);
            }
        }

        [Test]
        public async Task DeleteScopeReturnsnotFoundForNotExistingScope()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.DeleteScope(0);
                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Nie znaleziono zeszytu. Mógł zostać wcześniej usunięty", notFoundResult.Value);
            }
        }

        [Test]
        public async Task DeleteScopeReturnsBadRequestWrongUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);
                
                var scope = context.Scopes.FirstOrDefault(s => s.Name == "Test3");
                
                var result = await controller.DeleteScope(scope.Id);
                var badRequestResult = result as BadRequestObjectResult;

                Assert.AreEqual("Zeszyt nie należy do aktualnie zalogowanego użytkownika.", badRequestResult.Value);
            }
        }

        [Test]
        public async Task UpdateScopeReturnsOK()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");
                var result = await controller.UpdateScope(scope.Id, new ScopeResource { Name = "NewName" });
                var okResult = result as OkObjectResult;
                var scopeResult = okResult.Value as Scope;

                Assert.AreEqual("NewName", scopeResult.Name);
            }
        }

        [Test]
        public async Task UpdateScopeReturnsBadRequestOnValidationError()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                controller.ModelState.AddModelError("Test", "Test value");
                var result = await controller.UpdateScope(scopeIds[0], new ScopeResource { Name = "NewName" });

                var badRequestResult = result as BadRequestObjectResult;
                var error = badRequestResult.Value as SerializableError;
                var errorList = error["Test"] as string[];
                Assert.IsInstanceOf<BadRequestObjectResult>(result);
                Assert.AreEqual("Test", error.Keys.ToList()[0]);
                Assert.AreEqual("Test value", errorList[0]);
            }
        }

        [Test]
        public async Task UpdateScopeReturnsNotFoundForNotExistentScopeId()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var result = await controller.UpdateScope(0, new ScopeResource { Name = "NewName" });
                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Żądany zeszyt nie istnieje.", notFoundResult.Value);
            }
        }


        [Test]
        public async Task UpdateScopeReturnsBadRequestWrongUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var scope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test3");
                var result = await controller.UpdateScope(scope.Id, new ScopeResource { Name = "NewName" });
                var badRequestResult = result as BadRequestObjectResult;

                Assert.AreEqual("Zeszyt nie należy do aktualnie zalogowanego użytkownika.", badRequestResult.Value);
            }
        }

        [Test]
        public async Task AddUserToScopeReturnsOK()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.AddUserToScope(scopeIds[1], user.Id);

                var okResult = result as OkResult;

                Assert.IsNotNull(okResult);
            }
        }

        [Test]
        public async Task AddUserToScopeReturnsNotFoundForNoUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds, noUser: true))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                //var user = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");

                var result = await controller.AddUserToScope(scopeIds[1], "0");

                var notFoundResult = result as NotFoundObjectResult;

                Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value);
            }
        }

        [Test]
        public async Task AddUserToScopeReturnsBadRequestOnWrongUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var user = context.Users.FirstOrDefault(u => u.FirstName == "Krzysiek");

                var scope = context.Scopes.FirstOrDefault(s => s.Name == "Test3");

                var result = await controller.AddUserToScope(scope.Id, user.Id);

                var notFoundResult = result as NotFoundObjectResult;

                Assert.IsNotNull(notFoundResult);
            }
        }


        [Test]
        public async Task AddUserToScopeReturnsBadRequestOnNotExistentUser()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                //var user = context.Users.FirstOrDefault(u => u.FirstName == "Krzysiek");

                var result = await controller.AddUserToScope(scopeIds[2], "0");

                var badRequestResult = result as BadRequestObjectResult;

                Assert.IsNotNull(badRequestResult);
            }
        }


        [Test]
        public async Task AddUserToScopeReturnsNotFoundForNoScope()
        {
            List<int> scopeIds;
            using (var context = GetContextWithData(out scopeIds))
            {
                var userRepository = new UserRepository(context);
                var scopeRepository = new ScopeRepository(context);
                var unitOfWork = new EFUnitOfWork(context);
                var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));

                var controller = new ScopeController(scopeRepository, unitOfWork, new FakeHttpContextAccesor(context), userRepository, mapper);

                var user = context.Users.FirstOrDefault(u => u.FirstName == "Krzysiek");

                var result = await controller.AddUserToScope(0, user.Id);

                var notFoundResult = result as NotFoundObjectResult;

                Assert.IsNotNull(notFoundResult);
                Assert.AreEqual("Nie znaleziono zeszytu.", notFoundResult.Value);
            }
        }

        private MainDbContext GetContextWithData(out List<int> scopeIds, bool noScope = false, bool noUser = false)
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
                context.Users.Add(new User { FirstName = "Wojtek" });
                context.Users.Add(new User { FirstName = "Krzysiek" });
                context.SaveChanges();
            }

            var defaultUser = context.Users.FirstOrDefault();

            context.Scopes.Add(new Scope { Name = "Test", Owner = noScope ? null : defaultUser });
            context.Scopes.Add(new Scope { Name = "Test2", Owner = noScope ? null : defaultUser });
            context.Scopes.Add(new Scope { Name = "Test3", Owner = noScope ? null : context.Users.FirstOrDefault(u => u.FirstName == "Wojtek") });
            context.Scopes.Add(new Scope { Name = "Test4", Owner = noScope ? null : defaultUser });
            context.SaveChanges();

            scopeIds = context.Scopes.Select(s => s.Id).ToList();

            var selectedScopeId = context.Scopes.First(s => s.Name == "Test").Id;
            if (!noScope)
            {
                var user = context.Users.FirstOrDefault();
                if (user != null)
                {
                    user.SelectedScope = context.Scopes.First(s => s.Name == "Test");
                    selectedScopeId = user.SelectedScope.Id;
                }
            }

            var firstScope = context.Scopes.Include(s => s.ScopeUsers).FirstOrDefault(s => s.Name == "Test");
            var lastUser = context.Users.FirstOrDefault(u => u.FirstName == "Wojtek");
            if (firstScope != null && lastUser != null)
            {
                firstScope.ScopeUsers.Add(new ScopeUser { Scope = firstScope, User = lastUser });
                context.SaveChanges();
            }

            context.Add(new Category { Name = "Category1", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category2", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category3", ScopeId = selectedScopeId });
            context.Add(new Category { Name = "Category4", ScopeId = selectedScopeId });

            context.SaveChanges();

            var category = context.Categories.FirstOrDefault(c => c.Name == "Category1");

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
