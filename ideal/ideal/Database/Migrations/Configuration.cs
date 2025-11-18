using DevExpress.Utils.Filtering;
using System.Data.Entity.Migrations;

namespace ideal.Database.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ideal.Database.GeistDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;          // Otomatik migration açık
            AutomaticMigrationDataLossAllowed = false;  // Veri kaybına izin verme (drop/alter risklerini engeller)
        }
    }

    internal sealed class AlgoBarConfiguration: DbMigrationsConfiguration<ideal.Database.AlgobarDbContext>
    {
        public AlgoBarConfiguration()
        {
            AutomaticMigrationsEnabled = true;          // Otomatik migration açık
            AutomaticMigrationDataLossAllowed = false;  // Veri kaybına izin verme (drop/alter risklerini engeller)
        }
    }
}
