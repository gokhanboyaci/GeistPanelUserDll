using System.Data.Entity;
using ideal.Config;
using ideal.Model;

namespace ideal.Database
{
    public class SembollerDbContext : DbContext
    {
        public SembollerDbContext() : base(new System.Data.SqlClient.SqlConnection(AppConfig.Instance.SembollerDbConnString), true)
        {
            System.Data.Entity.Database.SetInitializer<SembollerDbContext>(null);
        }

        public DbSet<Semboller> Semboller { get; set; }

    }
}
