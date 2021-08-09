using Blazored.LocalStorage;
using DataAccessLibrary.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LeetaWebBlazor
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {

        private const string USER_SESSION_OBJECT_KEY = "user_session_obj";

        private ILocalStorageService protectedSessionStore;
        private IHttpContextAccessor httpContextAccessor;

        public CustomAuthStateProvider(ILocalStorageService protectedSessionStore, IHttpContextAccessor httpContextAccessor) =>
            (this.protectedSessionStore, this.httpContextAccessor) = (protectedSessionStore, httpContextAccessor);

        public string IpAddress => httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;

        private UserModel User { get; set; }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            UserModel userSession = await GetUserSession();

            if (userSession != null)
                return await GenerateAuthenticationState(userSession);
            return await GenerateEmptyAuthenticationState();
        }

        public async Task LoginAsync(UserModel user)
        {
            //await SetUserSession(user);

            NotifyAuthenticationStateChanged(GenerateAuthenticationState(user));
        }

        public async Task LogoutAsync()
        {
            //await SetUserSession(null);

            NotifyAuthenticationStateChanged(GenerateEmptyAuthenticationState());
        }

        public async Task<UserModel> GetUserSession()
        {
            if (User != null)
                return User;

            var localUserJson = await protectedSessionStore.GetItemAsync<string>(USER_SESSION_OBJECT_KEY);

            if (localUserJson == null)
                return null;

            if (string.IsNullOrEmpty(localUserJson.ToString()))
                return null;

            try
            {
                return RefreshUserSession(JsonConvert.DeserializeObject<UserModel>(localUserJson.ToString()));
            }
            catch
            {
                await LogoutAsync();
                return null;
            }
        }

        private async Task SetUserSession(UserModel user)
        {
            RefreshUserSession(user);

            await protectedSessionStore.SetItemAsync(USER_SESSION_OBJECT_KEY, JsonConvert.SerializeObject(user));
        }

        private UserModel RefreshUserSession(UserModel user) => User = user;

        private Task<AuthenticationState> GenerateAuthenticationState(UserModel user)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.user_name.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Role, user.role.ToString())
            }, "apiauth_type");

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            return Task.FromResult(new AuthenticationState(claimsPrincipal));
        }

        private Task<AuthenticationState> GenerateEmptyAuthenticationState() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
    }
}
