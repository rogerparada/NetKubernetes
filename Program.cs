using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetKubernetes.Data;
using NetKubernetes.Data.Inmuebles;
using NetKubernetes.Data.Usuarios;
using NetKubernetes.Middleware;
using NetKubernetes.Models;
using NetKubernetes.Profiles;
using NetKubernetes.Token;

var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddDbContext<AppDbContext>(opt =>
// {
//     opt.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information).EnableSensitiveDataLogging();
//     opt.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"));
// });

var connectionMySqlString = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionMySqlString, ServerVersion.AutoDetect(connectionMySqlString));
});
builder.Services.AddScoped<IInmuebleRepository, InmuebleRepository>();
// Add services to the container.

builder.Services.AddControllers(opt =>
{
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opt.Filters.Add(new AuthorizeFilter(policy));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new InmuebleProfile());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var builderSecurity = builder.Services.AddIdentityCore<Usuario>();
var identityBuilder = new IdentityBuilder(builderSecurity.UserType, builder.Services);
identityBuilder.AddEntityFrameworkStores<AppDbContext>();
identityBuilder.AddSignInManager<SignInManager<Usuario>>();
builderSecurity.Services.AddSingleton<ISystemClock, SystemClock>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builderSecurity.Services.AddScoped<IUsuarioSession, UsuarioSession>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Mi palabra secreta"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                    };
                });

builder.Services.AddCors(o => o.AddPolicy("corsApp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ManagerMiddleware>();

app.UseAuthentication();
app.UseCors("corsApp");
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var ambiente = app.Services.CreateScope())
{
    var services = ambiente.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<Usuario>>();
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await LoadDatabase.InsertData(context, userManager);
    }
    catch (System.Exception ex)
    {
        var logging = services.GetRequiredService<ILogger<Program>>();
        logging.LogError(ex, "Ocurrió un error en la migración");
    }
}

app.Run();
