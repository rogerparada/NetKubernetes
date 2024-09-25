using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace NetKubernetes.Token;

public class UsuarioSession : IUsuarioSession
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string GetUserSession()
    {
        var userName = _httpContextAccessor.HttpContext!.User?.Claims?
                            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        return userName!;
    }
}