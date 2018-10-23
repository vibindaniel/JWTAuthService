using DuploAuth.Auth;
using DuploAuth.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuploAuth.Helpers
{
    public class Tokens
    {
        public static async Task<string> GenerateJwt(ClaimsIdentity identity, IJwtFactory jwtFactory, string userName, string role, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings)
        {
            var response = new
            {
                id = identity.Claims.Single(c => c.Type == "id").Value,
                auth_token = await jwtFactory.GenerateEncodedToken(userName, role, identity),
                expires_in = (int)jwtOptions.ValidFor.TotalSeconds,
                email = identity.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value,
                name = identity.Claims.Single(c => c.Type == JwtRegisteredClaimNames.GivenName).Value
            };

            return JsonConvert.SerializeObject(response, serializerSettings);
        }
    }
}