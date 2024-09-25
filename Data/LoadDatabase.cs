using Microsoft.AspNetCore.Identity;
using NetKubernetes.Models;

namespace NetKubernetes.Data;

public class LoadDatabase
{
    public static async Task InsertData(AppDbContext context, UserManager<Usuario> usuarioManager)
    {
        if (!usuarioManager.Users.Any())
        {
            var usuario = new Usuario()
            {
                Nombre = "Roger",
                Apellido = "Parada",
                Email = "falseMail@myfalseMail.com",
                UserName = "rparada",
                Telefono = "916328932"
            };

            await usuarioManager.CreateAsync(usuario, "PasswordLargo2022!");
        }

        if (!context.Inmuebles!.Any())
        {
            context.Inmuebles!.AddRange(
                new Inmueble { Nombre = "Casa Playa", Direccion = "Av. Sol 32", Precio = 103000M, FechaCreacion = DateTime.Now },
                new Inmueble { Nombre = "Casa Invierno", Direccion = "Av. La Roca 132", Precio = 303000M, FechaCreacion = DateTime.Now }
            );
        }

        context.SaveChanges();
    }
}