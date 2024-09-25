using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetKubernetes.Middleware;
using NetKubernetes.Models;
using NetKubernetes.Token;

namespace NetKubernetes.Data.Inmuebles;

public class InmuebleRepository : IInmuebleRepository
{
    private readonly AppDbContext _contexto;
    private readonly IUsuarioSession _usuarioSession;
    private readonly UserManager<Usuario> _userManager;

    public InmuebleRepository(AppDbContext contexto, IUsuarioSession session, UserManager<Usuario> manager)
    {
        _contexto = contexto;
        _usuarioSession = session;
        _userManager = manager;
    }
    public async Task CreateInmueble(Inmueble inmueble)
    {
        var usuario = await _userManager.FindByNameAsync(_usuarioSession.GetUserSession());
        if (usuario is null)
        {
            throw new MiddlewareException(
                        System.Net.HttpStatusCode.Unauthorized,
                        new { mensaje = "El usuario no es valido para hacer esta inserción" }
                        );
        }

        if (inmueble is null)
        {
            throw new MiddlewareException(
                System.Net.HttpStatusCode.BadRequest,
                new { mensaje = "Los datos del inmueble son incorrectos" }
                );
        }

        inmueble.FechaCreacion = DateTime.Now;
        inmueble.UsuarioId = Guid.Parse(usuario!.Id);

        await _contexto.Inmuebles!.AddAsync(inmueble);
    }

    public async Task DeleteInmueble(int id)
    {
        var inmueble = await _contexto.Inmuebles!.FirstOrDefaultAsync(x => x.Id == id);
        _contexto.Inmuebles!.Remove(inmueble!);
    }

    public async Task<IEnumerable<Inmueble>> GetAllInmuebles() => await _contexto.Inmuebles!.ToListAsync();

    public async Task<Inmueble> GetInmuebleById(int id) => (await _contexto.Inmuebles!.FirstOrDefaultAsync(x => x.Id == id))!;

    public async Task<bool> SaveChanges() => (await _contexto.SaveChangesAsync()) >= 0;
}