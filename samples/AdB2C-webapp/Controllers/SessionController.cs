﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nubizsoft.AspNetCore.Authentication.AzureADB2C;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features.Authentication;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AdB2C_webapp.Controllers
{
	public class SessionController : Controller
	{
		public SessionController(IOptions<AzureAdB2COptions> b2cOptions)
		{
			AzureAdB2COptions = b2cOptions.Value;
		}

		public AzureAdB2COptions AzureAdB2COptions { get; set; }

		[HttpGet]
		public async Task SignIn()
		{
			await HttpContext.Authentication.ChallengeAsync(
				OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
		}

		[HttpGet]
		public async Task ResetPassword()
		{
			var properties = new AuthenticationProperties() { RedirectUri = "/" };
			properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = AzureAdB2COptions.ResetPasswordPolicyId;
			await HttpContext.Authentication.ChallengeAsync(
				OpenIdConnectDefaults.AuthenticationScheme, properties, ChallengeBehavior.Unauthorized);
		}

		[HttpGet]
		public async Task EditProfile()
		{
			var properties = new AuthenticationProperties() { RedirectUri = "/" };
			properties.Items[AzureAdB2COptions.PolicyAuthenticationProperty] = AzureAdB2COptions.EditProfilePolicyId;
			await HttpContext.Authentication.ChallengeAsync(
				OpenIdConnectDefaults.AuthenticationScheme, properties, ChallengeBehavior.Unauthorized);
		}

		[HttpGet]
		public IActionResult SignOut()
		{
			return SignOut(
				new AuthenticationProperties { RedirectUri = Url.Action("SignedOut") },
				CookieAuthenticationDefaults.AuthenticationScheme,
				OpenIdConnectDefaults.AuthenticationScheme);
		}

		[HttpGet]
		public IActionResult SignedOut()
		{
			return View();
		}
	}
}
