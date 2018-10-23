using System.Security.Claims;
using System.Threading.Tasks;

namespace DuploAuth.Auth
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string email, string role, ClaimsIdentity identity);

        ClaimsIdentity GenerateClaimsIdentity(string userName, string email, string id, string Role);
    }
}