using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using thirdpartynetwork;
using thirdpartynetwork.Application.Auth;
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
    var sp = builder.Services.BuildServiceProvider();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = config["JwtSettings:Issuer"],
        AudienceValidator = (audiences, securityToken, validationParameters) =>
        {
            var httpContext = httpContextAccessor.HttpContext;

            var defaultAudience = config["JwtSettings:Audience"];
            var originHeader = httpContext.Request.Headers["Origin"].FirstOrDefault();
            
            if (originHeader == defaultAudience) return true;
            
            var enumerable = audiences as string[] ?? audiences.ToArray();
            return enumerable.Contains(originHeader) || enumerable.Contains(defaultAudience);
        },
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
    o.Events = new JwtBearerEvents
    {
        OnForbidden = context => ErrorResponseWriter.WriteJsonError(context.HttpContext, 403,
            "You are not authorized to access this resource."),

        OnChallenge = context =>
        {
            context.HandleResponse(); // Prevent default
            return ErrorResponseWriter.WriteJsonError(context.HttpContext, 401, "Authentication is required to access this resource.");
        }
    };
});

// Add CORS with permissive policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
    {
        b.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        // Increased to 64. Fuck it, we ball
        options.JsonSerializerOptions.MaxDepth = 64; // Optional: Increase if necessary
    });

builder.Services.AddTransient<DatabaseSeeder>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Other
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.Configure<CloudinaryOptions>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")!;
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")!;
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")!;
    options.DefaultFolder = "DataFolder";
});
builder.Services.Configure<FormOptions>(options =>
{
    // options.MultipartBodyLengthLimit = 104857600; // 100 MB
    options.MultipartBodyLengthLimit =  52428800; // 50 MB
});
builder.Services.AddSingleton<ICDNService, CloudinaryService>();


// Add Repositories
builder.Services.AddScoped<IAccessRepository, AccessRepository>();
builder.Services.AddScoped<ICustomPageRepository, CustomPageRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<INetworkAccessRepository, NetworkAccessRepository>();
builder.Services.AddScoped<INetworkRepository, NetworkRepository>();
builder.Services.AddScoped<INetworkUserAccessRepository, NetworkUserAccessRepository>();
builder.Services.AddScoped<INetworkUserRepository, NetworkUserRepository>();
builder.Services.AddScoped<INetworkUserRoleRepository, NetworkUserRoleRepository>();
builder.Services.AddScoped<IPageBlockRepository, PageBlockRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserProxyRepository, UserProxyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Helpers
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();
builder.Services.AddScoped<IJwtHandler, JwtHandler>();
builder.Services.AddSingleton<IJtiCacheService, JtiCacheService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IIdentityVerifier, IdentityVerifier>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

// Add Services
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomPageService, CustomPageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMeService, MeService>();
builder.Services.AddScoped<INetworkAccessService, NetworkAccessService>();
builder.Services.AddScoped<INetworkService, NetworkService>();
builder.Services.AddScoped<INetworkUserAccessService, NetworkUserAccessService>();
builder.Services.AddScoped<INetworkUserService, NetworkUserService>();
builder.Services.AddScoped<INetworkUserRoleService, NetworkUserRoleService>();
builder.Services.AddScoped<IPageBlockService, PageBlockService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IUserProxyManagerService, UserProxyManagerService>();
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

// Wrap the seeding in a scope to safely handle errors
using (var scope = app.Services.CreateScope())
{
    try 
    {
        // Only run seeder if not generating migrations (optional check)
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        // Log the error but don't crash the app entirely
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        
        // In CI/Migrations context, we might expect this to fail if secrets aren't present
    }
}

// Middlewares
app.UseMiddleware<CustomExceptionMiddleware>();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
