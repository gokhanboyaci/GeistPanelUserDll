using ideal.Config;
using System;
using System.Data.SqlClient;
using System.IO;

namespace ideal
{
    /// <summary>
    /// Uygulama başlangıç adımları (klasörler, log veritabanı, uzak DB varlık kontrolleri).
    /// </summary>
    public static class AppInitializer
    {
        /// <summary>
        /// Başlatma dizisini çalıştırır.
        /// </summary>
        public static void Initialize()
        {
            CreateFolders();
            InitializeLogStore();            
        }

        private static void CreateFolders()
        {
            var cfg = AppConfig.Instance;
            try
            {
                Directory.CreateDirectory(cfg.MainPath);
                Directory.CreateDirectory(cfg.LogPath);
            }
            catch { }
        }

        private static void InitializeLogStore()
        {
            var dbPath = AppConfig.Instance.LogDatabasePath;
            var dir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (!File.Exists(dbPath))
            {
                System.Data.SQLite.SQLiteConnection.CreateFile(dbPath);
            }

            var connStr = "Data Source=" + dbPath + ";Version=3;";
            using (var con = new System.Data.SQLite.SQLiteConnection(connStr))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ERROR_LOG (
                        ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        TIME_STAMP_UTC TEXT,
                        MESSAGE TEXT,
                        STACKTRACE TEXT
                    );";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        
    }
}
