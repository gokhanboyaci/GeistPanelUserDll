using ideal.Config;
using ideal.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ideal.Database
{
    public  class AlgobarDbContext : DbContext
    {
        public AlgobarDbContext() : base(new SqlConnection(AppConfig.Instance.AlgobardataDbConnString), true)
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<AlgobarDbContext, ideal.Database.Migrations.AlgoBarConfiguration>()
            );
            this.Database.CommandTimeout = 0;
            this.Database.Initialize(false);
        }

        
        public DbSet<AlgoBarDataAll> AlgoBarDataAll { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Properties<decimal>().Configure(cfg => cfg.HasPrecision(18, 4));           
            modelBuilder.Entity<AlgoBarDataAll>().HasKey(x => x.ID).ToTable("IMKB_ALGOBARDATA_ALL");
            
            base.OnModelCreating(modelBuilder);
        }

        #region EF yardımcıları

        private void DetachRange(IEnumerable<object> entities)
        {
            var adapter = (IObjectContextAdapter)this;
            foreach (var e in entities)
            {
                var entry = Entry(e);
                if (entry != null) entry.State = EntityState.Detached;
            }
        }
        #endregion

        #region Bulk Upsert/Insert
        
        public async Task InsertBarsAsync(IEnumerable<AlgoBarDataAll> rows, int batchSize = 20000)
        {
            if (rows == null) return;
            var list = rows.ToList();
            if (list.Count == 0) return;

            await this.BulkInsertAsync(list).ConfigureAwait(false);

            // Bellek temizliği (opsiyonel)
            DetachRange(list);
        }

        /// <summary>
        /// Verilen gün için (tarih: Date kısmı) sembol bazında Max(ZAMAN) değerlerini tek sorguda getirir.
        /// </summary>
        public async Task<Dictionary<string, DateTime>> GetLastBarTimesBySymbolAsync(DateTime selectedDate, IEnumerable<string> symbols)
        {
            if (symbols == null) symbols = new string[0];
            var symbolList = symbols.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            if (symbolList.Count == 0) return new Dictionary<string, DateTime>();

            DateTime? day = selectedDate.Date;

            var data = await AlgoBarDataAll
                .Where(x => symbolList.Contains(x.SEMBOL) && DbFunctions.TruncateTime(x.ZAMAN) == day)
                .GroupBy(x => x.SEMBOL)
                .Select(g => new { SEMBOL = g.Key, MAXZ = g.Max(t => t.ZAMAN) })
                .ToListAsync()
                .ConfigureAwait(false);

            return data.ToDictionary(x => x.SEMBOL, x => x.MAXZ);
        }

        #endregion
    }
}

