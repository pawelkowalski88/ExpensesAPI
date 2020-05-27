﻿using ExpensesAPI.Domain.ExternalAPIUtils;
using ExpensesAPI.Domain.Models;
using IdentityModel.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.SecurityTokenService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExpensesAPI.Domain.Persistence
{
    public class UserRepositoryExternalApi : IUserRepository<IdentityServerUser>
    {
        private MainDbContext context;
        private readonly ITokenRepository tokenRepository;
        private readonly IConfiguration configuration;

        public UserRepositoryExternalApi(MainDbContext context, ITokenRepository tokenRepository, IConfiguration configuration)
        {
            this.context = context;
            this.tokenRepository = tokenRepository;
            this.configuration = configuration;
        }
        public async Task AddUser(string id, string name)
        {
            context.Users.Add(new User
            {
                Id = id,
                UserName = name
            });

            await context.SaveChangesAsync();
        }

        public async Task<User> GetUserAsync(string id)
        {
            return await context.Users
               .Include(u => u.SelectedScope)
               .Include(u => u.OwnedScopes)
               .Include(u => u.ScopeUsers)
                   .ThenInclude(su => su.Scope)
                       .ThenInclude(s => s.Owner)
               .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUserListAsync(string query, string myId)
        {
            var tokenResponse = await DelegateAsync(tokenRepository.RetrieveToken());
            var token = tokenResponse.AccessToken;

            var apiClient = new HttpClient();

            apiClient.SetBearerToken(token);

            var url = configuration["ExpensesIdentityAPI"];

            var apiResponse = await apiClient.GetAsync($"{url}/api/account/list?query={query}");

            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new RequestFailedException("Nie udało się pobrać użytkowników.");
            }

            var responseContent = await apiResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<IdentityServerUser>>(responseContent).Cast<User>().ToList();

            return result;
        }

        public async Task<User> GetUserWithScopesAsync(string id)
        {
            return await context.Users
               .Include(u => u.SelectedScope)
               .Include(u => u.OwnedScopes)
               .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task SetSelectedScope(string userId, int scopeId)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new ArgumentOutOfRangeException("Nie znaleziono użytkownika.");

            var scope = context.Scopes.FirstOrDefault(s => s.Id == scopeId);
            if (scope == null)
                throw new ArgumentOutOfRangeException("Nie znaleziono zeszytu.");

            user.SelectedScope = scope;
        }
        public async Task<List<User>> GetUserDetails(List<string> ids)
        {
            if (ids.Count == 0)
                return new List<User>();
            var tokenResponse = await DelegateAsync(tokenRepository.RetrieveToken());
            var token = tokenResponse.AccessToken;

            var apiClient = new HttpClient();

            apiClient.SetBearerToken(token);

            var idsString = string.Join("&ids=", ids.ToArray());
            var apiResponse = await apiClient.GetAsync($"{configuration["IExpensesIdentityAPI"]}/api/account/details?ids={idsString}");

            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new RequestFailedException("Nie udało się pobrać danych.");
            }

            var responseContent = await apiResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<IdentityServerUser>>(responseContent).Cast<User>().ToList();

            return result;
        }

        private async Task<TokenResponse> DelegateAsync(string userToken)
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(configuration["IdentityProviderURL"]);
            if (disco.IsError) throw new Exception(disco.Error);

            var payload = new
            {
                token = userToken
            };

            // create token client
            var tokenClient = new HttpClient();

            return await tokenClient.RequestTokenAsync(new TokenRequest
            {
                Address = disco.TokenEndpoint,
                GrantType = "delegation",

                ClientId = "ExpensesAPIClient",
                ClientSecret = configuration["ExpensesIdentityAPIClientSecret"],

                Parameters =
                {
                    { "scope", "ExpensesIdentityServerUsersAPI" },
                    { "token", userToken }
                }
            });
        }

    }
}
