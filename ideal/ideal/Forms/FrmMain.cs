using DevExpress.XtraEditors;
using ideal.Database;
using ideal.Helper;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace ideal.Forms
{
    public partial class FrmMain : XtraForm
    {
        public static FrmMain Reference = null;

        private IdealHelper _idealHelper;
        private CancellationTokenSource _cts;
        private readonly WinFormsTimer _timer;
        private readonly List<JobSlot> _jobSlots = new List<JobSlot>();
        private GeistDbContext _db;
        private AlgobarDbContext _algobarDb;

        private sealed class JobSlot
        {
            public string Name { get; set; }
            public Func<CancellationToken, Task> Job { get; set; }

            // Zamanlama
            public TimeSpan StartLocal { get; set; }
            public TimeSpan EndLocal { get; set; }
            public int IntervalSeconds { get; set; }

            // Durum
            public Task Running;
            public DateTime NextRunUtc;
            public bool HasOverdue;
            public bool FinalAfterCloseDone;
            public bool ForceMode { get; set; }
        }

        public FrmMain()
        {
            InitializeComponent();



            Reference = this;
            _idealHelper = new IdealHelper();

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnStop.Enabled = false;

            _timer = new WinFormsTimer { Interval = 1000 };
            _timer.Tick += (s, e) => TickScheduler();

            txtStartCalendar.DateTime = DateTime.Today;

            try
            {
                _db = new GeistDbContext();
                _db.Database.Connection.Open();

                checkEditGeistContext.Checked = true;

                _algobarDb = new AlgobarDbContext();
                _algobarDb.Database.Connection.Open();

                checkEditAlgobarDataContext.Checked = true;

                
            }
            catch (Exception ex)
            {
                checkEditGeistContext.Checked = false;
                XtraMessageBox.Show("Bağlantı hatası:\n" + ex.Message);
                _db?.Dispose();
                _db = null;
            }

        }




        private void BtnStart_Click(object sender, EventArgs e)
        {


            btnStart.Enabled = false;
            btnStop.Enabled = true;

            DateTime selectedDate = txtStartCalendar.DateTime.Date;

            if (selectedDate > DateTime.Today)
            {
                XtraMessageBox.Show(this, "Lütfen bir tarih seçiniz.", "Geçersiz Tarih", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                return;
            }

            if (_cts != null) _cts.Dispose();
            _cts = new CancellationTokenSource();

            BuildJobs();
            InitializeNextRuns();
            _timer.Start();
            TickScheduler();
            _ = TelegramHelper.SendMessageAsync("🚀 Geist User DLL çalışmaya başladı.");

        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            _timer.Stop();
            if (_cts != null) _cts.Cancel();
            btnStart.Enabled = true;

            _ = TelegramHelper.SendMessageAsync("✅ Geist User DLL çalışmayı bitirdi..");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timer.Stop();
            _cts?.Cancel();

            if (_db != null)
            {
                if (_db.Database.Connection.State == System.Data.ConnectionState.Open)
                    _db.Database.Connection.Close();

                _db.Dispose();
                _db = null;
            }
            _ = TelegramHelper.SendMessageAsync("✖️ Geist User DLL kapandı. Bilginiz dışında ise kontrol ediniz.");
            base.OnFormClosing(e);
        }

        #region Yardımcılar

        private static void NormalizeSlot(JobSlot slot)
        {
            if (slot.IntervalSeconds < 1) slot.IntervalSeconds = 1;
        }

        private static DateTime NextAlignedLocal(DateTime nowLocal, DateTime today, TimeSpan startLocal, TimeSpan endLocal, int intervalSeconds)
        {
            if (intervalSeconds < 1) intervalSeconds = 1;

            DateTime anchor = today + startLocal;
            DateTime close = today + endLocal;

            if (nowLocal <= anchor) return anchor;

            double elapsedSec = (nowLocal - anchor).TotalSeconds;
            double k = Math.Ceiling(elapsedSec / intervalSeconds);
            DateTime cand = anchor.AddSeconds(k * intervalSeconds);

            return cand < close ? cand : today.AddDays(1) + startLocal;
        }

        #endregion

        #region Is Planlama

        private void BuildJobs()
        {
            _jobSlots.Clear();

            Action<string, Func<CancellationToken, Task>, TimeSpan, TimeSpan, int, bool> AddJob =
                (name, job, _start, _end, interval, enabled) =>
                {
                    if (!enabled) return;
                    _jobSlots.Add(new JobSlot
                    {
                        Name = name,
                        Job = job,
                        StartLocal = _start,
                        EndLocal = _end,
                        IntervalSeconds = interval
                    });
                };

            var start = new TimeSpan(9, 55, 30);
            var end = new TimeSpan(18, 25, 0);

            var start2 = new TimeSpan(18, 15, 30);
            var end2 = new TimeSpan(18, 45, 0);

            AddJob("IMKB - Yüzeysel",
                ct => _idealHelper.IMKB_YuzeyselVeri_OkuAsync(progressBarImkbYuzeyselVeri, ct),
                start, end, 30, btnImkbYuzeyselVeri.Checked);


            AddJob("IMKB - DD",
                ct => _idealHelper.IMKB_DD_OkuAsync(progressBarDD, ct),
                start, end, 120 * 60, btnImkbDD.Checked);


            AddJob("IMKB - Günlük Bar Data",
                ct => _idealHelper.IMKB_BarData_OkuAsync(progressBarImkbBarData, ct),
                start, end, 5 * 60, btnImkbBarData.Checked);

            AddJob("IMKB - Günlük Bar Data Gun Sonu",
                ct => _idealHelper.IMKB_BarData_GunSonu_OkuAsync(progressBarImkbBarDataGunSonu, ct),
                start2, end2, 5 * 60, btnImkbBarDataGunSonu.Checked);

            AddJob("IMKB - Kompozit Data",
              ct => _idealHelper.IMKB_Kompozit_OkuAsync(progressBarKompozitData, ct),
              start, end, 120 * 60, btnImkbKompozitData.Checked);

            AddJob("IMKB - Tarama Uyumsuzluklar",
                ct => _idealHelper.IMKB_Uyumsuzluk_OkuAsync(progressBarTaramaUyumsuzluk, ct),
                start, end, 1 * 60, btnImkbTaramaUyumsuzluk.Checked);

            AddJob("IMKB - Tarama SRSI",
                ct => _idealHelper.IMKB_RSISinyal_OkuAsync(progressBarTaramaSRSI, ct),
                start, end, 1 * 60, btnImkbTaramaSRSI.Checked);

            AddJob("IMKB - Bar Data Aralikli",
                ct => _idealHelper.IMKB_BarData_OkuAsync_Aralikli(progressBarImkbBarDataSembol, ct),
                start, end, 600 * 60, btnImkbBarDataSembol.Checked);

            AddJob("IMKB - Bar Data Tek Sembol",
                ct => _idealHelper.IMKB_BarData_TekSembol_OkuAsync_Aralikli(progressBarImkbBarDataTekSembol, ct),
                start, end, 600 * 60, btnImkbBarDataTekSembol.Checked);


        }

        #endregion

        #region Zamanlama

        private void InitializeNextRuns()
        {
            TimeZoneInfo tz = TimeZoneInfo.Local;
            DateTime nowUtc = DateTime.UtcNow;
            DateTime nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
            DateTime today = nowLocal.Date;

            bool force = checkEditSeansClosed.Checked;

            foreach (var slot in _jobSlots)
            {
                NormalizeSlot(slot);

                slot.HasOverdue = false;
                slot.FinalAfterCloseDone = false;
                slot.ForceMode = force;

                if (force)
                {
                    // Koşulsuz başlat: pencereyi yok say, hemen çalıştır, sonraki = now + interval
                    slot.StartLocal = nowLocal.TimeOfDay;   // görsel/diagnostic amaçlı güncel başlat saati
                    slot.NextRunUtc = nowUtc;
                    continue;
                }

                DateTime startTodayLocal = today + slot.StartLocal;
                DateTime endTodayLocal = today + slot.EndLocal;

                if (nowLocal < startTodayLocal)
                {
                    slot.NextRunUtc = TimeZoneInfo.ConvertTimeToUtc(startTodayLocal, tz);
                }
                else if (nowLocal >= endTodayLocal)
                {
                    slot.NextRunUtc = nowUtc; // kapanış sonrası son kez
                    slot.FinalAfterCloseDone = false;
                }
                else
                {
                    slot.NextRunUtc = nowUtc; // pencere içindeyse hemen
                }
            }
        }

        #endregion

        #region Ana Döngü   

        private void TickScheduler()
        {
            if (_cts == null || _cts.IsCancellationRequested) return;

            var tz = TimeZoneInfo.Local;
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
            var today = nowLocal.Date;

            foreach (var slot in _jobSlots)
            {
                bool isIdle = slot.Running == null || slot.Running.IsCompleted;
                bool dueNow = nowUtc >= slot.NextRunUtc.AddMilliseconds(-500);

                // ---- Zorla mod: pencere yok, sadece interval ----
                if (slot.ForceMode)
                {
                    if (!isIdle && dueNow)
                    {
                        slot.HasOverdue = true;
                        continue;
                    }

                    if (isIdle && dueNow)
                    {
                        RunJob(slot);
                        // sıradaki tetikleme: now + interval
                        slot.NextRunUtc = DateTime.UtcNow.AddSeconds(
                            Math.Max(1, slot.IntervalSeconds));
                    }

                    // bu slot için normal pencere mantığına girmeden devam
                    continue;
                }

                // ---- Normal mod: mevcut pencere mantığı ----
                DateTime startToday = today + slot.StartLocal;
                DateTime endToday = today + slot.EndLocal;

                if (nowLocal < startToday)
                {
                    slot.NextRunUtc = TimeZoneInfo.ConvertTimeToUtc(startToday, tz);
                    continue;
                }

                if (nowLocal >= endToday)
                {
                    if (!slot.FinalAfterCloseDone && isIdle)
                    {
                        RunJob(slot);
                        slot.FinalAfterCloseDone = true;
                    }
                    slot.NextRunUtc = TimeZoneInfo.ConvertTimeToUtc(today.AddDays(1) + slot.StartLocal, tz);
                    slot.HasOverdue = false;
                    continue;
                }

                if (!isIdle && dueNow)
                {
                    slot.HasOverdue = true;
                    continue;
                }

                if (isIdle && dueNow)
                {
                    RunJob(slot);
                    DateTime nextLocal = NextAlignedLocal(
                        nowLocal.AddMilliseconds(1),
                        today, slot.StartLocal, slot.EndLocal, slot.IntervalSeconds);
                    slot.NextRunUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocal, tz);
                }
            }
        }

        private void RunJob(JobSlot slot)
        {
            slot.Running = slot.Job(_cts.Token).ContinueWith(tr =>
            {
                if (tr.IsFaulted)
                {
                    var ex = tr.Exception != null ? tr.Exception.GetBaseException() : null;
                    try
                    {
                        if (!IsDisposed && !Disposing)
                            BeginInvoke((Action)(() =>
                                XtraMessageBox.Show(this, ex != null ? ex.Message : "Bilinmeyen hata", "İş Hatası")));
                    }
                    catch { }
                }

                try
                {
                    if (_cts != null && !_cts.IsCancellationRequested && slot.HasOverdue)
                    {
                        slot.HasOverdue = false;

                        if (slot.ForceMode)
                        {
                            // Zorla modda anında telafi ve interval ile devam
                            slot.NextRunUtc = DateTime.UtcNow;
                            BeginInvoke((Action)TickScheduler);
                        }
                        else
                        {
                            var tz = TimeZoneInfo.Local;
                            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                            var today = nowLocal.Date;

                            if (nowLocal < today + slot.EndLocal)
                            {
                                slot.NextRunUtc = DateTime.UtcNow;
                                BeginInvoke((Action)TickScheduler);
                            }
                            else
                            {
                                slot.NextRunUtc = TimeZoneInfo.ConvertTimeToUtc(today.AddDays(1) + slot.StartLocal, tz);
                            }
                        }
                    }
                }
                catch { }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        #endregion

        private void grupControlStart_DoubleClick(object sender, EventArgs e)
        {
            btnImkbBarDataSembol.Visible = !btnImkbBarDataSembol.Visible;
            btnImkbBarDataTekSembol.Visible = !btnImkbBarDataTekSembol.Visible;
            progressBarImkbBarDataSembol.Visible = !progressBarImkbBarDataSembol.Visible;
            progressBarImkbBarDataTekSembol.Visible = !progressBarImkbBarDataTekSembol.Visible;
            txtEndCalendar.Visible = !txtEndCalendar.Visible;
            txtSembol.Visible = !txtSembol.Visible;
            

        }
    }
}
