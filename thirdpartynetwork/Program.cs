using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using thirdpartynetwork;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Application.Helpers;
using thirdpartynetwork.Core.Data;
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
    o.Events = new JwtBearerEvents
    {
        OnForbidden = context => ErrorResponseWriter.WriteJsonError(context.HttpContext, 403,
            "You are not authorized to access this resource."),

        OnChallenge = context =>
        {
            Console.WriteLine("OnChallenge invoked");

            // 1. Check for the specific exception that caused the failure
            if (context.AuthenticateFailure != null)
            {
                Console.WriteLine($"JWT Validation Failed: {context.AuthenticateFailure.Message}");

                // If you need the full stack trace, use:
                // Console.WriteLine(context.AuthenticateFailure.ToString());
            }
            else
            {
                // If it's null, it usually means no token was provided in the header at all,
                // or the header was malformed (e.g., missing the 'Bearer ' prefix).
                Console.WriteLine("JWT Validation Failed: No token provided or token was completely unreadable.");
            }

            // 2. Check for standard OAuth error details (sometimes populated by the middleware)
            if (!string.IsNullOrEmpty(context.Error) || !string.IsNullOrEmpty(context.ErrorDescription))
            {
                Console.WriteLine($"OAuth Error: {context.Error} - {context.ErrorDescription}");
            }

            context.HandleResponse();
            return ErrorResponseWriter.WriteJsonError(context.HttpContext, 401,
                "Authentication is required to access this resource :).");
        }
    };
});

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IHttpContextAccessor, IDynamicAudienceCache>((options, httpContextAccessor, audienceCache) =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = config["JwtSettings:Issuer"],
            AudienceValidator = (tokenAudiences, securityToken, validationParameters) =>
            {
                var enumerable = tokenAudiences as string[] ?? tokenAudiences.ToArray();
                if (enumerable.Length == 0) 
                {
                    return false;
                }

                var defaultAudience = config["JwtSettings:Audience"];

                foreach (var aud in enumerable)
                {
                    if (string.Equals(aud, defaultAudience, StringComparison.OrdinalIgnoreCase)) 
                    {
                        return true;
                    }

                    if (audienceCache.IsValidAudience(aud)) 
                    {
                        return true;
                    }
                }

                Console.WriteLine("No token audiences matched the valid list.");
                return false;
            },
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("TOKEN_SECRET")!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddCors();

builder.Services.AddOptions<CorsOptions>()
    .Configure<ICorsOriginCache>((options, originCache) =>
    {
        options.AddPolicy("DynamicCorsPolicy", builderContext =>
        {
            builderContext
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowed(originCache.IsOriginAllowed);
        });
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.MaxDepth = 128; // Give it some breathing room
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 128;
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 128;
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.MaxDepth = 128;
});

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        // This is a "Hammer" fix: it tells the generator to stop 
        // recursing if the schema gets too complex.
        if (context.JsonTypeInfo.Type.Name.EndsWith("Repository") ||
            context.JsonTypeInfo.Type.Name.Contains("Proxy"))
        {
            // Just treat these as objects instead of crawling them
            schema.Properties.Clear();
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddTransient<DatabaseSeeder>();

// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(cfg => { cfg.LicenseKey = Environment.GetEnvironmentVariable("LUCKY_PENNY_LICENSE"); },
    typeof(Program).Assembly, typeof(UserService).Assembly, typeof(Program).Assembly);

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
    options.MultipartBodyLengthLimit = 52428800; // 50 MB
});

// Add Singletons
builder.Services.AddSingleton<IDynamicAudienceCache, DynamicAudienceCache>();
builder.Services.AddSingleton<ICDNService, CloudinaryService>();
builder.Services.AddSingleton<ICorsOriginCache, CorsOriginCache>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicPermissionPolicyProvider>();

// Add Repositories
builder.Services.AddScoped<IAccessRepository, AccessRepository>();
builder.Services.AddScoped<ICustomPageRepository, CustomPageRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<INetworkAccessRepository, NetworkAccessRepository>();
builder.Services.AddScoped<INetworkRepository, NetworkRepository>();
builder.Services.AddScoped<INetworkEntitlementRepository, NetworkEntitlementRepository>();
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
builder.Services.AddScoped<IAccessLevelVerifier, AccessLevelVerifier>();
builder.Services.AddSingleton<WebsocketHandler>();

// Add Services
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomPageService, CustomPageService>();
builder.Services.AddScoped<IEntitlementService, EntitlementService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IBlogService, BlogService>();
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
builder.Services.AddScoped<IWebsocketService, WebsocketService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    // This is where you add your JWT Security Definition now
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Third Party Network API";
        document.Info.Version = "v0.8.11";

        // Add Bearer Security Scheme
        var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Please provide a valid JWT token."
        };

        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        return Task.CompletedTask;
    });
});

// builder.Services.AddSwaggerGen();
// builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
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

if (true || app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    var audienceCache = scope.ServiceProvider.GetRequiredService<IDynamicAudienceCache>();

    var redirectUris = await context.Networks
        .Select(n => n.RedirectURI)
        .ToListAsync();

    var defaultAudience = config["JwtSettings:Audience"];
    if (!string.IsNullOrEmpty(defaultAudience))
    {
        redirectUris.Add(defaultAudience);
    }

    audienceCache.ReloadAudiences(redirectUris);
}

app.UseCors("DynamicCorsPolicy");

app.UseWebSockets();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();