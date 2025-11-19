using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using ideal.Config;
using ideal.Database;
using ideal.Forms;
using ideal.Logging;
using ideal.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ideal.Helper
{
    /// <summary>
    /// UI yardımcıları, veri okuma akışları (EF tabanlı).
    /// İSTENENLER:
    /// - NormalizeSymbol kaldırıldı
    /// - FindStartIndexOld & FindStartIndex var
    /// - BarKey, GetIso8601WeekOfYear kaldırıldı
    /// - EndeksListesiniOkuAsync kaldırıldı
    /// - IMKBH_YuzeyselVeri_OkuAsync var
    /// - IMKBH_DD_OkuAsync var (GDD/HDD/ADD birleşik)
    /// - IMKBH_BarData_OkuAsync var
    /// </summary>
    public class IdealHelper
    {
        #region UI Yardımcıları
        private sealed class DisplayState { public string Text = string.Empty; public CustomDisplayTextEventHandler Handler; }
        private readonly Dictionary<ProgressBarControl, DisplayState> _displayStates = new Dictionary<ProgressBarControl, DisplayState>();

        private DisplayState EnsureDisplayBinding(ProgressBarControl bar)
        {
            DisplayState st;
            if (_displayStates.TryGetValue(bar, out st)) return st;
            st = new DisplayState();
            bar.Properties.ShowTitle = true;
            st.Handler = (object _, CustomDisplayTextEventArgs e) => { e.DisplayText = st.Text ?? string.Empty; };
            bar.CustomDisplayText += st.Handler;
            _displayStates[bar] = st; return st;
        }
        private void ResetProgress(ProgressBarControl bar, int max)
        {
            var st = EnsureDisplayBinding(bar); st.Text = string.Empty;
            Action apply = () =>
            {
                bar.Properties.Minimum = 0;
                bar.Properties.Maximum = Math.Max(1, max);
                bar.EditValue = 0;
                bar.Refresh();
            };
            if (bar.IsHandleCreated && bar.InvokeRequired) bar.Invoke(apply); else apply();
        }
        private void Update(int current, int total, string item, Stopwatch sw, ProgressBarControl bar)
        {
            var st = EnsureDisplayBinding(bar);
            if (item != null && item.Length < 10) item = item + " ";
            var percent = total > 0 ? (int)Math.Round(current * 100.0 / total) : 0;
            st.Text = string.Format("%{0} | {1} | Geçen Süre : {2}", percent, item ?? "", sw.Elapsed.ToString(@"hh\:mm\:ss"));
            Action apply = () =>
            {
                bar.Properties.Maximum = Math.Max(total, 1);
                var val = current;
                if (val < 0) val = 0;
                if (val > bar.Properties.Maximum) val = bar.Properties.Maximum;
                bar.EditValue = val;
                bar.Refresh();
            };
            if (bar.IsHandleCreated && bar.InvokeRequired) bar.Invoke(apply); else apply();
        }
        #endregion

        #region Genel Yardımcılar

        /// <summary>
        /// (Eski) Lineer arama – gerekmesi halinde kullanılabilir.
        /// </summary>
        private static int FindStartIndexOld(IList<cxBar> bars, DateTime selectedDate)
        {
            var startOfSelected = selectedDate.Date;
            for (int j = 0; j < bars.Count; j++)
                if (bars[j].Date >= startOfSelected) return j;
            return bars.Count;
        }

        /// <summary>
        /// Seçilen güne ait ilk barın indeksini ikili arama ile bulur.
        /// </summary>
        private static int FindStartIndex(IList<cxBar> bars, DateTime selectedDate)
        {
            var startOfSelected = selectedDate.Date;
            int left = 0, right = bars.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (bars[mid].Date < startOfSelected) left = mid + 1;
                else right = mid - 1;
            }
            return left;
        }

        #endregion

        #region Kaynak Okuma

        /// <summary>
        /// SEMBOLLER tablosundan aktif sembolleri okur (DURUM=1 ve GRUP A/Y filtreli).
        /// </summary>
        public async Task<List<Semboller>> SembolListesiniOkuAsync()
        {
            using (var db = new SembollerDbContext())
            {
                var q = db.Semboller.AsQueryable();
                try
                {
                    q = q.Where(x => x.DURUM == 1 && (x.GRUP == "A" || x.GRUP == "Y" || x.GRUP == "ENDEKS"));
                }
                catch
                {
                    _ = TelegramHelper.SendMessageAsync("Sembol listesi okuma hatası.");
                }
                return await q.ToListAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region YÜZEYSEL VERİ (IMKB)

        /// <summary>
        /// Hisse (IMKBH) yüzeysel verilerini okur ve IMKBH_YUZEYSEL_VERI tablosuna EF upsert ile yazar.
        /// </summary>
        public async Task IMKB_YuzeyselVeri_OkuAsync(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sistem = User.MySistem1;
            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0) { ResetProgress(bar, 1); return; }

            ResetProgress(bar, semboller.Count);
            var sw = Stopwatch.StartNew();
            var list = new List<YuzeyselVeri>(semboller.Count);
            int i = 0;

            foreach (var sbl in semboller)
            {
                ct.ThrowIfCancellationRequested();
                var orj = sbl != null ? sbl.SEMBOL : string.Empty;

                try
                {
                    var usdData = await Task.Run(() => sistem.YuzeyselVeriOku("FX'USDTRY"), ct).ConfigureAwait(false);
                    var data = await Task.Run(() => sistem.YuzeyselVeriOku(orj), ct).ConfigureAwait(false);

                    var fiyat = (decimal)Math.Round((decimal)data.LastPrice, 2);
                    var usdKur = (decimal)Math.Round((decimal)usdData.LastPrice, 2);

                    if (data != null)
                    {
                        list.Add(new YuzeyselVeri
                        {
                            SEMBOL = orj ?? string.Empty,
                            TANIM = data.Description,
                            SEKTOR = sbl.SEKTOR,
                            PAZAR = data.Grup,
                            FIYAT = fiyat,
                            USD_FIYAT = usdKur != 0 ? fiyat / usdKur : 0,
                            YUZDE = (decimal)Math.Round((decimal)data.NetPerDay, 2),
                            TAVAN = (decimal)Math.Round((decimal)data.LimitUp, 2),
                            TABAN = (decimal)Math.Round((decimal)data.LimitDown, 2),
                            YUKSEK = (decimal)Math.Round((decimal)data.HighDay, 2),
                            DUSUK = (decimal)Math.Round((decimal)data.LowDay, 2),
                            HAFTALIK_YUKSEK = (decimal)Math.Round((decimal)data.HighWeek, 2),
                            HAFTALIK_DUSUK = (decimal)Math.Round((decimal)data.LowWeek, 2),
                            AYLIK_YUKSEK = (decimal)Math.Round((decimal)data.HighMonth, 2),
                            AYLIK_DUSUK = (decimal)Math.Round((decimal)data.LowMonth, 2),
                            ONCEKI_KAPANIS = (decimal)Math.Round((decimal)data.PrevCloseDay, 2),
                            ENDEKS = int.TryParse(data.IndexType, out var endeks) ? endeks : 0
                        });
                    }
                }
                catch (OperationCanceledException ex)
                {
                    _ = TelegramHelper.SendMessageAsync("IMKB yüzeysel veri okuma iptal edildi: " + ex.Message);
                }
                catch (Exception ex)
                {
                    await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + orj + "] " + ex.Message, ex));

                    _ = TelegramHelper.SendMessageAsync(orj + " sembolü için yüzeysel veri okunamadı: " + ex.Message);
                }
                finally
                {
                    i++;
                    Update(i, semboller.Count, orj, sw, bar);
                }
            }

            sw.Stop();
            if (list.Count == 0) return;

            try
            {
                using (var db = new GeistDbContext())
                {
                    await db.UpsertYuzeyselAsync(list).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {

                _ = TelegramHelper.SendMessageAsync("IMKB yüzeysel veri DB yazma hatası: " + ex.Message);
            }


            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        #endregion

        #region ALGORITMA (GDD/HDD/ADD birleşik)

        /// <summary>
        /// Günlük/Haftalık/Aylık değerleri üretir ve IMKBH_ALGORITMA_DATA tablosuna kısmi upsert eder.
        /// (GDD/HDD/ADD tek metotta toplandı)
        /// </summary>
        public async Task IMKB_DD_OkuAsync(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sysGunluk = User.MySistem2;
            var sysHaftalik = User.MySistem3;
            var sysAylik = User.MySistem4;

            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            ResetProgress(bar, semboller.Count);
            var sw = Stopwatch.StartNew();
            int i = 0;

            var toSave = new List<DestekDirencPivot>(semboller.Count);

            foreach (var sbl in semboller)
            {
                ct.ThrowIfCancellationRequested();
                var orj = sbl != null ? sbl.SEMBOL : string.Empty;

                try
                {
                    var cfg = AppConfig.Instance;

                    // 3 sistemi paralel çalıştır
                    var gddTask = Task.Run(() => sysGunluk.SistemGetir(cfg.GDD_IdealName, orj, "5"), ct);
                    var hddTask = Task.Run(() => sysHaftalik.SistemGetir(cfg.HDD_IdealName, orj, "5"), ct);
                    var addTask = Task.Run(() => sysAylik.SistemGetir(cfg.ADD_IdealName, orj, "5"), ct);

                    await Task.WhenAll(gddTask, hddTask, addTask).ConfigureAwait(false);

                    var sysG = gddTask.Result;
                    var sysH = hddTask.Result;
                    var sysA = addTask.Result;

                    int ixG = (sysG != null ? sysG.BarSayisi : 0) - 1;
                    int ixH = (sysH != null ? sysH.BarSayisi : 0) - 1;
                    int ixA = (sysA != null ? sysA.BarSayisi : 0) - 1;

                    Func<dynamic, int, int, decimal> Read = (sys, ix, idx) =>
                        (sys != null && ix >= 0) ? (decimal)Math.Round((float)sys.Cizgiler[idx].Deger[ix], 2) : 0m;

                    var a = new DestekDirencPivot
                    {
                        SEMBOL = orj ?? string.Empty,

                        // Günlük
                        GUN_DIR1 = Read(sysG, ixG, 2),
                        GUN_DIR2 = Read(sysG, ixG, 1),
                        GUN_DIR3 = Read(sysG, ixG, 0),
                        GUN_PIVOT = Read(sysG, ixG, 3),
                        GUN_DEST1 = Read(sysG, ixG, 4),
                        GUN_DEST2 = Read(sysG, ixG, 5),
                        GUN_DEST3 = Read(sysG, ixG, 6),

                        // Haftalık
                        HAFTA_DIR1 = Read(sysH, ixH, 2),
                        HAFTA_DIR2 = Read(sysH, ixH, 1),
                        HAFTA_DIR3 = Read(sysH, ixH, 0),
                        HAFTA_PIVOT = Read(sysH, ixH, 3),
                        HAFTA_DEST1 = Read(sysH, ixH, 4),
                        HAFTA_DEST2 = Read(sysH, ixH, 5),
                        HAFTA_DEST3 = Read(sysH, ixH, 6),

                        // Aylık
                        AY_DIR1 = Read(sysA, ixA, 2),
                        AY_DIR2 = Read(sysA, ixA, 1),
                        AY_DIR3 = Read(sysA, ixA, 0),
                        AY_PIVOT = Read(sysA, ixA, 3),
                        AY_DEST1 = Read(sysA, ixA, 4),
                        AY_DEST2 = Read(sysA, ixA, 5),
                        AY_DEST3 = Read(sysA, ixA, 6)
                    };

                    toSave.Add(a);
                }
                catch (OperationCanceledException)
                {
                    _ = TelegramHelper.SendMessageAsync("IMKB algoritma veri okuma iptal edildi.");
                }
                catch (Exception ex)
                {
                    await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + orj + "] " + ex.Message, ex));
                    _ = TelegramHelper.SendMessageAsync(orj + " sembolü için algoritma veri okunamadı: " + ex.Message);
                }
                finally
                {
                    i++;
                    Update(i, semboller.Count, orj, sw, bar);
                }
            }

            if (toSave.Count > 0)
            {
                try
                {
                    using (var db = new GeistDbContext())
                    {
                        await db.UpsertAlgoritmaAsync(toSave).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {

                    _ = TelegramHelper.SendMessageAsync("IMKB algoritma veri DB yazma hatası: " + ex.Message);
                }

            }

            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        #endregion
       

        #region BARDATA (IMKB)

        /// <summary>
        /// IMKBH bardata okur ve IMKBH_ALGOBARDATA sabit tablosuna yazar.
        /// Aynı güne ait daha önce yazılan bar'lardan sonra gelen bar'lar eklenir.
        /// </summary>
        public async Task IMKB_BarData_OkuAsync(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sistemler = new[] { User.MySistem5, User.MySistem6, User.MySistem7, User.MySistem8 };
            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            var selectedDate = FrmMain.Reference.txtStartCalendar.DateTime.Date;
            int minuteStep = 5; // Periyot
            ResetProgress(bar, semboller.Count);

            var sw = Stopwatch.StartNew();


            // DB'de aynı gün için sembol -> Max(ZAMAN) sözlüğü
            Dictionary<string, DateTime> lastBarTimes;

            using (var db = new GeistDbContext())
            {
                await db.ResetBarsIfDifferentDayAsync(selectedDate);
            }

            using (var db = new GeistDbContext())
            {
                var symbols = semboller.Select(s => s.SEMBOL).ToList();
                lastBarTimes = await db.GetLastBarTimesBySymbolAsync(selectedDate, symbols).ConfigureAwait(false);
            }

            var toInsert = new ConcurrentBag<GunlukAlgoBarData>();
            int completed = 0;
            var limiter = new SemaphoreSlim(3);
            int batchSize = 4;

            for (int i = 0; i < semboller.Count; i += batchSize)
            {
                ct.ThrowIfCancellationRequested();
                var batch = semboller.Skip(i).Take(batchSize).ToList();
                var tasks = new List<Task>(batch.Count);

                for (int k = 0; k < batch.Count; k++)
                {
                    var localSbl = batch[k];
                    var localSistem = sistemler[k % sistemler.Length];

                    tasks.Add(Task.Run(async () =>
                    {
                        await limiter.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            var orj = localSbl != null ? localSbl.SEMBOL : string.Empty;
                            if (string.IsNullOrWhiteSpace(orj)) return;

                            var barData = localSistem.GrafikVerileriniOku(orj, "1");
                            var cfg = AppConfig.Instance;
                            var sistemData = localSistem.SistemGetir(cfg.IMKBH_BarData_IdealName, orj, "1");
                            if (barData == null || barData.Count < 1) return;

                            int startIndex = FindStartIndex(barData, selectedDate);

                            DateTime lastTime;
                            var hasLast = lastBarTimes.TryGetValue(orj, out lastTime);

                            for (int barX = startIndex; barX < barData.Count; barX++)
                            {
                                var bItem = barData[barX];

                                // 1) Aynı gün içinde son yazılandan daha yeni olmalı
                                if (hasLast && bItem.Date <= lastTime) continue;

                                // 2) Periot filtresi (ör. 5 dk)
                                if (minuteStep > 0 && (bItem.Date.Minute % minuteStep != 0)) continue;

                                Func<int, float> ReadF = idx =>
                                    (sistemData != null && barX >= 0)
                                        ? (float)sistemData.Cizgiler[idx].Deger[barX]
                                        : 0f;

                                toInsert.Add(new GunlukAlgoBarData
                                {
                                    SEMBOL = orj,
                                    PERIYOT = minuteStep.ToString(),
                                    ZAMAN = bItem.Date,
                                    ACILIS = (decimal)Math.Round(barData[barX].Open, 2),
                                    YUKSEK = (decimal)Math.Round(barData[barX].High, 2),
                                    DUSUK = (decimal)Math.Round(barData[barX].Low, 2),
                                    KAPANIS = (decimal)Math.Round(barData[barX].Close, 2),
                                    HACIM = (decimal)Math.Round(barData[barX].Vol, 2),

                                    GEIST1 = (decimal)Math.Round(ReadF(0), 2),
                                    GEIST2 = (decimal)Math.Round(ReadF(1), 2),
                                    GEIST3 = (decimal)Math.Round(ReadF(2), 2),
                                    GEIST_TOPLAM = (decimal)Math.Round(ReadF(3), 2),
                                    MULTI_SISTEM_EK = (int)Math.Round(ReadF(4), 2),
                                    HACIM_YON = (decimal)Math.Round(ReadF(5), 2),
                                    HACIM_YON_EK = (int)Math.Round(ReadF(6), 2),
                                    MULTI_ROKET = (decimal)Math.Round(ReadF(7), 2),
                                    MULTI_ROKET_EK = (int)Math.Round(ReadF(8), 2),
                                    MULTI_EX = (decimal)Math.Round(ReadF(9), 2),
                                    MULTI_EX_EK = (int)Math.Round(ReadF(10), 2),
                                    GEIST_TR = (decimal)Math.Round(ReadF(11), 2),
                                    GEIST_TR_EK = (int)Math.Round(ReadF(12), 2),
                                    TERMINATOR = (decimal)Math.Round(ReadF(13), 2),
                                    TERMINATOR_EK = (int)Math.Round(ReadF(14), 2),
                                    HACIM_SIRA = (int)Math.Round(ReadF(15), 2),
                                    BOGA_AYI_TL_GUN = (int)Math.Round(ReadF(16), 2),
                                    BOGA_AYI_USD_GUN = (int)Math.Round(ReadF(17), 2),
                                    BOGA_AYI_TL_HAFTA = (int)Math.Round(ReadF(18), 2),
                                    BOGA_AYI_USD_HAFTA = (int)Math.Round(ReadF(19), 2),
                                    BOGA_AYI_TL_AY = (int)Math.Round(ReadF(20), 2),
                                    BOGA_AYI_USD_AY = (int)Math.Round(ReadF(21), 2),
                                    P1M = (int)Math.Round(ReadF(22), 2),
                                    P5M = (int)Math.Round(ReadF(23), 2),
                                    P15M = (int)Math.Round(ReadF(24), 2),
                                    P20M = (int)Math.Round(ReadF(25), 2),
                                    P30M = (int)Math.Round(ReadF(26), 2),
                                    P60M = (int)Math.Round(ReadF(27), 2),
                                    P120M = (int)Math.Round(ReadF(28), 2),
                                    P240M = (int)Math.Round(ReadF(29), 2),
                                    PD = (int)Math.Round(ReadF(30), 2),
                                    PW = (int)Math.Round(ReadF(31), 2),
                                    PM = (int)Math.Round(ReadF(32), 2),
                                    EMA5 = (decimal)Math.Round(ReadF(33), 2),
                                    EMA21 = (decimal)Math.Round(ReadF(34), 2),
                                    EMA63 = (decimal)Math.Round(ReadF(35), 2),
                                    EMA126 = (decimal)Math.Round(ReadF(36), 2),
                                    EMA252 = (decimal)Math.Round(ReadF(37), 2),
                                    EMA756 = (decimal)Math.Round(ReadF(38), 2),
                                    EMA100 = (decimal)Math.Round(ReadF(39), 2),
                                    EMA200 = (decimal)Math.Round(ReadF(40), 2),
                                    RSI = (decimal)Math.Round(ReadF(41), 2),
                                    SRSI = (decimal)Math.Round(ReadF(42), 2)
                                });
                            }

                            // Bellek sözlüğünü güncelle (bu sembol için son bar)
                            DateTime maxForSymbol = DateTime.MinValue;
                            foreach (var r in toInsert.Where(x => x.SEMBOL == orj))
                                if (r.ZAMAN > maxForSymbol) maxForSymbol = r.ZAMAN;

                            if (maxForSymbol > DateTime.MinValue)
                                lastBarTimes[orj] = maxForSymbol;
                        }
                        catch (OperationCanceledException)
                        {
                            _ = TelegramHelper.SendMessageAsync("IMKB bardata okuma iptal edildi.");
                        }
                        catch (Exception ex)
                        {
                            await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + (localSbl != null ? localSbl.SEMBOL : "?") + "] " + ex.Message, ex));
                            _ = TelegramHelper.SendMessageAsync((localSbl != null ? localSbl.SEMBOL : "?") + " sembolü için bardata okunamadı: " + ex.Message);

                        }
                        finally
                        {
                            Interlocked.Increment(ref completed);
                            Update(completed, semboller.Count, localSbl != null ? localSbl.SEMBOL : "", sw, bar);
                            limiter.Release();
                        }
                    }, ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            sw.Stop();

            if (!toInsert.IsEmpty)
            {
                try
                {
                    Update(semboller.Count, semboller.Count, "Veritabanına yazılıyor...", sw, bar);
                    using (var db = new GeistDbContext())
                        await db.InsertBarsAsync(toInsert.ToList()).ConfigureAwait(false);
                }
                catch (Exception)
                {

                    _ = TelegramHelper.SendMessageAsync("IMKB bardata DB yazma hatası.");
                }

            }

            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        public async Task IMKB_BarData_GunSonu_OkuAsync(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sistemler = new[] { User.MySistem5, User.MySistem6, User.MySistem7, User.MySistem8 };
            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            var selectedDate = FrmMain.Reference.txtStartCalendar.DateTime.Date;
            int minuteStep = 5; // Periyot
            ResetProgress(bar, semboller.Count);

            var sw = Stopwatch.StartNew();


            // DB'de aynı gün için sembol -> Max(ZAMAN) sözlüğü
            Dictionary<string, DateTime> lastBarTimes;
                 

            using (var db = new AlgobarDbContext())
            {
                var symbols = semboller.Select(s => s.SEMBOL).ToList();
                lastBarTimes = await db.GetLastBarTimesBySymbolAsync(selectedDate, symbols).ConfigureAwait(false);
            }

            var toInsert = new ConcurrentBag<AlgoBarDataAll>();
            int completed = 0;
            var limiter = new SemaphoreSlim(3);
            int batchSize = 4;

            for (int i = 0; i < semboller.Count; i += batchSize)
            {
                ct.ThrowIfCancellationRequested();
                var batch = semboller.Skip(i).Take(batchSize).ToList();
                var tasks = new List<Task>(batch.Count);

                for (int k = 0; k < batch.Count; k++)
                {
                    var localSbl = batch[k];
                    var localSistem = sistemler[k % sistemler.Length];

                    tasks.Add(Task.Run(async () =>
                    {
                        await limiter.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            var orj = localSbl != null ? localSbl.SEMBOL : string.Empty;
                            if (string.IsNullOrWhiteSpace(orj)) return;

                            var barData = localSistem.GrafikVerileriniOku(orj, "1");
                            var cfg = AppConfig.Instance;
                            var sistemData = localSistem.SistemGetir(cfg.IMKBH_BarData_IdealName, orj, "1");
                            if (barData == null || barData.Count < 1) return;

                            int startIndex = FindStartIndex(barData, selectedDate);

                            DateTime lastTime;
                            var hasLast = lastBarTimes.TryGetValue(orj, out lastTime);

                            for (int barX = startIndex; barX < barData.Count; barX++)
                            {
                                var bItem = barData[barX];

                                // 1) Aynı gün içinde son yazılandan daha yeni olmalı
                                if (hasLast && bItem.Date <= lastTime) continue;

                                // 2) Periot filtresi (ör. 5 dk)
                                if (minuteStep > 0 && (bItem.Date.Minute % minuteStep != 0)) continue;

                                Func<int, float> ReadF = idx =>
                                    (sistemData != null && barX >= 0)
                                        ? (float)sistemData.Cizgiler[idx].Deger[barX]
                                        : 0f;

                                toInsert.Add(new AlgoBarDataAll
                                {
                                    SEMBOL = orj,
                                    PERIYOT = minuteStep.ToString(),
                                    ZAMAN = bItem.Date,
                                    ACILIS = (decimal)Math.Round(barData[barX].Open, 2),
                                    YUKSEK = (decimal)Math.Round(barData[barX].High, 2),
                                    DUSUK = (decimal)Math.Round(barData[barX].Low, 2),
                                    KAPANIS = (decimal)Math.Round(barData[barX].Close, 2),
                                    HACIM = (decimal)Math.Round(barData[barX].Vol, 2),

                                    GEIST1 = (decimal)Math.Round(ReadF(0), 2),
                                    GEIST2 = (decimal)Math.Round(ReadF(1), 2),
                                    GEIST3 = (decimal)Math.Round(ReadF(2), 2),
                                    GEIST_TOPLAM = (decimal)Math.Round(ReadF(3), 2),
                                    MULTI_SISTEM_EK = (int)Math.Round(ReadF(4), 2),
                                    HACIM_YON = (decimal)Math.Round(ReadF(5), 2),
                                    HACIM_YON_EK = (int)Math.Round(ReadF(6), 2),
                                    MULTI_ROKET = (decimal)Math.Round(ReadF(7), 2),
                                    MULTI_ROKET_EK = (int)Math.Round(ReadF(8), 2),
                                    MULTI_EX = (decimal)Math.Round(ReadF(9), 2),
                                    MULTI_EX_EK = (int)Math.Round(ReadF(10), 2),
                                    GEIST_TR = (decimal)Math.Round(ReadF(11), 2),
                                    GEIST_TR_EK = (int)Math.Round(ReadF(12), 2),
                                    TERMINATOR = (decimal)Math.Round(ReadF(13), 2),
                                    TERMINATOR_EK = (int)Math.Round(ReadF(14), 2),
                                    HACIM_SIRA = (int)Math.Round(ReadF(15), 2),
                                    BOGA_AYI_TL_GUN = (int)Math.Round(ReadF(16), 2),
                                    BOGA_AYI_USD_GUN = (int)Math.Round(ReadF(17), 2),
                                    BOGA_AYI_TL_HAFTA = (int)Math.Round(ReadF(18), 2),
                                    BOGA_AYI_USD_HAFTA = (int)Math.Round(ReadF(19), 2),
                                    BOGA_AYI_TL_AY = (int)Math.Round(ReadF(20), 2),
                                    BOGA_AYI_USD_AY = (int)Math.Round(ReadF(21), 2),
                                    P1M = (int)Math.Round(ReadF(22), 2),
                                    P5M = (int)Math.Round(ReadF(23), 2),
                                    P15M = (int)Math.Round(ReadF(24), 2),
                                    P20M = (int)Math.Round(ReadF(25), 2),
                                    P30M = (int)Math.Round(ReadF(26), 2),
                                    P60M = (int)Math.Round(ReadF(27), 2),
                                    P120M = (int)Math.Round(ReadF(28), 2),
                                    P240M = (int)Math.Round(ReadF(29), 2),
                                    PD = (int)Math.Round(ReadF(30), 2),
                                    PW = (int)Math.Round(ReadF(31), 2),
                                    PM = (int)Math.Round(ReadF(32), 2),
                                    EMA5 = (decimal)Math.Round(ReadF(33), 2),
                                    EMA21 = (decimal)Math.Round(ReadF(34), 2),
                                    EMA63 = (decimal)Math.Round(ReadF(35), 2),
                                    EMA126 = (decimal)Math.Round(ReadF(36), 2),
                                    EMA252 = (decimal)Math.Round(ReadF(37), 2),
                                    EMA756 = (decimal)Math.Round(ReadF(38), 2),
                                    EMA100 = (decimal)Math.Round(ReadF(39), 2),
                                    EMA200 = (decimal)Math.Round(ReadF(40), 2),
                                    RSI = (decimal)Math.Round(ReadF(41), 2),
                                    SRSI = (decimal)Math.Round(ReadF(42), 2)
                                });
                            }

                            // Bellek sözlüğünü güncelle (bu sembol için son bar)
                            DateTime maxForSymbol = DateTime.MinValue;
                            foreach (var r in toInsert.Where(x => x.SEMBOL == orj))
                                if (r.ZAMAN > maxForSymbol) maxForSymbol = r.ZAMAN;

                            if (maxForSymbol > DateTime.MinValue)
                                lastBarTimes[orj] = maxForSymbol;
                        }
                        catch (OperationCanceledException)
                        {
                            _ = TelegramHelper.SendMessageAsync("IMKB bardata okuma iptal edildi.");
                        }
                        catch (Exception ex)
                        {
                            await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + (localSbl != null ? localSbl.SEMBOL : "?") + "] " + ex.Message, ex));
                            _ = TelegramHelper.SendMessageAsync((localSbl != null ? localSbl.SEMBOL : "?") + " sembolü için bardata okunamadı: " + ex.Message);

                        }
                        finally
                        {
                            Interlocked.Increment(ref completed);
                            Update(completed, semboller.Count, localSbl != null ? localSbl.SEMBOL : "", sw, bar);
                            limiter.Release();
                        }
                    }, ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            sw.Stop();

            if (!toInsert.IsEmpty)
            {
                try
                {
                    Update(semboller.Count, semboller.Count, "Veritabanına yazılıyor...", sw, bar);
                    using (var db = new AlgobarDbContext())
                        await db.InsertBarsAsync(toInsert.ToList()).ConfigureAwait(false);
                }
                catch (Exception)
                {

                    _ = TelegramHelper.SendMessageAsync("IMKB bardata DB yazma hatası.");
                }

            }

            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        public async Task IMKB_BarData_OkuAsync_Aralikli(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sistemler = new[] { User.MySistem5, User.MySistem6, User.MySistem7, User.MySistem8 };
            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            var selectedDate = FrmMain.Reference.txtStartCalendar.DateTime.Date;
            var endDate = (FrmMain.Reference.txtEndCalendar.EditValue == null || FrmMain.Reference.txtEndCalendar.DateTime == DateTime.MinValue) ? selectedDate : FrmMain.Reference.txtEndCalendar.DateTime.Date;

            if (endDate < selectedDate)
                endDate = selectedDate;

            const int minuteStep = 5;
            ResetProgress(bar, semboller.Count);

            var sw = Stopwatch.StartNew();

            // DB'de aynı gün için sembol -> Max(ZAMAN) sözlüğü
            Dictionary<string, DateTime> lastBarTimes;
            using (var db = new AlgobarDbContext())
            {
                var symbols = semboller.Select(s => s.SEMBOL).ToList();
                lastBarTimes = await db.GetLastBarTimesBySymbolAsync(selectedDate, symbols).ConfigureAwait(false);
            }

            var toInsert = new ConcurrentBag<AlgoBarDataAll>();
            int completed = 0;
            var limiter = new SemaphoreSlim(3);
            int batchSize = 4;

            for (int i = 0; i < semboller.Count; i += batchSize)
            {
                ct.ThrowIfCancellationRequested();
                var batch = semboller.Skip(i).Take(batchSize).ToList();
                var tasks = new List<Task>(batch.Count);

                for (int k = 0; k < batch.Count; k++)
                {
                    var localSbl = batch[k];
                    var localSistem = sistemler[k % sistemler.Length];

                    tasks.Add(Task.Run(async () =>
                    {
                        string orj = localSbl?.SEMBOL ?? string.Empty;

                        await limiter.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            if (string.IsNullOrWhiteSpace(orj)) return;

                            var barData = localSistem.GrafikVerileriniOku(orj, "1");
                            var cfg = AppConfig.Instance;
                            var sistemData = localSistem.SistemGetir(cfg.IMKBH_BarData_IdealName, orj, "1");
                            if (barData == null || barData.Count < 1) return;

                            int startIndex = FindStartIndex(barData, selectedDate);

                            DateTime lastTime;
                            var hasLast = lastBarTimes.TryGetValue(orj, out lastTime);

                            var localInsert = new List<AlgoBarDataAll>();

                            for (int barX = startIndex; barX < barData.Count; barX++)
                            {
                                var bItem = barData[barX];
                                var d = bItem.Date.Date;
                                if (d < selectedDate || d > endDate) continue;
                                if (hasLast && bItem.Date <= lastTime) continue;
                                if (minuteStep > 0 && (bItem.Date.Minute % minuteStep != 0)) continue;

                                Func<int, float> ReadF = idx =>
                                    (sistemData != null && barX >= 0)
                                        ? (float)sistemData.Cizgiler[idx].Deger[barX]
                                        : 0f;

                                localInsert.Add(new AlgoBarDataAll
                                {
                                    SEMBOL = orj,
                                    PERIYOT = minuteStep.ToString(),
                                    ZAMAN = bItem.Date,
                                    ACILIS = (decimal)Math.Round(barData[barX].Open, 2),
                                    YUKSEK = (decimal)Math.Round(barData[barX].High, 2),
                                    DUSUK = (decimal)Math.Round(barData[barX].Low, 2),
                                    KAPANIS = (decimal)Math.Round(barData[barX].Close, 2),
                                    HACIM = (decimal)Math.Round(barData[barX].Vol, 2),

                                    GEIST1 = (decimal)Math.Round(ReadF(0), 2),
                                    GEIST2 = (decimal)Math.Round(ReadF(1), 2),
                                    GEIST3 = (decimal)Math.Round(ReadF(2), 2),
                                    GEIST_TOPLAM = (decimal)Math.Round(ReadF(3), 2),
                                    MULTI_SISTEM_EK = (int)Math.Round(ReadF(4), 2),
                                    HACIM_YON = (decimal)Math.Round(ReadF(5), 2),
                                    HACIM_YON_EK = (int)Math.Round(ReadF(6), 2),
                                    MULTI_ROKET = (decimal)Math.Round(ReadF(7), 2),
                                    MULTI_ROKET_EK = (int)Math.Round(ReadF(8), 2),
                                    MULTI_EX = (decimal)Math.Round(ReadF(9), 2),
                                    MULTI_EX_EK = (int)Math.Round(ReadF(10), 2),
                                    GEIST_TR = (decimal)Math.Round(ReadF(11), 2),
                                    GEIST_TR_EK = (int)Math.Round(ReadF(12), 2),
                                    TERMINATOR = (decimal)Math.Round(ReadF(13), 2),
                                    TERMINATOR_EK = (int)Math.Round(ReadF(14), 2),
                                    HACIM_SIRA = (int)Math.Round(ReadF(15), 2),
                                    BOGA_AYI_TL_GUN = (int)Math.Round(ReadF(16), 2),
                                    BOGA_AYI_USD_GUN = (int)Math.Round(ReadF(17), 2),
                                    BOGA_AYI_TL_HAFTA = (int)Math.Round(ReadF(18), 2),
                                    BOGA_AYI_USD_HAFTA = (int)Math.Round(ReadF(19), 2),
                                    BOGA_AYI_TL_AY = (int)Math.Round(ReadF(20), 2),
                                    BOGA_AYI_USD_AY = (int)Math.Round(ReadF(21), 2),
                                    P1M = (int)Math.Round(ReadF(22), 2),
                                    P5M = (int)Math.Round(ReadF(23), 2),
                                    P15M = (int)Math.Round(ReadF(24), 2),
                                    P20M = (int)Math.Round(ReadF(25), 2),
                                    P30M = (int)Math.Round(ReadF(26), 2),
                                    P60M = (int)Math.Round(ReadF(27), 2),
                                    P120M = (int)Math.Round(ReadF(28), 2),
                                    P240M = (int)Math.Round(ReadF(29), 2),
                                    PD = (int)Math.Round(ReadF(30), 2),
                                    PW = (int)Math.Round(ReadF(31), 2),
                                    PM = (int)Math.Round(ReadF(32), 2),
                                    EMA5 = (decimal)Math.Round(ReadF(33), 2),
                                    EMA21 = (decimal)Math.Round(ReadF(34), 2),
                                    EMA63 = (decimal)Math.Round(ReadF(35), 2),
                                    EMA126 = (decimal)Math.Round(ReadF(36), 2),
                                    EMA252 = (decimal)Math.Round(ReadF(37), 2),
                                    EMA756 = (decimal)Math.Round(ReadF(38), 2),
                                    EMA100 = (decimal)Math.Round(ReadF(39), 2),
                                    EMA200 = (decimal)Math.Round(ReadF(40), 2),
                                    RSI = (decimal)Math.Round(ReadF(41), 2),
                                    SRSI = (decimal)Math.Round(ReadF(42), 2)
                                });
                            }

                            // Sembol için DB'ye yaz
                            if (localInsert.Count > 0)
                            {
                                using (var db = new AlgobarDbContext())
                                    await db.InsertBarsAsync(localInsert).ConfigureAwait(false);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _ = TelegramHelper.SendMessageAsync("IMKB bardata okuma iptal edildi.");
                        }
                        catch (Exception ex)
                        {
                            await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + orj + "] " + ex.Message, ex));
                            _ = TelegramHelper.SendMessageAsync($"{orj} sembolü için bardata okunamadı: {ex.Message}");
                        }
                        finally
                        {
                            Interlocked.Increment(ref completed);
                            Update(completed, semboller.Count, orj, sw, bar);
                            limiter.Release();
                        }
                    }, ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            sw.Stop();
            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        public async Task IMKB_BarData_TekSembol_OkuAsync_Aralikli(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sistemler = new[] { User.MySistem5, User.MySistem6, User.MySistem7, User.MySistem8 };

            string sembolText = FrmMain.Reference.txtSembol.Text?.Trim();
            List<string> manuelSemboller = null;

            if (!string.IsNullOrWhiteSpace(sembolText))
            {
                manuelSemboller = sembolText
                    .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToUpper())
                    .Distinct()
                    .ToList();
            }

            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            if (manuelSemboller != null && manuelSemboller.Count > 0)
            {
                semboller = semboller
                    .Where(x => manuelSemboller.Contains(x.SEMBOL))
                    .ToList();
            }

            var selectedDate = FrmMain.Reference.txtStartCalendar.DateTime.Date;
            var endDate = (FrmMain.Reference.txtEndCalendar.EditValue == null || FrmMain.Reference.txtEndCalendar.DateTime == DateTime.MinValue) ? selectedDate : FrmMain.Reference.txtEndCalendar.DateTime.Date;

            if (endDate < selectedDate)
                endDate = selectedDate;

            const int minuteStep = 5;
            ResetProgress(bar, semboller.Count);

            var sw = Stopwatch.StartNew();

            // DB'de aynı gün için sembol -> Max(ZAMAN) sözlüğü
            Dictionary<string, DateTime> lastBarTimes;
            using (var db = new AlgobarDbContext())
            {
                var symbols = semboller.Select(s => s.SEMBOL).ToList();
                lastBarTimes = await db.GetLastBarTimesBySymbolAsync(selectedDate, symbols).ConfigureAwait(false);
            }

            var toInsert = new ConcurrentBag<AlgoBarDataAll>();
            int completed = 0;
            var limiter = new SemaphoreSlim(3);
            int batchSize = 4;

            for (int i = 0; i < semboller.Count; i += batchSize)
            {
                ct.ThrowIfCancellationRequested();
                var batch = semboller.Skip(i).Take(batchSize).ToList();
                var tasks = new List<Task>(batch.Count);

                for (int k = 0; k < batch.Count; k++)
                {
                    var localSbl = batch[k];
                    var localSistem = sistemler[k % sistemler.Length];

                    tasks.Add(Task.Run(async () =>
                    {
                        string orj = localSbl?.SEMBOL ?? string.Empty;

                        await limiter.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            if (string.IsNullOrWhiteSpace(orj)) return;

                            var barData = localSistem.GrafikVerileriniOku(orj, "1");
                            var cfg = AppConfig.Instance;
                            var sistemData = localSistem.SistemGetir(cfg.IMKBH_BarData_IdealName, orj, "1");
                            if (barData == null || barData.Count < 1) return;

                            int startIndex = FindStartIndex(barData, selectedDate);

                            DateTime lastTime;
                            var hasLast = lastBarTimes.TryGetValue(orj, out lastTime);

                            var localInsert = new List<AlgoBarDataAll>();

                            for (int barX = startIndex; barX < barData.Count; barX++)
                            {
                                var bItem = barData[barX];
                                var d = bItem.Date.Date;
                                if (d < selectedDate || d > endDate) continue;
                                if (hasLast && bItem.Date <= lastTime) continue;
                                if (minuteStep > 0 && (bItem.Date.Minute % minuteStep != 0)) continue;

                                Func<int, float> ReadF = idx =>
                                    (sistemData != null && barX >= 0)
                                        ? (float)sistemData.Cizgiler[idx].Deger[barX]
                                        : 0f;

                                localInsert.Add(new AlgoBarDataAll
                                {
                                    SEMBOL = orj,
                                    PERIYOT = minuteStep.ToString(),
                                    ZAMAN = bItem.Date,
                                    ACILIS = (decimal)Math.Round(barData[barX].Open, 2),
                                    YUKSEK = (decimal)Math.Round(barData[barX].High, 2),
                                    DUSUK = (decimal)Math.Round(barData[barX].Low, 2),
                                    KAPANIS = (decimal)Math.Round(barData[barX].Close, 2),
                                    HACIM = (decimal)Math.Round(barData[barX].Vol, 2),

                                    GEIST1 = (decimal)Math.Round(ReadF(0), 2),
                                    GEIST2 = (decimal)Math.Round(ReadF(1), 2),
                                    GEIST3 = (decimal)Math.Round(ReadF(2), 2),
                                    GEIST_TOPLAM = (decimal)Math.Round(ReadF(3), 2),
                                    MULTI_SISTEM_EK = (int)Math.Round(ReadF(4), 2),
                                    HACIM_YON = (decimal)Math.Round(ReadF(5), 2),
                                    HACIM_YON_EK = (int)Math.Round(ReadF(6), 2),
                                    MULTI_ROKET = (decimal)Math.Round(ReadF(7), 2),
                                    MULTI_ROKET_EK = (int)Math.Round(ReadF(8), 2),
                                    MULTI_EX = (decimal)Math.Round(ReadF(9), 2),
                                    MULTI_EX_EK = (int)Math.Round(ReadF(10), 2),
                                    GEIST_TR = (decimal)Math.Round(ReadF(11), 2),
                                    GEIST_TR_EK = (int)Math.Round(ReadF(12), 2),
                                    TERMINATOR = (decimal)Math.Round(ReadF(13), 2),
                                    TERMINATOR_EK = (int)Math.Round(ReadF(14), 2),
                                    HACIM_SIRA = (int)Math.Round(ReadF(15), 2),
                                    BOGA_AYI_TL_GUN = (int)Math.Round(ReadF(16), 2),
                                    BOGA_AYI_USD_GUN = (int)Math.Round(ReadF(17), 2),
                                    BOGA_AYI_TL_HAFTA = (int)Math.Round(ReadF(18), 2),
                                    BOGA_AYI_USD_HAFTA = (int)Math.Round(ReadF(19), 2),
                                    BOGA_AYI_TL_AY = (int)Math.Round(ReadF(20), 2),
                                    BOGA_AYI_USD_AY = (int)Math.Round(ReadF(21), 2),
                                    P1M = (int)Math.Round(ReadF(22), 2),
                                    P5M = (int)Math.Round(ReadF(23), 2),
                                    P15M = (int)Math.Round(ReadF(24), 2),
                                    P20M = (int)Math.Round(ReadF(25), 2),
                                    P30M = (int)Math.Round(ReadF(26), 2),
                                    P60M = (int)Math.Round(ReadF(27), 2),
                                    P120M = (int)Math.Round(ReadF(28), 2),
                                    P240M = (int)Math.Round(ReadF(29), 2),
                                    PD = (int)Math.Round(ReadF(30), 2),
                                    PW = (int)Math.Round(ReadF(31), 2),
                                    PM = (int)Math.Round(ReadF(32), 2),
                                    EMA5 = (decimal)Math.Round(ReadF(33), 2),
                                    EMA21 = (decimal)Math.Round(ReadF(34), 2),
                                    EMA63 = (decimal)Math.Round(ReadF(35), 2),
                                    EMA126 = (decimal)Math.Round(ReadF(36), 2),
                                    EMA252 = (decimal)Math.Round(ReadF(37), 2),
                                    EMA756 = (decimal)Math.Round(ReadF(38), 2),
                                    EMA100 = (decimal)Math.Round(ReadF(39), 2),
                                    EMA200 = (decimal)Math.Round(ReadF(40), 2),
                                    RSI = (decimal)Math.Round(ReadF(41), 2),
                                    SRSI = (decimal)Math.Round(ReadF(42), 2)
                                });
                            }

                            // Sembol için DB'ye yaz
                            if (localInsert.Count > 0)
                            {
                                using (var db = new AlgobarDbContext())
                                    await db.InsertBarsAsync(localInsert).ConfigureAwait(false);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            _ = TelegramHelper.SendMessageAsync("IMKB bardata okuma iptal edildi.");
                        }
                        catch (Exception ex)
                        {
                            await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + orj + "] " + ex.Message, ex));
                            _ = TelegramHelper.SendMessageAsync($"{orj} sembolü için bardata okunamadı: {ex.Message}");
                        }
                        finally
                        {
                            Interlocked.Increment(ref completed);
                            Update(completed, semboller.Count, orj, sw, bar);
                            limiter.Release();
                        }
                    }, ct));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }

            sw.Stop();
            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }
        #endregion

        #region ALGORITMA (KOMPOZIT)

        /// <summary>
        /// IMKB_KOMPOZIT_DATA tablosuna kısmi upsert eder.        
        /// </summary>
        public async Task IMKB_Kompozit_OkuAsync(ProgressBarControl bar, CancellationToken ct = default(CancellationToken))
        {
            var sistem = User.MySistem9;

            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            ResetProgress(bar, semboller.Count);
            var sw = Stopwatch.StartNew();
            int i = 0;

            var toSave = new List<KompozitData>(semboller.Count);

            foreach (var sbl in semboller)
            {
                ct.ThrowIfCancellationRequested();
                var orj = sbl != null ? sbl.SEMBOL : string.Empty;

                try
                {
                    var cfg = AppConfig.Instance;

                    // 3 sistemi paralel çalıştır
                    var kompozitTask = Task.Run(() => sistem.SistemGetir(cfg.KompozitSistem_IdealName, orj, "G"), ct);

                    await Task.WhenAll(kompozitTask).ConfigureAwait(false);

                    var kompozitSistem = kompozitTask.Result;                    

                    int ixKompozit = (kompozitSistem != null ? kompozitSistem.BarSayisi : 0) - 1;
                    

                    Func<dynamic, int, int, decimal> Read = (sys, ix, idx) =>
                        (sys != null && ix >= 0) ? (decimal)Math.Round((float)sys.Cizgiler[idx].Deger[ix], 2) : 0m;

                    var a = new KompozitData
                    {
                        SEMBOL = orj ?? string.Empty,                        
                        GUN_EMA5 = Read(kompozitSistem, ixKompozit, 0),
                        GUN_EMA13 = Read(kompozitSistem, ixKompozit, 1),
                        GUN_EMA21 = Read(kompozitSistem, ixKompozit, 2),
                        GUN_EMA34 = Read(kompozitSistem, ixKompozit, 3),
                        GUN_EMA55 = Read(kompozitSistem, ixKompozit, 4),
                        GUN_EMA63 = Read(kompozitSistem, ixKompozit, 5),
                        GUN_EMA89 = Read(kompozitSistem, ixKompozit, 6),
                        GUN_EMA100 = Read(kompozitSistem, ixKompozit, 7),
                        GUN_EMA126 = Read(kompozitSistem, ixKompozit, 8),
                        GUN_EMA144 = Read(kompozitSistem, ixKompozit, 9),
                        GUN_EMA200 = Read(kompozitSistem, ixKompozit, 10),
                        GUN_EMA233 = Read(kompozitSistem, ixKompozit, 11),
                        GUN_EMA252 = Read(kompozitSistem, ixKompozit, 12),
                        GUN_EMA756 = Read(kompozitSistem, ixKompozit, 13),
                        HAFTA_EMA5 = Read(kompozitSistem, ixKompozit, 14),
                        HAFTA_EMA13 = Read(kompozitSistem, ixKompozit, 15),
                        HAFTA_EMA21 = Read(kompozitSistem, ixKompozit, 16),
                        HAFTA_EMA34 = Read(kompozitSistem, ixKompozit, 17),
                        HAFTA_EMA55 = Read(kompozitSistem, ixKompozit, 18),
                        HAFTA_EMA63 = Read(kompozitSistem, ixKompozit, 19),
                        HAFTA_EMA89 = Read(kompozitSistem, ixKompozit, 20),
                        HAFTA_EMA100 = Read(kompozitSistem, ixKompozit, 21),
                        HAFTA_EMA126 = Read(kompozitSistem, ixKompozit, 22),
                        HAFTA_EMA144 = Read(kompozitSistem, ixKompozit, 23),
                        HAFTA_EMA200 = Read(kompozitSistem, ixKompozit, 24),
                        HAFTA_EMA233 = Read(kompozitSistem, ixKompozit, 25),
                        HAFTA_EMA252 = Read(kompozitSistem, ixKompozit, 26),
                        HAFTA_EMA756 = Read(kompozitSistem, ixKompozit, 27),
                        AY_EMA5 = Read(kompozitSistem, ixKompozit, 28),
                        AY_EMA13 = Read(kompozitSistem, ixKompozit, 29),
                        AY_EMA21 = Read(kompozitSistem, ixKompozit, 30),
                        AY_EMA34 = Read(kompozitSistem, ixKompozit, 31),
                        AY_EMA55 = Read(kompozitSistem, ixKompozit, 32),
                        AY_EMA63 = Read(kompozitSistem, ixKompozit, 33),
                        AY_EMA89 = Read(kompozitSistem, ixKompozit, 34),
                        AY_EMA100 = Read(kompozitSistem, ixKompozit, 35),
                        AY_EMA126 = Read(kompozitSistem, ixKompozit, 36),
                        AY_EMA144 = Read(kompozitSistem, ixKompozit, 37),
                        AY_EMA200 = Read(kompozitSistem, ixKompozit, 38),
                        AY_EMA233 = Read(kompozitSistem, ixKompozit, 39),
                        AY_EMA252 = Read(kompozitSistem, ixKompozit, 40),
                        AY_EMA756 = Read(kompozitSistem, ixKompozit, 41),
                        PD = Read(kompozitSistem, ixKompozit, 42),
                        PW = Read(kompozitSistem, ixKompozit, 43),
                        PM = Read(kompozitSistem, ixKompozit, 44),
                        BOGA_AYI_GUN = Read(kompozitSistem, ixKompozit, 45),
                        BOGA_AYI_HAFTA = Read(kompozitSistem, ixKompozit, 46),
                        BOGA_AYI_AY = Read(kompozitSistem, ixKompozit, 47)
                    };

                    toSave.Add(a);
                }
                catch (OperationCanceledException)
                {
                    _ = TelegramHelper.SendMessageAsync("IMKB kopmozit veri okuma iptal edildi.");
                }
                catch (Exception ex)
                {
                    await ErrorLogger.LogAsync(new Exception("[SEMBOL: " + orj + "] " + ex.Message, ex));
                    _ = TelegramHelper.SendMessageAsync(orj + " sembolü için kompozit veri okunamadı: " + ex.Message);
                }
                finally
                {
                    i++;
                    Update(i, semboller.Count, orj, sw, bar);
                }
            }

            if (toSave.Count > 0)
            {
                try
                {
                    using (var db = new GeistDbContext())
                    {
                        await db.UpsertKompozitAsync(toSave).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {

                    _ = TelegramHelper.SendMessageAsync("IMKB kompozit veri DB yazma hatası: " + ex.Message);
                }

            }

            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        #endregion

        #region UYUMSUZLUK TARAMA (RSI DIVERGENCE)

        /// <summary>
        /// Tüm sembollerde RSI divergence taraması yapar.
        /// Her sembol için 11 periyot paralel işlenir.
        /// </summary>
        public async Task IMKB_Uyumsuzluk_OkuAsync(ProgressBarControl bar, CancellationToken ct = default)
        {
            var sistem = User.MySistem10;            

            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            // TABLOYU TEMİZLE
            try
            {
                using (var db = new GeistDbContext())
                {
                    await db.ClearUyumsuzlukAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await ErrorLogger.LogAsync(new Exception("Tablo temizleme hatası: " + ex.Message, ex));
                _ = TelegramHelper.SendMessageAsync("Uyumsuzluk tablosu temizleme hatası!");
                return;
            }

            ResetProgress(bar, semboller.Count);
            var sw = Stopwatch.StartNew();
            var taramaZamani = DateTime.Now;

            var tumSonuclar = new ConcurrentBag<TaramaUyumsuzluk>();
            var limiter = new SemaphoreSlim(3);
            int completed = 0;

            var periyotlar = new[] { "5", "15", "20", "30", "60", "120", "240", "G"};

            var tasks = semboller.Select(sbl => Task.Run(async () =>
            {
                await limiter.WaitAsync(ct).ConfigureAwait(false);
                var orj = sbl?.SEMBOL ?? string.Empty;

                try
                {
                    // Her periyot için tarama yap
                    foreach (var periyot in periyotlar)
                    {
                        var sonuclar = TaramaUyumsuzluk(sistem, orj, periyot, taramaZamani);
                        if (sonuclar != null && sonuclar.Count > 0)
                        {
                            foreach (var item in sonuclar)
                            {
                                tumSonuclar.Add(item);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _ = TelegramHelper.SendMessageAsync("Uyumsuzluk tarama iptal edildi.");
                }
                catch (Exception ex)
                {
                    await ErrorLogger.LogAsync(new Exception($"[{orj}] {ex.Message}", ex));
                }
                finally
                {
                    Interlocked.Increment(ref completed);
                    Update(completed, semboller.Count, orj, sw, bar);
                    limiter.Release();
                }
            }, ct)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            // DB'ye toplu yazma
            if (!tumSonuclar.IsEmpty)
            {
                try
                {
                    using (var db = new GeistDbContext())
                    {
                        await db.InsertTaramaUyumsuzlukAsync(tumSonuclar.ToList()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _ = TelegramHelper.SendMessageAsync("Uyumsuzluk DB yazma hatası: " + ex.Message);
                }
            }

            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        /// <summary>
        /// Tek bir sembol ve periyot için RSI divergence taraması yapar.
        /// </summary>
        private List<TaramaUyumsuzluk> TaramaUyumsuzluk(dynamic sistem, string sembol, string periyot, DateTime taramaZamani)
        {
            var sonuclar = new List<TaramaUyumsuzluk>();

            try
            {
                const int RSIPeriod = 14;
                const int TepeDipPencere = 5;
                const int MinBarArasi = 10;
                const float MinGuc = 2.0F;
                const int Tolerans = 3;
                const float RSIAsiriAlimSeviye = 70.0F;

                int MaxBarArasi = 50;
                if (periyot == "1" || periyot == "5" || periyot == "10" || periyot == "15")
                    MaxBarArasi = 200;
                else if (periyot == "20")
                    MaxBarArasi = 100;

                var V = sistem.GrafikVerileriniOku(sembol, periyot);
                if (V == null || V.Count < (TepeDipPencere * 2 + 20)) return sonuclar;

                var RSI = sistem.RSI(V, RSIPeriod);
                if (RSI == null) return sonuclar;

                int BarSayisi = V.Count;
                List<int> FiyatTepeleri = new List<int>();
                List<int> FiyatDipleri = new List<int>();
                List<int> RSITepeleri = new List<int>();
                List<int> RSIDipleri = new List<int>();

                int BaslangicBar = Math.Max(TepeDipPencere, BarSayisi - 100);

                for (int bar = BaslangicBar; bar < BarSayisi - TepeDipPencere; bar++)
                {
                    bool fiyatTepe = true;
                    for (int i = bar - TepeDipPencere; i <= bar + TepeDipPencere; i++)
                    {
                        if (i != bar && V[i].High >= V[bar].High)
                        {
                            fiyatTepe = false;
                            break;
                        }
                    }
                    if (fiyatTepe) FiyatTepeleri.Add(bar);

                    bool fiyatDip = true;
                    for (int i = bar - TepeDipPencere; i <= bar + TepeDipPencere; i++)
                    {
                        if (i != bar && V[i].Low <= V[bar].Low)
                        {
                            fiyatDip = false;
                            break;
                        }
                    }
                    if (fiyatDip) FiyatDipleri.Add(bar);

                    bool rsiTepe = true;
                    for (int i = bar - TepeDipPencere; i <= bar + TepeDipPencere; i++)
                    {
                        if (i != bar && RSI[i] >= RSI[bar])
                        {
                            rsiTepe = false;
                            break;
                        }
                    }
                    if (rsiTepe) RSITepeleri.Add(bar);

                    bool rsiDip = true;
                    for (int i = bar - TepeDipPencere; i <= bar + TepeDipPencere; i++)
                    {
                        if (i != bar && RSI[i] <= RSI[bar])
                        {
                            rsiDip = false;
                            break;
                        }
                    }
                    if (rsiDip) RSIDipleri.Add(bar);
                }

                // POZİTİF DIVERGENCE
                if (FiyatDipleri.Count >= 2)
                {
                    int sonFiyatDip = FiyatDipleri[FiyatDipleri.Count - 1];
                    int oncekiFiyatDip = FiyatDipleri[FiyatDipleri.Count - 2];
                    int barArasi = sonFiyatDip - oncekiFiyatDip;

                    if (barArasi >= MinBarArasi && barArasi <= MaxBarArasi)
                    {
                        if (V[sonFiyatDip].Low < V[oncekiFiyatDip].Low)
                        {
                            int sonRSIDip = -1;
                            int oncekiRSIDip = -1;

                            for (int i = 0; i < RSIDipleri.Count; i++)
                            {
                                if (Math.Abs(RSIDipleri[i] - sonFiyatDip) <= Tolerans)
                                {
                                    sonRSIDip = RSIDipleri[i];
                                    break;
                                }
                            }

                            for (int i = 0; i < RSIDipleri.Count; i++)
                            {
                                if (Math.Abs(RSIDipleri[i] - oncekiFiyatDip) <= Tolerans)
                                {
                                    oncekiRSIDip = RSIDipleri[i];
                                    break;
                                }
                            }

                            if (sonRSIDip != -1 && oncekiRSIDip != -1 && RSI[sonRSIDip] > RSI[oncekiRSIDip])
                            {
                                double fiyatDususYuzde = ((V[oncekiFiyatDip].Low - V[sonFiyatDip].Low) / V[oncekiFiyatDip].Low) * 100.0;
                                double rsiYukselisYuzde = ((RSI[sonRSIDip] - RSI[oncekiRSIDip]) / RSI[oncekiRSIDip]) * 100.0;
                                double guc = fiyatDususYuzde + rsiYukselisYuzde;

                                if (guc >= MinGuc)
                                {
                                    double divergenceSonFiyat = V[sonFiyatDip].Low;
                                    double maksimumArtisYuzde = 0.0;

                                    for (int k = sonFiyatDip + 1; k < BarSayisi; k++)
                                    {
                                        double artisYuzde = ((V[k].High - divergenceSonFiyat) / divergenceSonFiyat) * 100.0;
                                        if (artisYuzde > maksimumArtisYuzde)
                                            maksimumArtisYuzde = artisYuzde;
                                    }

                                    if (maksimumArtisYuzde <= 10.0)
                                    {
                                        bool pozitifIptalOldu = false;
                                        double divergenceSonRSI = RSI[sonRSIDip];
                                        double divergenceSonDip = V[sonFiyatDip].Low;

                                        for (int k = sonFiyatDip + 1; k < BarSayisi; k++)
                                        {
                                            if (RSI[k] < divergenceSonRSI && V[k].Low < divergenceSonDip)
                                            {
                                                pozitifIptalOldu = true;
                                                break;
                                            }
                                        }

                                        if (!pozitifIptalOldu)
                                        {
                                            sonuclar.Add(new TaramaUyumsuzluk
                                            {
                                                SEMBOL = sembol ?? "",
                                                PERIYOT = periyot ?? "",
                                                ILK_TARIH = V[oncekiFiyatDip].Date,
                                                SON_TARIH = V[sonFiyatDip].Date,
                                                UYUMSUZLUK_TIPI = true,
                                                ILK_FIYAT = (decimal)Math.Round(V[oncekiFiyatDip].Low, 4),
                                                SON_FIYAT = (decimal)Math.Round(V[sonFiyatDip].Low, 4),
                                                FIYAT_DEGISIM = (decimal)Math.Round(-fiyatDususYuzde, 4),
                                                ILK_RSI = (decimal)Math.Round(RSI[oncekiRSIDip], 4),
                                                SON_RSI = (decimal)Math.Round(RSI[sonRSIDip], 4),
                                                RSI_DEGISIM = (decimal)Math.Round(rsiYukselisYuzde, 4),
                                                GUC = (decimal)Math.Round(guc, 4),
                                                TARAMA_ZAMANI = taramaZamani
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // NEGATİF DIVERGENCE
                if (FiyatTepeleri.Count >= 2)
                {
                    int sonFiyatTepe = FiyatTepeleri[FiyatTepeleri.Count - 1];
                    int oncekiFiyatTepe = FiyatTepeleri[FiyatTepeleri.Count - 2];
                    int barArasi = sonFiyatTepe - oncekiFiyatTepe;

                    if (barArasi >= MinBarArasi && barArasi <= MaxBarArasi)
                    {
                        if (V[sonFiyatTepe].High > V[oncekiFiyatTepe].High)
                        {
                            int sonRSITepe = -1;
                            int oncekiRSITepe = -1;

                            for (int i = 0; i < RSITepeleri.Count; i++)
                            {
                                if (Math.Abs(RSITepeleri[i] - sonFiyatTepe) <= Tolerans)
                                {
                                    sonRSITepe = RSITepeleri[i];
                                    break;
                                }
                            }

                            for (int i = 0; i < RSITepeleri.Count; i++)
                            {
                                if (Math.Abs(RSITepeleri[i] - oncekiFiyatTepe) <= Tolerans)
                                {
                                    oncekiRSITepe = RSITepeleri[i];
                                    break;
                                }
                            }

                            if (sonRSITepe != -1 && oncekiRSITepe != -1 && RSI[sonRSITepe] < RSI[oncekiRSITepe])
                            {
                                bool rsiFiltreGecti = true;
                                if (RSI[oncekiRSITepe] > RSIAsiriAlimSeviye && RSI[sonRSITepe] >= RSIAsiriAlimSeviye)
                                {
                                    rsiFiltreGecti = false;
                                }

                                if (rsiFiltreGecti)
                                {
                                    double fiyatYukselisYuzde = ((V[sonFiyatTepe].High - V[oncekiFiyatTepe].High) / V[oncekiFiyatTepe].High) * 100.0;
                                    double rsiDususYuzde = ((RSI[oncekiRSITepe] - RSI[sonRSITepe]) / RSI[oncekiRSITepe]) * 100.0;
                                    double guc = fiyatYukselisYuzde + rsiDususYuzde;

                                    if (guc >= MinGuc)
                                    {
                                        double divergenceSonFiyat = V[sonFiyatTepe].High;
                                        double maksimumDususYuzde = 0.0;

                                        for (int k = sonFiyatTepe + 1; k < BarSayisi; k++)
                                        {
                                            double dususYuzde = ((divergenceSonFiyat - V[k].Low) / divergenceSonFiyat) * 100.0;
                                            if (dususYuzde > maksimumDususYuzde)
                                                maksimumDususYuzde = dususYuzde;
                                        }

                                        if (maksimumDususYuzde <= 10.0)
                                        {
                                            bool negatifIptalOldu = false;
                                            double divergenceSonRSI = RSI[sonRSITepe];
                                            double divergenceSonTepe = V[sonFiyatTepe].High;

                                            for (int k = sonFiyatTepe + 1; k < BarSayisi; k++)
                                            {
                                                if (RSI[k] > divergenceSonRSI && V[k].High > divergenceSonTepe)
                                                {
                                                    negatifIptalOldu = true;
                                                    break;
                                                }
                                            }

                                            if (!negatifIptalOldu)
                                            {
                                                sonuclar.Add(new TaramaUyumsuzluk
                                                {
                                                    SEMBOL = sembol ?? "",
                                                    PERIYOT = periyot ?? "",
                                                    ILK_TARIH = V[oncekiFiyatTepe].Date,
                                                    SON_TARIH = V[sonFiyatTepe].Date,
                                                    UYUMSUZLUK_TIPI = false,
                                                    ILK_FIYAT = (decimal)Math.Round(V[oncekiFiyatTepe].High, 4),
                                                    SON_FIYAT = (decimal)Math.Round(V[sonFiyatTepe].High, 4),
                                                    FIYAT_DEGISIM = (decimal)Math.Round(fiyatYukselisYuzde, 4),
                                                    ILK_RSI = (decimal)Math.Round(RSI[oncekiRSITepe], 4),
                                                    SON_RSI = (decimal)Math.Round(RSI[sonRSITepe], 4),
                                                    RSI_DEGISIM = (decimal)Math.Round(-rsiDususYuzde, 4),
                                                    GUC = (decimal)Math.Round(guc, 4),
                                                    TARAMA_ZAMANI = taramaZamani
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ErrorLogger.LogAsync(new Exception($"[{sembol}-{periyot}] {ex.Message}", ex));
            }

            return sonuclar;
        }

        #endregion

        #region RSI SINYAL TARAMA

        /// <summary>
        /// Tüm sembollerde RSI sinyal sistemi taraması yapar.
        /// Her sembol için 11 periyot paralel işlenir.
        /// </summary>
        public async Task IMKB_RSISinyal_OkuAsync(ProgressBarControl bar, CancellationToken ct = default)
        {
            var sistem = User.MySistem11;            

            var semboller = await SembolListesiniOkuAsync().ConfigureAwait(false);
            if (semboller == null || semboller.Count == 0)
            {
                ResetProgress(bar, 1);
                return;
            }

            // TABLOYU TEMİZLE
            try
            {
                using (var db = new GeistDbContext())
                {
                    await db.ClearTaramaSRSIAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await ErrorLogger.LogAsync(new Exception("Tablo temizleme hatası: " + ex.Message, ex));
                _ = TelegramHelper.SendMessageAsync("RSI sinyal tablosu temizleme hatası!");
                return;
            }

            ResetProgress(bar, semboller.Count);
            var sw = Stopwatch.StartNew();
            var taramaZamani = DateTime.Now;

            var tumSonuclar = new ConcurrentBag<TaramaSRSI>();
            var limiter = new SemaphoreSlim(3);
            int completed = 0;

            var periyotlar = new[] { "1", "5", "15", "20", "30", "60", "120", "240", "G"};

            var tasks = semboller.Select(sbl => Task.Run(async () =>
            {
                await limiter.WaitAsync(ct).ConfigureAwait(false);
                var orj = sbl?.SEMBOL ?? string.Empty;

                try
                {
                    // Her periyot için tarama yap
                    foreach (var periyot in periyotlar)
                    {
                        var sonuc = TaramaSRSISinyal(sistem, orj, periyot, taramaZamani);
                        if (sonuc != null)
                        {
                            tumSonuclar.Add(sonuc);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _ = TelegramHelper.SendMessageAsync("RSI sinyal tarama iptal edildi.");
                }
                catch (Exception ex)
                {
                    await ErrorLogger.LogAsync(new Exception($"[{orj}] {ex.Message}", ex));
                }
                finally
                {
                    Interlocked.Increment(ref completed);
                    Update(completed, semboller.Count, orj, sw, bar);
                    limiter.Release();
                }
            }, ct)).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);

            // DB'ye toplu yazma
            if (!tumSonuclar.IsEmpty)
            {
                try
                {
                    using (var db = new GeistDbContext())
                    {
                        await db.InsertTaramaSRSIAsync(tumSonuclar.ToList()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _ = TelegramHelper.SendMessageAsync("RSI sinyal DB yazma hatası: " + ex.Message);
                }
            }

            Update(semboller.Count, semboller.Count, "Bitti", sw, bar);
        }

        /// <summary>
        /// Tek bir sembol ve periyot için RSI sinyal taraması yapar.
        /// </summary>
        private TaramaSRSI TaramaSRSISinyal(dynamic sistem, string sembol, string periyot, DateTime taramaZamani)
        {
            try
            {
                const double Tolerans = 0.10;
                const int SRSI_Periyot = 14;
                const int EMA_Periyot = 10;
                const double StopYuzde = 0.05;
                const double DegisimFiltresi = 20.0;

                string ustPeriyot = "H";
                switch (periyot)
                {
                    case "1": ustPeriyot = "5"; break;
                    case "5": ustPeriyot = "15"; break;
                    case "15": ustPeriyot = "20"; break;
                    case "20": ustPeriyot = "30"; break;
                    case "30": ustPeriyot = "60"; break;
                    case "60": ustPeriyot = "120"; break;
                    case "120": ustPeriyot = "240"; break;
                    case "240": ustPeriyot = "G"; break;
                    case "G": ustPeriyot = "H"; break;
                    case "H":
                    case "A": ustPeriyot = "A"; break;
                }

                var V = sistem.GrafikVerileriniOku(sembol, periyot);
                if (V == null || V.Count < 30) return null;

                var RSI = sistem.RSI(V, 14);
                if (RSI == null) return null;

                var DataUst = sistem.GrafikVerileriniOku(sembol, ustPeriyot);
                if (DataUst == null || DataUst.Count < 30) return null;

                var SRSI_Ust = sistem.StochasticRSI(DataUst, SRSI_Periyot);
                if (SRSI_Ust == null) return null;

                var SRSI_EMA_Ust = sistem.MA(SRSI_Ust, "Exp", EMA_Periyot);
                if (SRSI_EMA_Ust == null) return null;

                var SRSI = sistem.DonemCevir(V, DataUst, SRSI_Ust);
                var SRSI_EMA = sistem.DonemCevir(V, DataUst, SRSI_EMA_Ust);

                if (SRSI == null || SRSI_EMA == null) return null;

                int BarSayisi = V.Count;

                var UstBant = new List<double>();
                var AltBant = new List<double>();

                for (int i = 0; i < BarSayisi; i++)
                {
                    UstBant.Add(SRSI[i] * (1 + Tolerans));
                    AltBant.Add(SRSI[i] * (1 - Tolerans));
                }

                string SonYon = "";
                double GirisFiyati = 0;
                string FlatOncesiSinyal = "";
                double FlatOncesiSinyalFiyati = 0;

                int sonSinyalIndex = -1;
                string sonSinyal = "";
                double sonRSI = 0, sonSRSI = 0, sonSRSI_EMA = 0, sonFiyat = 0;
                DateTime sonTarih = DateTime.MinValue;

                for (int i = 1; i < BarSayisi; i++)
                {
                    string sinyal = "";

                    if (SonYon == "A")
                    {
                        double Dusus = (GirisFiyati - V[i].Close) / GirisFiyati;
                        if (Dusus >= StopYuzde)
                        {
                            FlatOncesiSinyal = "A";
                            FlatOncesiSinyalFiyati = GirisFiyati;
                            SonYon = "F";
                            sinyal = "F";
                        }
                    }
                    else if (SonYon == "S")
                    {
                        double Yukseli = (V[i].Close - GirisFiyati) / GirisFiyati;
                        if (Yukseli >= StopYuzde)
                        {
                            FlatOncesiSinyal = "S";
                            FlatOncesiSinyalFiyati = GirisFiyati;
                            SonYon = "F";
                            sinyal = "F";
                        }
                    }

                    bool AL = RSI[i] < AltBant[i] && RSI[i - 1] >= AltBant[i - 1];
                    bool SAT = RSI[i] > UstBant[i] && RSI[i - 1] <= UstBant[i - 1];

                    if (SonYon != "F")
                    {
                        if (AL && SonYon != "A")
                        {
                            SonYon = "A";
                            GirisFiyati = V[i].Close;
                            sinyal = "A";
                        }
                        else if (SAT && SonYon != "S")
                        {
                            SonYon = "S";
                            GirisFiyati = V[i].Close;
                            sinyal = "S";
                        }
                    }
                    else
                    {
                        if (AL && FlatOncesiSinyal != "A")
                        {
                            SonYon = "A";
                            GirisFiyati = V[i].Close;
                            sinyal = "A";
                            FlatOncesiSinyal = "";
                        }
                        else if (SAT && FlatOncesiSinyal != "S")
                        {
                            SonYon = "S";
                            GirisFiyati = V[i].Close;
                            sinyal = "S";
                            FlatOncesiSinyal = "";
                        }
                        else if (FlatOncesiSinyal == "S" && V[i].Close < FlatOncesiSinyalFiyati)
                        {
                            SonYon = "S";
                            GirisFiyati = V[i].Close;
                            sinyal = "S";
                            FlatOncesiSinyal = "";
                        }
                        else if (FlatOncesiSinyal == "A" && V[i].Close > FlatOncesiSinyalFiyati)
                        {
                            SonYon = "A";
                            GirisFiyati = V[i].Close;
                            sinyal = "A";
                            FlatOncesiSinyal = "";
                        }
                    }

                    if (!string.IsNullOrEmpty(sinyal))
                    {
                        sonSinyalIndex = i;
                        sonSinyal = sinyal;
                        sonRSI = RSI[i];
                        sonSRSI = SRSI[i];
                        sonSRSI_EMA = SRSI_EMA[i];
                        sonFiyat = V[i].Close;
                        sonTarih = V[i].Date;
                    }
                }

                if (sonSinyalIndex == -1) return null;

                double fiyatBugun = V[BarSayisi - 1].Close;
                double degisim = ((fiyatBugun - sonFiyat) / sonFiyat) * 100.0;

                if (Math.Abs(degisim) > DegisimFiltresi) return null;

                int gun = (V[BarSayisi - 1].Date - sonTarih).Days;

                byte sinyalTuru = 2;
                if (sonSinyal == "A") sinyalTuru = 1;
                else if (sonSinyal == "S") sinyalTuru = 0;

                return new TaramaSRSI
                {
                    SEMBOL = sembol ?? "",
                    PERIYOT = periyot ?? "",
                    SINYAL_TARIHI = sonTarih,
                    SINYAL_TURU = sinyalTuru,
                    GUN_SAYISI = gun,
                    RSI_DEGERI = (decimal)Math.Round(sonRSI, 2),
                    SRSI_DEGERI = (decimal)Math.Round(sonSRSI, 2),
                    SRSI_EMA_DEGERI = (decimal)Math.Round(sonSRSI_EMA, 2),
                    GIRIS_FIYATI = (decimal)Math.Round(sonFiyat, 4),
                    GUNCEL_FIYAT = (decimal)Math.Round(fiyatBugun, 4),
                    DEGISIM_YUZDESI = (decimal)Math.Round(degisim, 2),
                    TARAMA_ZAMANI = taramaZamani
                };
            }
            catch (Exception ex)
            {
                _ = ErrorLogger.LogAsync(new Exception($"[{sembol}-{periyot}] {ex.Message}", ex));
                return null;
            }
        }

        #endregion
    }
}
