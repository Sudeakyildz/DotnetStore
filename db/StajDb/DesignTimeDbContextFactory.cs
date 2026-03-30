using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StajDb;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        // İleride backend projesi gelince connection string’i oradan yöneteceğiz.
        // Şimdilik migration için LocalDB default veriyorum.
        var connectionString =
            Environment.GetEnvironmentVariable("STAJ_CONNECTION_STRING") ??
            "Server=(localdb)\\MSSQLLocalDB;Database=StajProjeDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new DataContext(options);
    }
}

