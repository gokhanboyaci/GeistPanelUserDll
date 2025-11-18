using System;
using System.Data.SqlClient;
using System.IO;

namespace ideal.Config
{
    /// <summary>
    /// Uygulama genel yapılandırması.
    /// Bütün ayarlar bu sınıfın içinde tanımlıdır — harici JSON veya config dosyası kullanılmaz.
    /// </summary>
    public sealed class AppConfig
    {
        private static readonly Lazy<AppConfig> _instance = new Lazy<AppConfig>(() => new AppConfig());
        public static AppConfig Instance => _instance.Value;

        // === Klasörler ===
        public string MainPath { get; private set; }
        public string LogPath { get; private set; }
        public string LogDatabasePath { get; private set; }

        // === SQL Server bağlantı bilgileri ===
        public string SqlServerHost { get; private set; }
        public string SqlServerUser { get; private set; }
        public string SqlServerPassword { get; private set; }
        public bool TrustServerCertificate { get; private set; }
        public bool Encrypt { get; private set; }

        // === Veritabanı adları ===
        public string MainDatabaseName { get; private set; }
        public string SembollerDatabaseName { get; private set; }
        public string AlgobardataDatabaseName { get; private set; }

        // === İdeal sistem isimleri ===
        public string GDD_IdealName { get; private set; }
        public string HDD_IdealName { get; private set; }
        public string ADD_IdealName { get; private set; }
        public string KompozitSistem_IdealName { get; private set; }
        public string IMKBH_BarData_IdealName { get; private set; }
        public string IMKBX_BarData_IdealName { get; private set; }

        // === Telegram Bot Ayarları ===
        public string TelegramBotToken { get; private set; }
        public string TelegramChatId1 { get; private set; }
        public string TelegramChatId2 { get; private set; }

        private AppConfig()
        {
            LoadStaticDefaults();
        }

        /// <summary>
        /// Tüm yapılandırma değerlerini bu metotta sabit olarak tanımlarsın.
        /// </summary>
        private void LoadStaticDefaults()
        {
            // === Klasör yolları ===
            MainPath = @"C:\GeistPanelUserDll";
            LogPath = Path.Combine(MainPath, "Logs");
            LogDatabasePath = Path.Combine(LogPath, "Error.db");

            // === SQL Sunucu bilgileri ===
            SqlServerHost = "85.235.75.212";             // SQL Server adresi
            SqlServerUser = "geistUserDll";              // Kullanıcı adı
            SqlServerPassword = "k:Z71Hq>&pX,]q'D";      // Parola
            TrustServerCertificate = true;
            Encrypt = false;

            // === Veritabanı isimleri ===
            MainDatabaseName = "GEISTPANEL";               // Ana veritabanı
            SembollerDatabaseName = "IMKB_SEMBOLLER";      // Sembol ve endeks veritabanı
            AlgobardataDatabaseName = "GEISTPANELBARDATA";  // Algo bar veri veritabanı


            // === İdeal sistem isimleri ===
            GDD_IdealName = "AGP_GN";                                       // Günlük veri sistemi
            HDD_IdealName = "AHP_H";                                        // Haftalık veri sistemi
            ADD_IdealName = "AAP_A";                                        // Aylık veri sistemi
            KompozitSistem_IdealName = "A_GEIST_PANEL_V3_XU100";            // Kompozit veri sistemi
            IMKBH_BarData_IdealName = "A_GEIST_PANEL_V3_1DK_BAR_IMKBH";     // IMKBH bar datası
            IMKBX_BarData_IdealName = "A_GEIST_PANEL_V3_1DK_BAR_IMKBX";     // IMKBX bar datası


            // === Telegram Bot Ayarları ===
            TelegramBotToken = "7726922530:AAGJDBhl9k5tk4Gx4l1kyB4aM8ozhigV9Uw"; // Bot Token
            TelegramChatId1 = "1980323550"; // Birinci chat ID
            TelegramChatId2 = "479800646"; // İkinci chat ID
        }

        /// <summary>
        /// Verilen veritabanı ismiyle bağlantı dizesi üretir.
        /// </summary>
        public string GetSqlServerConnectionString(string databaseName)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = SqlServerHost,
                InitialCatalog = string.IsNullOrWhiteSpace(databaseName) ? "master" : databaseName,
                UserID = SqlServerUser,
                Password = SqlServerPassword,
                TrustServerCertificate = TrustServerCertificate,
                Encrypt = Encrypt
            };
            return builder.ToString();
        }

        
        public string MainDbConnString => GetSqlServerConnectionString(MainDatabaseName);
        public string SembollerDbConnString => GetSqlServerConnectionString(SembollerDatabaseName);
        public string AlgobardataDbConnString => GetSqlServerConnectionString(AlgobardataDatabaseName);


        /// <summary>
        /// Konsol veya log çıktısı için özet bağlantı bilgilerini döner.
        /// (Şifre hariç)
        /// </summary>
        public override string ToString()
        {
            return $"SQL Host: {SqlServerHost} | User: {SqlServerUser} | DB: {MainDatabaseName}";
        }
    }
}
