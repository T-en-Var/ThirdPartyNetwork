using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using thirdpartynetwork;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Presentation.Swagger;
using thirdpartynetwork.Application.Repositories;
using thirdpartynetwork.Application.Services;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

Env.Load();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["JwtSettings:Issuer"],
        ValidAudience = config["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // options.Filters.Add<GlobalExceptionFilter>(); // Register the global exception filter here
});

builder.Services.AddTransient<Seed>();
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //o.JsonSerializerOptions.Converters.Add(new NullableWithStateConverterFactory());
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Cache for JtiCacheService.
// builder.Services.AddDistributedMemoryCache();

// Add Repositories
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<INetworkPermissionRepository, NetworkPermissionRepository>();
builder.Services.AddScoped<INetworkRoleRepository, NetworkRoleRepository>();
builder.Services.AddScoped<INetworkRepository, NetworkRepository>();
builder.Services.AddScoped<INetworkUserPermissionRepository, NetworkUserPermissionRepository>();
builder.Services.AddScoped<INetworkUserRepository, NetworkUserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserProxyRepository, UserProxyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Services
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IJtiCacheService, JtiCacheService>();

builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<INetworkPermissionService, NetworkPermissionService>();
builder.Services.AddScoped<INetworkRoleService, NetworkRoleService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<INetworkUserPermissionService, NetworkUserPermissionService>();
builder.Services.AddScoped<INetworkUserService, NetworkUserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserProxyService, UserProxyService>();
builder.Services.AddScoped<IUserService, UserService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (args.Length == 1 && args[0].ToLower() == "seeddata")
    SeedData(app);

// Middlewares
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();
return;

static void SeedData(IHost app)
{
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    if (scopedFactory == null) throw new Exception("Service Scope Factory is null");
    
    using var scope = scopedFactory.CreateScope();
    var service = scope.ServiceProvider.GetService<Seed>();
    if (service == null) throw new Exception("Service is null");
    
    service.SeedDataContext();
}
