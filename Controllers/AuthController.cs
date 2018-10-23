using DuploAuth.Auth;
using DuploAuth.Helpers;
using DuploAuth.Models;
using DuploAuth.Models.Entities;
using DuploAuth.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using static DuploAuth.Models.WordpressOptions;

namespace DuploAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly WordpressAuthSettings _wordpressAuthSettings;

        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private static readonly HttpClient Client = new HttpClient();

        public AuthController(
            IOptions<WordpressAuthSettings> wordpressAuthSettings,
            UserManager<AppUser> userManager,
            ApplicationDbContext appDbContext,
            IJwtFactory jwtFactory,
            IOptions<JwtIssuerOptions> jwtOptions)
        {
            _wordpressAuthSettings = wordpressAuthSettings.Value;
            _userManager = userManager;
            _appDbContext = appDbContext;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet]
        [Route("providers")]
        [AllowAnonymous]
        public IActionResult Providers()
        {
            Debug.WriteLine("test");
            var providers = new List<Provider>();
            var options = new string[] { "wordpress", "google", "microsoft" };
            foreach (var option in options)
            {
                if (AuthRepository.ValidateProviders(option))
                {
                    var provider = new Provider
                    {
                        Name = option,
                        Endpoint = AuthRepository.getAuthorizationEndpoint(option)
                    };
                    providers.Add(provider);
                }
            }

            return new OkObjectResult(providers);
        }

        [HttpGet]
        [Route("isLoggedIn")]
        [Authorize]
        public IActionResult isLoggedIn()
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;
            var isUserInDb = _userManager.FindByEmailAsync(email).Result;
            if (isUserInDb == null)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpPost]
        [Route("wordpress-login")]
        [AllowAnonymous]
        public async Task<IActionResult> Wordpress([FromBody]WordpressAuthRequest model)
        {
            // 1.generate an app access token
            var appAccessTokenRequest = new List<KeyValuePair<string, string>>();
            appAccessTokenRequest.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            appAccessTokenRequest.Add(new KeyValuePair<string, string>("code", model.AccessToken));
            appAccessTokenRequest.Add(new KeyValuePair<string, string>("redirect_uri", model.RedirectURI));
            appAccessTokenRequest.Add(new KeyValuePair<string, string>("client_id", Environment.GetEnvironmentVariable("WORDPRESS_CLIENTID")));
            appAccessTokenRequest.Add(new KeyValuePair<string, string>("client_secret", Environment.GetEnvironmentVariable("WORDPRESS_CLIENT_SECRET")));
            var appAccessTokenContent = new FormUrlEncodedContent(appAccessTokenRequest);

            var appAccessTokenResponse = await Client.PostAsync($"{Environment.GetEnvironmentVariable("WORDPRESS_ENDPOINT")}/oauth/token/", appAccessTokenContent);
            if (!appAccessTokenResponse.IsSuccessStatusCode)
            {
                return BadRequest(await appAccessTokenResponse.Content.ReadAsStringAsync());
            }
            var appAccessToken = JsonConvert.DeserializeObject<WordpressAppAccessToken>(await appAccessTokenResponse.Content.ReadAsStringAsync());

            // 2. validate the user access token
            var userAccessTokenValidationResponse = await Client.GetStringAsync($"{Environment.GetEnvironmentVariable("WORDPRESS_ENDPOINT")}/oauth/me?access_token={appAccessToken.AccessToken}");
            var userAccessTokenData = JsonConvert.DeserializeObject<WordpressUserAccessTokenData>(userAccessTokenValidationResponse);

            if (string.IsNullOrEmpty(userAccessTokenData.Email))
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid wordpress token.", ModelState));
            }

            var localUser = await _userManager.FindByNameAsync(userAccessTokenData.Email);

            if (localUser == null)
            {
                return NotFound(Errors.AddErrorToModelState("login_failure", "Failed to locate local user account.", ModelState));
            }

            if (string.IsNullOrEmpty(localUser.Name))
            {
                localUser.Name = userAccessTokenData.Name;
                var result = await _userManager.UpdateAsync(localUser);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));
                }
            }

            var jwt = await Tokens
                .GenerateJwt(
                _jwtFactory.GenerateClaimsIdentity(localUser.Name, localUser.Email, localUser.Id, localUser.Role),
                _jwtFactory,
                localUser.Email,
                localUser.Role,
                _jwtOptions,
                new JsonSerializerSettings { Formatting = Formatting.Indented });

            Debug.WriteLine(jwt);
            return new OkObjectResult(jwt);
        }
    }
}