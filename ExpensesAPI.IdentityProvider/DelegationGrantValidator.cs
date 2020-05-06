using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesAPI
{
    public class DelegationGrantValidator : IExtensionGrantValidator
    {
        private readonly ITokenValidator validator;

        public string GrantType => "delegation";

        public DelegationGrantValidator(ITokenValidator validator)
        {
            this.validator = validator;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userToken = context.Request.Raw.Get("token");

            if (string.IsNullOrEmpty(userToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            var result = await validator.ValidateAccessTokenAsync(userToken);
            if (result.IsError)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                return;
            }

            var sub = result.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            context.Result = new GrantValidationResult(sub, GrantType);
            return;
        }
    }
}
