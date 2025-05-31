using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;
using System.Linq;
using CryptoWalletApi.Data;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainer _dbContainer;

    public CustomWebApplicationFactory()
    {
        // Credentials only for the local PostgreSQL test container managed by Testcontainers.
        _dbContainer = new ContainerBuilder()
            .WithImage("postgres:15-alpine")
            .WithEnvironment("POSTGRES_USER", "testuser")
            .WithEnvironment("POSTGRES_PASSWORD", "testpassword")
            .WithEnvironment("POSTGRES_DB", "cryptotestdb_tests")
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public string GetDbConnectionString() =>
        $"Host={_dbContainer.Hostname};Port={_dbContainer.GetMappedPublicPort(5432)};Database=cryptotestdb_tests;Username=testuser;Password=testpassword;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CryptoWalletDbContext>));
            services.RemoveAll(typeof(CryptoWalletDbContext));

            services.AddDbContext<CryptoWalletDbContext>(options =>
            {
                options.UseNpgsql(GetDbConnectionString());
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CryptoWalletDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }
    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

}
