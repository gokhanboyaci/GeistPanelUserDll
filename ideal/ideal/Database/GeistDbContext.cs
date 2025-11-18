using ideal.Config;
using ideal.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ideal.Database
{

    public class GeistDbContext : DbContext
    {
        public GeistDbContext() : base(new SqlConnection(AppConfig.Instance.MainDbConnString), true)
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<GeistDbContext, ideal.Database.Migrations.Configuration>()
            );
            this.Database.CommandTimeout = 0;
            this.Database.Initialize(false);
        }

        public DbSet<YuzeyselVeri> YuzeyselVeriler { get; set; }
        public DbSet<DestekDirencPivot> AlgoritmaVerileri { get; set; }
        public DbSet<GunlukAlgoBarData> GunlukAlgoBarVerileri { get; set; }
        public DbSet<KompozitData> KompozitVerileri { get; set; }
        public DbSet<TaramaUyumsuzluk> TaramaUyumsuzluk { get; set; }
        public DbSet<TaramaSRSI> TaramaSRSI { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Properties<decimal>().Configure(cfg => cfg.HasPrecision(18, 4));

            modelBuilder.Entity<YuzeyselVeri>().HasKey(x => x.SEMBOL).ToTable("IMKB_YUZEYSEL_VERI");
            modelBuilder.Entity<DestekDirencPivot>().HasKey(x => x.SEMBOL).ToTable("IMKB_DDP_DATA");
            modelBuilder.Entity<GunlukAlgoBarData>().HasKey(x => x.ID).ToTable("IMKB_GUNLUK_ALGOBARDATA");
            modelBuilder.Entity<KompozitData>().HasKey(x => x.SEMBOL).ToTable("IMKB_KOMPOZIT_DATA");
            modelBuilder.Entity<TaramaUyumsuzluk>().HasKey(x => x.ID).ToTable("IMKB_TARAMA_UYUMSUZLUK");
            modelBuilder.Entity<TaramaSRSI>().HasKey(x => x.ID).ToTable("IMKB_TARAMA_RSI_SINYAL");

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

        public async Task UpsertYuzeyselAsync(IEnumerable<YuzeyselVeri> rows, int batchSize = 10000)
        {
            if (rows == null) return;

            // null/boş temizliği ve duplike anahtarlarda son kayıt kalsın
            var list = rows
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SEMBOL))
                .GroupBy(r => r.SEMBOL)
                .Select(g => g.Last())
                .ToList();

            if (list.Count == 0) return;
            await this.BulkMergeAsync(list).ConfigureAwait(false);
        }

        public async Task UpsertAlgoritmaAsync(IEnumerable<DestekDirencPivot> rows, int batchSize = 10000)
        {
            if (rows == null) return;

            var list = rows
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SEMBOL))
                .GroupBy(r => r.SEMBOL)
                .Select(g => g.Last())
                .ToList();

            if (list.Count == 0) return;

            await this.BulkMergeAsync(list).ConfigureAwait(false);
        }

        public async Task UpsertKompozitAsync(IEnumerable<KompozitData> rows, int batchSize = 10000)
        {
            if (rows == null) return;

            var list = rows
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.SEMBOL))
                .GroupBy(r => r.SEMBOL)
                .Select(g => g.Last())
                .ToList();

            if (list.Count == 0) return;

            await this.BulkMergeAsync(list).ConfigureAwait(false);
        }
       
        public async Task InsertBarsAsync(IEnumerable<GunlukAlgoBarData> rows, int batchSize = 20000)
        {
            if (rows == null) return;
            var list = rows.ToList();
            if (list.Count == 0) return;

            await this.BulkInsertAsync(list).ConfigureAwait(false);

            // Bellek temizliği (opsiyonel)
            DetachRange(list);
        }

        public async Task InsertTaramaUyumsuzlukAsync(IEnumerable<TaramaUyumsuzluk> rows, int batchSize = 20000)
        {
            if (rows == null) return;
            var list = rows.ToList();
            if (list.Count == 0) return;

            await this.BulkInsertAsync(list).ConfigureAwait(false);

            // Bellek temizliği (opsiyonel)
            DetachRange(list);
        }

        public async Task InsertTaramaSRSIAsync(IEnumerable<TaramaSRSI> rows, int batchSize = 20000)
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

            var data = await GunlukAlgoBarVerileri
                .Where(x => symbolList.Contains(x.SEMBOL) && DbFunctions.TruncateTime(x.ZAMAN) == day)
                .GroupBy(x => x.SEMBOL)
                .Select(g => new { SEMBOL = g.Key, MAXZ = g.Max(t => t.ZAMAN) })
                .ToListAsync()
                .ConfigureAwait(false);

            return data.ToDictionary(x => x.SEMBOL, x => x.MAXZ);
        }

        public async Task ResetBarsIfDifferentDayAsync(DateTime targetDate)
        {
            // En son kayıt zamanını çek
            var lastDate = await GunlukAlgoBarVerileri
                .OrderByDescending(x => x.ZAMAN)
                .Select(x => (DateTime?)x.ZAMAN)
                .FirstOrDefaultAsync();

            // Tablo boşsa bir şey yapma
            if (lastDate == null)
                return;

            // Gün farklıysa tabloyu temizle
            if (lastDate.Value.Date != targetDate.Date)
            {
                await ClearBarsAsync();
            }
        }
        public async Task ClearBarsAsync()
        {
            await Database.ExecuteSqlCommandAsync("TRUNCATE TABLE IMKB_GUNLUK_ALGOBARDATA");
        }
        public async Task ClearUyumsuzlukAsync()
        {
            await Database.ExecuteSqlCommandAsync("TRUNCATE TABLE IMKB_TARAMA_UYUMSUZLUK");
        }
        public async Task ClearTaramaSRSIAsync()
        {
            await Database.ExecuteSqlCommandAsync("TRUNCATE TABLE IMKB_TARAMA_RSI_SINYAL");
        }




        #endregion
    }
}
