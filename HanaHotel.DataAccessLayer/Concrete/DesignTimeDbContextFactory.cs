using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HanaHotel.DataAccessLayer.Concrete
{
    // EF tools will call this at design-time to create the DbContext
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            // Use the same connection string as in your DataContext.OnConfiguring
            var connectionString = "Server=db31658.public.databaseasp.net; Database=db31658; User Id=db31658; Password=12345678; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

            optionsBuilder.UseSqlServer(connectionString, b => 
                b.MigrationsAssembly(typeof(DataContext).Assembly.FullName)
            );

            return new DataContext(optionsBuilder.Options);
        }
    }
}