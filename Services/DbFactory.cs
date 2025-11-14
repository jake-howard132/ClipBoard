using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClipBoard.Services
{
    public class DbFactory : IDesignTimeDbContextFactory<Db>
    {
        public Db CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Db>();

            // Match your runtime database provider
            optionsBuilder.UseSqlite("Data Source=ClipBoard.db");

            return new Db(optionsBuilder.Options);
        }
    }
}