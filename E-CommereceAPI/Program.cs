
using E_CommereceAPI.Extention;
using ECommerce.Domin.Contracte;
using ECommerce.Domin.Contracts;
using ECommerce.Persistence.Data.Dbcontexts;
using ECommerce.Persistence.DataSeed;
using ECommerce.Persistence.Repositoris;
using ECommerce.Presentation.Controler;
using ECommerce.Services;
using ECommerce.Services.MapingProfile;
using ECommerce.Services.TokenService;
using ECommerce.ServicesAbstraction;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ECommerce.Domin.Entitie.Identitymodule;
using ECommerce.Persistence.IdentityData.DbContest;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;

namespace E_CommereceAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

            // Add services to the container.

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(ProductControl).Assembly);
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter the JWT token only. Swagger adds the Bearer prefix automatically."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddDbContext<StoreDbcontext>(options =>
            {
                options.UseSqlServer(defaultConnection);
            });
            // Identity persistence
            builder.Services.AddDbContext<StoreIdentityDbcontest>(options =>
            {
                options.UseSqlServer(defaultConnection);
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // You can customize password / lockout / user settings here if needed
            })
            .AddEntityFrameworkStores<StoreIdentityDbcontest>()
            .AddDefaultTokenProviders();

            var jwtSecret = builder.Configuration["JwtSettings:Secret"]
                ?? throw new InvalidOperationException("JWT secret is not configured.");
            var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is not configured.");
            var jwtAudience = builder.Configuration["JwtSettings:Audience"]
                ?? throw new InvalidOperationException("JWT audience is not configured.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
            });

            // Register Authentication Services
            builder.Services.AddScoped<TokenService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder.Services.AddScoped<IDataInitilizer, DataInitilizer>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IProductServise, ProductServise>();
            builder.Services.AddScoped<IBasketServise, BasketServise>();
            builder.Services.AddScoped<IOrderServise, OrderServise>();
            builder.Services.AddRedisBasket(builder.Configuration, builder.Environment);

            builder.Services.AddAutoMapper(x =>
            {
                x.AddProfile<ProducteProfile>();
                x.AddProfile<BasketProfile>();
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();


            #region Data Seed

            await  app.Migratedbasync();
           await app.SeedDataasync(); 


            #endregion

            // Configure the HTTP request pipeline.
            app.UseExceptionHandler();
            app.UseCustomStatusCodePages();

            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API"));

            // Redirect to HTTPS only when an HTTPS port is configured (https launch profile).
            if (!string.IsNullOrEmpty(app.Configuration["ASPNETCORE_HTTPS_PORT"]))
                app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
