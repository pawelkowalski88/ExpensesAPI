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
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExpensesAPITests.Controllers
{
    [TestFixture]
    class ScopeControllerTests
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
                            new ClaimsIdentity(new List<Claim>{ new Claim("id", "Zenek") })
                        })
                );
        }

        [Test]
        public async Task GetScopesReturn2Scopes()
        {
            var user = new User { FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } };
            userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => user));
            scopeRepository.Setup(r => r.GetScopes(user)).Returns(Task.Run(() => new List<Scope> { new Scope { Name = "Scope1"}, new Scope { Name = "Scope2" } }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.GetScopes();
            var okResult = result as OkObjectResult;
            var scopes = okResult.Value as List<ScopeResource>;

            Assert.AreEqual(2, scopes.Count);
        }

        [Test]
        public async Task GetScopesReturnErrorForNoUser()
        {
            httpContext.Setup(c => c.User).Returns<ClaimsPrincipal>(null);
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.GetScopes();
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
        }

        [Test]
        public async Task GetScopeReturnOK()
        {
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "1234" }));
            userRepository.Setup(r => r.GetUserAsync("Zenek")).Returns(Task.Run(() => new User { Id = "1234", FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } }));
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.GetScope(15);
            var okResult = result as OkObjectResult;
            var scopeResult = okResult.Value as ScopeResource;

            scopeRepository.Verify(r => r.GetScope(15));

            Assert.AreEqual(15, scopeResult.Id);
        }


        [Test]
        public async Task GetScopeReturnsNotFoundOnId0()
        {
            scopeRepository.Setup(r => r.GetScope(0)).Returns(async () => null);
            userRepository.Setup(r => r.GetUserAsync("Zenek")).Returns(Task.Run(() => new User { Id = "1234", FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } }));
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.GetScope(0);
            var notFoundResult = result as NotFoundObjectResult;

            Assert.AreEqual("Nie znaleziono żądanego zeszytu.", notFoundResult.Value.ToString());
        }

        [Test]
        public async Task GetScopeReturnsForbiddenOnWrongOwner()
        {
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" }));
            userRepository.Setup(r => r.GetUserAsync("Zenek")).Returns(Task.Run(() => new User { Id = "1234", FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } }));
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.GetScope(15);
            var forbidResult = result as ForbidResult;

            Assert.NotNull(forbidResult);
        }


        [Test]
        public async Task GetScopeReturnsNotFoundOnForNoUser()
        {
            httpContext.Setup(c => c.User).Returns<ClaimsPrincipal>(null);
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" }));
            userRepository.Setup(r => r.GetUserAsync("Zenek")).Returns(async () => null);
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.GetScope(14);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
        }

        [Test]
        public async Task CreateScopeReturnsOK()
        {
            var user = new User { FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } };
            userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => user));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" }));
            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.CreateScope(new ScopeResource { Name = "NewScope" });

            scopeRepository.Verify(r => r.GetScope(It.IsAny<int>()));
            scopeRepository.Verify(r => r.AddScope(It.IsAny<Scope>()));
        }


        [Test]
        public async Task CreateScopeReturnsBadRequestOnExistingName()
        {
            var user = new User { Id = "4356", FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } };
            userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => user));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" }));
            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.CreateScope(new ScopeResource { Name = "Scope1" });
            var badRequestResult = result as BadRequestObjectResult;

            scopeRepository.Verify(r => r.AddScope(It.IsAny<Scope>()), Times.Never);
            Assert.AreEqual("Zeszyt o podanej nazwie już istnieje.", badRequestResult.Value.ToString());
        }

        [Test]
        public async Task CreateScopeReturnsOKOnExistingNameOnDifferentOwner()
        {
            var user = new User { Id = "43656", FirstName = "Zenek", SelectedScope = new Scope { Id = 25, Name = "Test", Owner = null } };
            userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => user));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" }));
            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));
                
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.CreateScope(new ScopeResource { Name = "Test3" });
            var okResult = result as OkObjectResult;
            var scope = okResult.Value as Scope;

            var scopeResult = await controller.GetScopes();
            var okScopesResult = scopeResult as OkObjectResult;
            var scopes = okScopesResult.Value as List<ScopeResource>;


            scopeRepository.Verify(r => r.GetScope(It.IsAny<int>()));
            scopeRepository.Verify(r => r.AddScope(It.IsAny<Scope>()));
            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateScopeReturnsBadRequestOnValidationError()
        {
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            controller.ModelState.AddModelError("Test", "Test value");
            var result = await controller.CreateScope(new ScopeResource { Name = "NewScope" });

            var badRequestResult = result as BadRequestObjectResult;
            var error = badRequestResult.Value as SerializableError;
            var errorList = error["Test"] as string[];
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.AreEqual("Test", error.Keys.ToList()[0]);
            Assert.AreEqual("Test value", errorList[0]);
        }

        [Test]
        public async Task CreateScopeReturnsOKAndSetsSelectedScopeForUser()
        {
            var user = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            userRepository.Setup(r => r.GetUserAsync(It.IsAny<string>())).Returns(Task.Run(() => user));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" }));
            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.CreateScope(new ScopeResource { Name = "NewlyCreatedScope" });
            userRepository.Verify(r => r.SetSelectedScope(It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsOK()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null }; 
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                        new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserAsync("43656")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserAsync("43656")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope 
            { 
                Id = 15, 
                Name = "Scope1", 
                OwnerId = "4356", 
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.RemoveUserFromScope(15, user1.Id);
            var okResult = result as OkResult;

            Assert.IsNotNull(okResult);
            scopeRepository.Verify(r => r.RemoveUserFromScope(It.IsAny<Scope>(), It.IsAny<ScopeUser>()));
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsNotFoundOnNoUser()
        {
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User).Returns<ClaimsPrincipal>(null); 
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.RemoveUserFromScope(15, "1");
            var notFoundResult = result as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value.ToString());
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsNotFoundOnNonExistentUserId()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                    new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserAsync("43656")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserAsync("43656")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.RemoveUserFromScope(15, "1");
            var notFoundResult = result as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Nie znaleziono użytkownika o podanym Id w danym zeszycie.", notFoundResult.Value.ToString());
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsBadRequestWrongUser()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                new ClaimsIdentity(new List<Claim>{ new Claim("id", "43656") })
                            })
                    );
            userRepository.Setup(r => r.GetUserAsync("43656")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserAsync("4356")).Returns(Task.Run(() => user2));
            userRepository.Setup(r => r.GetUserWithScopesAsync("43656")).Returns(Task.Run(() => user1));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "43656",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));
            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.RemoveUserFromScope(15, "4356");
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual("Zeszyt nie należy do aktualnie zalogowanego użytkownika.", badRequestResult.Value.ToString());
        }

        [Test]
        public async Task RemoveUserFromScopeReturnsNotFoundOnNonExistentScopeId()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                    new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserAsync("43656")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserAsync("4356")).Returns(Task.Run(() => user2));
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            scopeRepository.Setup(r => r.GetScopes()).Returns(Task.Run(() => new List<Scope> { new Scope { Id = 15, Name = "Scope1", OwnerId = "4356" } }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.RemoveUserFromScope(0, "43656");
            var notFoundResult = result as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Nie znaleziono danego zeszytu.", notFoundResult.Value.ToString());
        }

        [Test]
        public async Task DeleteScopeReturnsOK()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null }; 
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                            new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.DeleteScope(15);
            var okResult = result as OkObjectResult;

            scopeRepository.Verify(s => s.DeleteScope(It.IsAny<Scope>()));
        }

        [Test]
        public async Task DeleteScopeReturnsBadRequestForScopeContainingExpenses()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                        new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Expenses = new Collection<Expense>
                {
                    new Expense
                    {
                        Id = 24,
                        Date = DateTime.Parse("2020-03-14"),
                        Value = -234.43F,
                        Comment = "Test comment"
                    }
                },
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.DeleteScope(15);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.AreEqual("Do zeszytu: Scope1 są przypisane inne elementy. Nie można usunąć", badRequestResult.Value);
        }

        [Test]
        public async Task DeleteScopeReturnsnotFoundForNotExistingScope()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                    new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.DeleteScope(0);
            var notFoundResult = result as NotFoundObjectResult;

            Assert.AreEqual("Nie znaleziono zeszytu. Mógł zostać wcześniej usunięty", notFoundResult.Value);
        }

        [Test]
        public async Task DeleteScopeReturnsBadRequestWrongUser()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                new ClaimsIdentity(new List<Claim>{ new Claim("id", "43656") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("43656")).Returns(Task.Run(() => user1));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);
                
            var result = await controller.DeleteScope(15);
            var badRequestResult = result as BadRequestObjectResult;

            Assert.AreEqual("Zeszyt nie należy do aktualnie zalogowanego użytkownika.", badRequestResult.Value);
        }

        [Test]
        public async Task UpdateScopeReturnsOK()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                        new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.UpdateScope(15, new ScopeResource { Name = "NewName" });
            var okResult = result as OkObjectResult;
            var scopeResult = okResult.Value as Scope;

            scopeRepository.Verify(r => r.UpdateScope(It.IsAny<Scope>(), 15));
        }

        [Test]
        public async Task UpdateScopeReturnsBadRequestOnValidationError()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                        new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            controller.ModelState.AddModelError("Test", "Test value");
            var result = await controller.UpdateScope(15, new ScopeResource { Name = "NewName" });

            var badRequestResult = result as BadRequestObjectResult;
            var error = badRequestResult.Value as SerializableError;
            var errorList = error["Test"] as string[];
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            Assert.AreEqual("Test", error.Keys.ToList()[0]);
            Assert.AreEqual("Test value", errorList[0]);
        }

        [Test]
        public async Task UpdateScopeReturnsNotFoundForNotExistentScopeId()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                    new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user2));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.UpdateScope(0, new ScopeResource { Name = "NewName" });
            var notFoundResult = result as NotFoundObjectResult;

            Assert.AreEqual("Żądany zeszyt nie istnieje.", notFoundResult.Value);
        }


        [Test]
        public async Task UpdateScopeReturnsBadRequestWrongUser()
        {
            var user1 = new User { Id = "43656", FirstName = "Zenek", SelectedScope = null };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                                new ClaimsIdentity(new List<Claim>{ new Claim("id", "43656") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("43656")).Returns(Task.Run(() => user1));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.UpdateScope(15, new ScopeResource { Name = "NewName" });
            var badRequestResult = result as BadRequestObjectResult;

            Assert.AreEqual("Zeszyt nie należy do aktualnie zalogowanego użytkownika.", badRequestResult.Value);
        }

        [Test]
        public async Task AddUserToScopeReturnsOK()
        {
            var user1 = new User { 
                Id = "43656", 
                FirstName = "Zenek", 
                SelectedScope = null, 
                OwnedScopes = { 
                    new Scope { 
                        Id = 15,
                        Name = "Test",
                        OwnerId = "43656"
                    } 
                } 
            };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                            new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserAsync("4356")).Returns(Task.Run(() => user2));
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user1));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.AddUserToScope(15, "4356");

            var okResult = result as OkResult;

            Assert.IsNotNull(okResult);
        }

        [Test]
        public async Task AddUserToScopeReturnsNotFoundForNoUser()
        {
            var user1 = new User
            {
                Id = "43656",
                FirstName = "Zenek",
                SelectedScope = null,
                OwnedScopes = {
                    new Scope {
                        Id = 15,
                        Name = "Test",
                        OwnerId = "43656"
                    }
                }
            };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User).Returns<ClaimsPrincipal>(null);

            userRepository.Setup(r => r.GetUserAsync("4356")).Returns(Task.Run(() => user2));
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user1));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.AddUserToScope(15, "0");

            var notFoundResult = result as NotFoundObjectResult;

            Assert.AreEqual("Nie rozpoznano użytkownika.", notFoundResult.Value);
        }

        [Test]
        public async Task AddUserToScopeReturnsBadRequestOnNotExistentUser()
        {
            var user1 = new User
            {
                Id = "43656",
                FirstName = "Zenek",
                SelectedScope = null,
                OwnedScopes = {
                new Scope {
                    Id = 15,
                    Name = "Test",
                    OwnerId = "43656"
                }
            }
            };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                        new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user1));
            scopeRepository.Setup(r => r.GetScope(15)).Returns(Task.Run(() => new Scope
            {
                Id = 15,
                Name = "Scope1",
                OwnerId = "4356",
                Owner = user2,
                ScopeUsers = new List<ScopeUser> { new ScopeUser { ScopeId = 15, UserId = "43656" } }
            }));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.AddUserToScope(15, "1234");
            var badRequestResult = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequestResult);
        }


        [Test]
        public async Task AddUserToScopeReturnsNotFoundForNoScope()
        {
            var user1 = new User
            {
                Id = "43656",
                FirstName = "Zenek",
                SelectedScope = null
            };
            var user2 = new User { Id = "4356", FirstName = "Wojtek", SelectedScope = null };
            httpContext.Setup(c => c.User)
                    .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                            {
                    new ClaimsIdentity(new List<Claim>{ new Claim("id", "4356") })
                            })
                    );
            userRepository.Setup(r => r.GetUserWithScopesAsync("4356")).Returns(Task.Run(() => user1));
            userRepository.Setup(r => r.GetUserAsync("4356")).Returns(Task.Run(() => user2));

            var controller = new ScopeController(scopeRepository.Object, unitOfWork.Object, httpContextAccessor.Object, userRepository.Object, mapper);

            var result = await controller.AddUserToScope(15, "4356");

            var notFoundResult = result as NotFoundObjectResult;

            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual("Nie znaleziono zeszytu.", notFoundResult.Value);
        }
    }
}
