using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI.IdentityProvider
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                //new IdentityResource
                //{
                //    Name = "expenses",
                //    DisplayName = "Expenses Profile",
                //    Emphasize = false,
                //    UserClaims = new List<string>
                //    {
                //        JwtClaimTypes.Id,
                //        JwtClaimTypes.Email
                //    }
                //}
            };

        public static IEnumerable<ApiResource> Apis =>
            new List<ApiResource>
            {
                new ApiResource("ExpensesAPI", "Expenses API"),
                new ApiResource("ExpensesIdentityServerUsersAPI", "Expenses IdentityServer Users API")
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "ExpensesSPAClient",
                    //ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    RequirePkce = true,
                    AllowAccessTokensViaBrowser = true,
                   

                    AllowedCorsOrigins = { "http://localhost:4200" },
                    RedirectUris = { "http://localhost:4200/signin-callback" },
                    PostLogoutRedirectUris = { "http://localhost:4200/signout-callback" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "ExpensesAPI"
                    },

                    AllowOfflineAccess = true
                },

                new Client
                {
                    ClientId = "ExpensesAPIClient",
                    ClientSecrets = { new Secret("D7t7r4rahf9ZyyMweEeyKazeDPOV5vca".Sha256()) },

                    AllowedGrantTypes = { "delegation" },

                    AllowedScopes = { "ExpensesIdentityServerUsersAPI" }
                }
            };
    }
}
