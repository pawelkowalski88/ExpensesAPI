using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ExpensesAPI.Domain.Models;
using AutoMapper;
using System.Threading.Tasks;
using ExpensesAPI.SimpleJWTAuth.Resources;

namespace ExpensesAPI.SimpleJWTAuth
{
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;

        public AccountController(UserManager<User> userManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
        }

        [HttpPost("/api/accounts")]
        public async Task<IActionResult> Post([FromBody]RegistrationResource model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdentity = mapper.Map<User>(model);

            var result = await userManager.CreateAsync(userIdentity, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

            return new OkObjectResult("Account created");
        }
    }

}
