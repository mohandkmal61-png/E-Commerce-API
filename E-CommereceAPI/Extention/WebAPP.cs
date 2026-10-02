using ECommerce.Domin.Contracte;
using ECommerce.Persistence.Data.Dbcontexts;
using Microsoft.EntityFrameworkCore;
using ECommerce.Persistence.IdentityData.DbContest;
using System;
using System.Linq;

namespace E_CommereceAPI.Extention
{
    public static class WebAPP
    {
        public static async Task <WebApplication> Migratedbasync(this WebApplication aPP)
        {
            await using var scope = aPP.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbcontext>();
            var identityDbContext = scope.ServiceProvider.GetRequiredService<StoreIdentityDbcontest>();



            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                    
                
                
                await dbContext.Database.MigrateAsync();
            }

            var pendingIdentityMigrations = await identityDbContext.Database.GetPendingMigrationsAsync();
            if (pendingIdentityMigrations.Any())
            {
                await identityDbContext.Database.ExecuteSqlRawAsync(
                    """
                    IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[Addresses]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
                       AND OBJECT_ID(N'[dbo].[UserRoles]', N'U') IS NOT NULL
                    BEGIN
                        EXEC(N'
                            IF NOT EXISTS
                            (
                                SELECT 1
                                FROM [dbo].[__EFMigrationsHistory]
                                WHERE [MigrationId] = N''20260813135739_IdentityTabelsCreate''
                            )
                            BEGIN
                                INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                                VALUES (N''20260813135739_IdentityTabelsCreate'', N''9.0.11'');
                            END;
                        ');
                    END;
                    """);

                await identityDbContext.Database.MigrateAsync();
            }


            return aPP;
        }

        public static async Task <WebApplication> SeedDataasync(this WebApplication aPP)
        {
             await using var scope = aPP.Services.CreateAsyncScope();
            
                 var dataInitilizerService = scope.ServiceProvider.GetRequiredService<IDataInitilizer>();
               await dataInitilizerService.InitilizeAsync();
            
            return aPP;
        }
    }
}
