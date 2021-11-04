using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Persistance.Conext
{
    public class DataContext : DbContext
    {
        public DbSet<Department> Departments { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DataContext() { }
        public DataContext(DbContextOptions options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BankExplorerDB;Trusted_Connection=True;");
            //base.OnConfiguring(optionsBuilder);
        }

    }
}
