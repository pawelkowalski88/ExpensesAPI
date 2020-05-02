using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ExpensesAPI.IdentityProvider.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4.Events;
using IdentityServer4.Extensions;

namespace ExpensesAPI.IdentityProvider.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private IIdentityServerInteractionService _interaction;
        private IEventService _events;

        public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet(string logoutId = null)
        {
            if (logoutId == null)
            {
                return null;
            }
            var logout = await _interaction.GetLogoutContextAsync(logoutId);
            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }
            return Redirect(logout?.PostLogoutRedirectUri);
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
