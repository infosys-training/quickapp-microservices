using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Data;

namespace Order.API.Tests;

public class OrderApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestOrderDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var efDescriptors = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(DbContextOptions<OrderDbContext>)
                          || d.ServiceType == typeof(DbContextOptions)
                          || d.ServiceType == typeof(OrderDbContext)
                          || d.ServiceType.FullName.Contains("EntityFrameworkCore")
                          || d.ServiceType.FullName.Contains("Sqlite")))
                .ToList();
            foreach (var d in efDescriptors)
                services.Remove(d);

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Development");
    }
}
