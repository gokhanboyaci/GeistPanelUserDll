using ideal.Config;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace ideal.Logging
{
    /// <summary>
    /// Hata loglarını yerel SQLite veritabanına yazar.
    /// </summary>
    public static class ErrorLogger
    {
        /// <summary>
        /// Verilen hatayı ERROR_LOG tablosuna kaydeder ve eklenen kaydın kimliğini döner.
        /// </summary>
        public static Task<long> LogAsync(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));

            return Task.Run(() =>
            {
                var dbPath = AppConfig.Instance.LogDatabasePath;
                var connStr = "Data Source=" + dbPath + ";Version=3;Journal Mode=WAL;Synchronous=NORMAL;";

                using (var con = new SQLiteConnection(connStr))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = @"
                        INSERT INTO ERROR_LOG (TIME_STAMP_UTC, MESSAGE, STACKTRACE)
                        VALUES (@ts, @msg, @st);
                        SELECT last_insert_rowid();";

                        cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
                        cmd.Parameters.AddWithValue("@msg", ex.Message);
                        cmd.Parameters.AddWithValue("@st", ex.ToString());

                        var result = cmd.ExecuteScalar();
                        return result == null ? 0L : (long)result;
                    }
                }
            });
        }
    }
}
