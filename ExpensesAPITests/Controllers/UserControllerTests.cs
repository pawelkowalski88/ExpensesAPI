using AutoMapper;
using ExpensesAPI.Controllers;
using ExpensesAPI.Mapping;
using ExpensesAPI.Models;
using ExpensesAPI.Persistence;
using ExpensesAPI.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAPITests.Controllers
{
    [TestFixture]
    class UserControllerTests
    {
        private Mock<IScopeRepository> scopeRepository;
        private Mock<IUserRepository> userRepository;
        private Mock<IUnitOfWork> unitOfWork;
        private IMapper mapper;
        private Mock<IHttpContextAccessor> httpContextAccessor;
        private Mock<HttpContext> httpContext;

        [SetUp]
        public void Setup()
        {
            scopeRepository = new Mock<IScopeRepository>();
            userRepository = new Mock<IUserRepository>();
            unitOfWork = new Mock<IUnitOfWork>();
            mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MainMappingProfile>()));
            httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContext = new Mock<HttpContext>();

            httpContextAccessor.Setup(c => c.HttpContext).Returns(httpContext.Object);
            httpContext.Setup(c => c.User)
                .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                        {
                            new ClaimsIdentity(new List<Claim>{ new Claim("id", "25") })
                        })
                );
        }

        [Test]
        public async Task GetUserDataAsyncReturnsOK()
        {
                var user1 = new User { FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } };
                userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => user1));

                var controller = new UserController(userRepository.Object, httpContextAccessor.Object, mapper, scopeRepository.Object, unitOfWork.Object);

                var result = await controller.GetUserDataAsync();
                var okResult = result as OkObjectResult;
                var user = okResult.Value as UserResource;

                Assert.AreEqual("Zenek", user.FirstName);
        }

        [Test]
        public async Task GetUserDataAsyncReturnsNotFoundOnNoUser()
        {
            httpContext.Setup(c => c.User).Returns<ClaimsPrincipal>(null);
            var controller = new UserController(userRepository.Object, httpContextAccessor.Object, mapper, scopeRepository.Object, unitOfWork.Object);

            var result = await controller.GetUserDataAsync();
            var notFoundResult = result as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult);
        }

        [Test]
        public async Task GetUserListReturnsOneUserForQuery()
        {
            userRepository.Setup(r => r.GetUserListAsync("wojt", "25")).Returns(async () => new List<User>
            {
                new User
                {
                    Id = "2",
                    FirstName = "Wojtek"
                }
            });
            var controller = new UserController(userRepository.Object, httpContextAccessor.Object, mapper, scopeRepository.Object, unitOfWork.Object);

            var result = await controller.GetUserList("wojt");
            var okResult = result as OkObjectResult;
            var results = okResult.Value as IEnumerable<UserResource>;

            Assert.AreEqual(1, results.Count());
        }

        [Test]
        public async Task GetUserListReturnsThreeUsersForQuery()
        {
            userRepository.Setup(r => r.GetUserListAsync("k", "25")).Returns(async () => new List<User>
            {
                new User
                {
                    Id = "1",
                    FirstName = "Zenek"
                },
                new User
                {
                    Id = "2",
                    FirstName = "Wojtek"
                }
            });
            var controller = new UserController(userRepository.Object, httpContextAccessor.Object, mapper, scopeRepository.Object, unitOfWork.Object);

            var result = await controller.GetUserList("k");
            var okResult = result as OkObjectResult;
            var results = okResult.Value as IEnumerable<UserResource>;

            Assert.AreEqual(2, results.Count());
        }


        [Test]
        public async Task GetUserListReturnsNoUsersForEmptyQuery()
        {
            userRepository.Setup(r => r.GetUserListAsync("", "25")).Returns(async () => new List<User> {});
            var controller = new UserController(userRepository.Object, httpContextAccessor.Object, mapper, scopeRepository.Object, unitOfWork.Object);

            var result = await controller.GetUserList("");
            var okResult = result as OkObjectResult;
            var results = okResult.Value as IEnumerable<UserResource>;

            Assert.AreEqual(0, results.Count());
        }

        [Test]
        public async Task GetUserListReturnsNoUsersForWrongQuery()
        {
            userRepository.Setup(r => r.GetUserListAsync("baba", "25")).Returns(async () => new List<User> { });
            var controller = new UserController(userRepository.Object, httpContextAccessor.Object, mapper, scopeRepository.Object, unitOfWork.Object);

            var result = await controller.GetUserList("baba");
            var okResult = result as OkObjectResult;
            var results = okResult.Value as IEnumerable<UserResource>;

            Assert.AreEqual(0, results.Count());
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
