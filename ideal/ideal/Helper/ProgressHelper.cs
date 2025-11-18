using System;
using System.Diagnostics;
using DevExpress.XtraEditors;

namespace ideal.Helper
{
    /// <summary>
    /// DevExpress ProgressBarControl adaptörü.
    /// </summary>
    public class ProgressHelper
    {
        public sealed class ProgressInfo
        {
            public int Current;
            public int Total;
            public string Item;
            public TimeSpan Elapsed;
        }

        public sealed class DevExpressProgressAdapter : IProgress<ProgressInfo>, IDisposable
        {
            private readonly ProgressBarControl _bar;
            private readonly Stopwatch _sw = new Stopwatch();
            private string _display = string.Empty;
            private bool _disposed;

            /// <summary>
            /// Adaptörü başlatır ve progress bar sınırlarını ayarlar.
            /// </summary>
            public DevExpressProgressAdapter(ProgressBarControl bar, int total)
            {
                _bar = bar ?? throw new ArgumentNullException(nameof(bar));
                _bar.Properties.Minimum = 0;
                _bar.Properties.Maximum = Math.Max(1, total);
                _bar.EditValue = 0;
                _bar.Properties.ShowTitle = true;
                _bar.CustomDisplayText += OnCustomDisplayText;
                _sw.Start();
                UpdateTitle(0, total, "");
            }

            public void Report(ProgressInfo info)
            {
                if (_disposed) return;
                if (_bar.IsHandleCreated && _bar.InvokeRequired) _bar.Invoke((Action)(() => Apply(info)));
                else Apply(info);
            }

            private void Apply(ProgressInfo info)
            {
                var current = Math.Min(Math.Max(info.Current, 0), Math.Max(1, info.Total));
                _bar.Properties.Maximum = Math.Max(1, info.Total);
                _bar.EditValue = current;
                UpdateTitle(current, info.Total, info.Item ?? "");
            }

            private void UpdateTitle(int current, int total, string item)
            {
                int percent = total > 0 ? (int)Math.Round(current * 100.0 / total) : 0;
                _display = $"%{percent} | {item} | Geçen Süre : {_sw.Elapsed:hh\\:mm\\:ss}";
                _bar.Invalidate();
            }

            private void OnCustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
            {
                e.DisplayText = _display;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _sw.Stop();
                _bar.CustomDisplayText -= OnCustomDisplayText;
            }
        }
    }
}
