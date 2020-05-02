using ExpensesAPI.IdentityProvider.Data;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExpensesAPI.IdentityProvider
{
    public class IdentityProfileService : IProfileService
    {
        private readonly UserManager<User> userManager;
        private readonly IUserClaimsPrincipalFactory<User> claimsPrincipalFactory;

        public IdentityProfileService(UserManager<User> userManager, IUserClaimsPrincipalFactory<User> claimsPrincipalFactory)
        {
            this.userManager = userManager;
            this.claimsPrincipalFactory = claimsPrincipalFactory;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await userManager.FindByIdAsync(sub);
            var principal = await claimsPrincipalFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}
