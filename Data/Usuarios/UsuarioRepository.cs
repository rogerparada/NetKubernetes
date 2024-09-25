using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NetKubernetes.Dto.UsuariosDtos;
using NetKubernetes.Middleware;
using NetKubernetes.Models;
using NetKubernetes.Token;

namespace NetKubernetes.Data.Usuarios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly AppDbContext _context;
    private readonly IUsuarioSession _usuarioSession;

    public UsuarioRepository(UserManager<Usuario> manager, SignInManager<Usuario> signManager, IJwtGenerator jwt, AppDbContext contexto, IUsuarioSession usuario)
    {
        _userManager = manager;
        _signInManager = signManager;
        _jwtGenerator = jwt;
        _context = contexto;
        _usuarioSession = usuario;
    }

    private UsuarioResponseDto TransformerUserToUserDto(Usuario usuario) =>
    new UsuarioResponseDto
    {
        Id = usuario.Id,
        Nombre = usuario.Nombre,
        Apellido = usuario.Apellido,
        Telefono = usuario.Telefono,
        Email = usuario.Email,
        UserName = usuario.UserName,
        Token = _jwtGenerator.CreateToken(usuario)
    };


    public async Task<UsuarioResponseDto> GetUsuario()
    {
        var usuario = await _userManager.FindByNameAsync(_usuarioSession.GetUserSession());

        if (usuario is null)
        {
            throw new MiddlewareException(
                System.Net.HttpStatusCode.Unauthorized,
                new { mensaje = "El usuario del token no existe en la base de datos" }
                );
        }

        return TransformerUserToUserDto(usuario!);
    }

    public async Task<UsuarioResponseDto> Login(UsuarioLoginRequestDto request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email!);

        if (usuario is null)
        {
            throw new MiddlewareException(
                System.Net.HttpStatusCode.Unauthorized,
                new { mensaje = "El email del usuario no existe en la base de datos" }
                );
        }

        var resultado = await _signInManager.CheckPasswordSignInAsync(usuario!, request.Password!, false);

        if (resultado.Succeeded)
        {
            return TransformerUserToUserDto(usuario!);
        }

        throw new MiddlewareException(System.Net.HttpStatusCode.Unauthorized, new { mensaje = "Las credenciales son incorrectas" });
    }

    public async Task<UsuarioResponseDto> RegistroUsuario(UsuarioRegistroRequestDto request)
    {
        var existeEmail = await _context.Users.Where(x => x.Email == request.Email).AnyAsync();

        if (existeEmail)
        {
            throw new MiddlewareException(
                System.Net.HttpStatusCode.BadRequest,
                new { mensaje = "El email del usuario ya existe en la base de datos" }
                );
        }

        var existeUsername = await _context.Users.Where(x => x.UserName == request.UserName).AnyAsync();

        if (existeUsername)
        {
            throw new MiddlewareException(
                System.Net.HttpStatusCode.BadRequest,
                new { mensaje = "El username del usuario ya existe en la base de datos" }
                );
        }

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            Telefono = request.Telefono,
            UserName = request.UserName
        };

        var result = await _userManager.CreateAsync(usuario!, request.Password!);
        if (result.Succeeded)
        {
            return TransformerUserToUserDto(usuario);
        }

        throw new Exception("No se pudo registrar el usuario");
    }
}