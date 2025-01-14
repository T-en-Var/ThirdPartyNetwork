using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using thirdpartynetwork;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Application.Auth.Requirements;
using thirdpartynetwork.Application.Helpers;
using thirdpartynetwork.Core.Data;
using thirdpartynetwork.Presentation.Swagger;
using thirdpartynetwork.Application.Repositories;
using thirdpartynetwork.Application.Services;
using thirdpartynetwork.Core.Helper;
using thirdpartynetwork.Core.Interfaces.Auth;
using thirdpartynetwork.Core.Interfaces.Helpers;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

Env.Load();

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
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

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("NetworkAdminPolicy", policy =>
        policy.Requirements.Add(new NetworkAdminRequirement("networkId")));

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddTransient<DatabaseSeeder>();
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    //o.JsonSerializerOptions.Converters.Add(new NullableWithStateConverterFactory());
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Cache for JtiCacheService.
// builder.Services.AddDistributedMemoryCache();

// Other
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, NetworkAdminHandler>();
// builder.Services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();


// Add Repositories
builder.Services.AddScoped<IAccessRepository, AccessRepository>();
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<INetworkAccessRepository, NetworkAccessRepository>();
builder.Services.AddScoped<INetworkRepository, NetworkRepository>();
builder.Services.AddScoped<INetworkUserAccessRepository, NetworkUserAccessRepository>();
builder.Services.AddScoped<INetworkUserRepository, NetworkUserRepository>();
builder.Services.AddScoped<INetworkUserRoleRepository, NetworkUserRoleRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleRuleRepository, RoleRuleRepository>();
builder.Services.AddScoped<IRuleRepository, RuleRepository>();
builder.Services.AddScoped<IUserProxyRepository, UserProxyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Helpers
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IJtiCacheService, JtiCacheService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IRoleVerifier, RoleVerifier>();

// Add Services
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<INetworkAccessService, NetworkAccessService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<INetworkUserAccessService, NetworkUserAccessService>();
builder.Services.AddScoped<INetworkUserService, NetworkUserService>();
builder.Services.AddScoped<INetworkUserRoleService, NetworkUserRoleService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleRuleService, RoleRuleService>();
builder.Services.AddScoped<IRuleService, RuleService>();
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

app.Services.CreateScope().ServiceProvider
    .GetRequiredService<DatabaseSeeder>().SeedAsync().Wait();

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
