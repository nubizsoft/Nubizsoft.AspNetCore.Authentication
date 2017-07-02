﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Nubizsoft.AspNetCore.Authentication.AzureADB2C
{
	public class AzureAdB2COpenIdConnectOptionsSetup : IConfigureOptions<OpenIdConnectOptions>
	{

		public AzureAdB2COpenIdConnectOptionsSetup(IOptions<AzureAdB2COptions> b2cOptions)
		{
			AzureAdB2COptions = b2cOptions.Value;
		}

		public AzureAdB2COptions AzureAdB2COptions { get; set; }

		public void Configure(OpenIdConnectOptions options)
		{
			options.ClientId = AzureAdB2COptions.ClientId;
			options.Authority = AzureAdB2COptions.Authority;
			options.UseTokenLifetime = true;
			options.TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "name" };

			options.Events = new OpenIdConnectEvents()
			{
				OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
				OnRemoteFailure = OnRemoteFailure,
				OnAuthorizationCodeReceived = OnAuthorizationCodeReceived
			};
		}

		public virtual Task OnRedirectToIdentityProvider(RedirectContext context)
		{
			var defaultPolicy = AzureAdB2COptions.DefaultPolicy;
			if (context.Properties.Items.TryGetValue(AzureAdB2COptions.PolicyAuthenticationProperty, out var policy) &&
				!policy.Equals(defaultPolicy))
			{
				context.ProtocolMessage.Scope = OpenIdConnectScope.OpenIdProfile;
				context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
				context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.ToLower().Replace(defaultPolicy.ToLower(), policy.ToLower());
				context.Properties.Items.Remove(AzureAdB2COptions.PolicyAuthenticationProperty);
			}
			else if (!string.IsNullOrEmpty(AzureAdB2COptions.ApiScopes))
			{
				context.ProtocolMessage.Scope += $" offline_access {AzureAdB2COptions.ApiScopes}";
				context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.CodeIdToken;
			}
			return Task.FromResult(0);
		}

		public virtual Task OnRemoteFailure(FailureContext context)
		{
			context.HandleResponse();
			// Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
			// because password reset is not supported by a "sign-up or sign-in policy"
			if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
			{
				// If the user clicked the reset password link, redirect to the reset password route
				context.Response.Redirect(AzureAdB2COptions.ResetPasswordUri);
			}
			else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
			{
				context.Response.Redirect(AzureAdB2COptions.AccessDeniedUri);
			}
			else
			{
				context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
			}
			return Task.FromResult(0);
		}

		public virtual async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
		{
			// Use MSAL to swap the code for an access token
			// Extract the code from the response notification
			var code = context.ProtocolMessage.Code;

			string signedInUserID = context.Ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
			TokenCache userTokenCache = new SessionTokenCachePersistence(signedInUserID, context.HttpContext).GetUserCache();
			ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureAdB2COptions.ClientId, AzureAdB2COptions.Authority, AzureAdB2COptions.RedirectUri, new ClientCredential(AzureAdB2COptions.ClientSecret), userTokenCache, null);
			try
			{
				AuthenticationResult result = await cca.AcquireTokenByAuthorizationCodeAsync(code, AzureAdB2COptions.ApiScopes.Split(' '));


				context.HandleCodeRedemption(result.AccessToken, result.IdToken);
			}
			catch (Exception ex)
			{
				//TODO: Handle
				throw;
			}
		}
	}
}