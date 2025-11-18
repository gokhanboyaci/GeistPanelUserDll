using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ideal.Model
{

    [Table("SEMBOLLER")]
    public class Semboller
    {
        public int ID { get; set; }

        [Key, MaxLength(50)]
        public string SEMBOL { get; set; }

        [MaxLength(100)]
        public string SEKTOR { get; set; }

        public int? DURUM { get; set; }

        [MaxLength(10)]
        public string GRUP { get; set; }
    }

    [Table("IMKB_YUZEYSEL_VERI")]
    public class YuzeyselVeri
    {
        [Key, MaxLength(50)]
        public string SEMBOL { get; set; }

        public string TANIM { get; set; }
        public string SEKTOR { get; set; }
        public string PAZAR { get; set; }
        public int ENDEKS { get; set; }

        public decimal ONCEKI_KAPANIS { get; set; }
        public decimal FIYAT { get; set; }
        public decimal USD_FIYAT { get; set; }
        public decimal YUZDE { get; set; }
        public decimal TAVAN { get; set; }
        public decimal TABAN { get; set; }
        public decimal YUKSEK { get; set; }
        public decimal DUSUK { get; set; }
        public decimal HAFTALIK_YUKSEK { get; set; }
        public decimal HAFTALIK_DUSUK { get; set; }
        public decimal AYLIK_YUKSEK { get; set; }
        public decimal AYLIK_DUSUK { get; set; }
    }

    [Table("IMKB_DDP_DATA")]
    public class DestekDirencPivot
    {
        [Key, MaxLength(50)]
        public string SEMBOL { get; set; }

        // Günlük
        public decimal GUN_DIR1 { get; set; }
        public decimal GUN_DIR2 { get; set; }
        public decimal GUN_DIR3 { get; set; }
        public decimal GUN_PIVOT { get; set; }
        public decimal GUN_DEST1 { get; set; }
        public decimal GUN_DEST2 { get; set; }
        public decimal GUN_DEST3 { get; set; }

        // Haftalık
        public decimal HAFTA_DIR1 { get; set; }
        public decimal HAFTA_DIR2 { get; set; }
        public decimal HAFTA_DIR3 { get; set; }
        public decimal HAFTA_PIVOT { get; set; }
        public decimal HAFTA_DEST1 { get; set; }
        public decimal HAFTA_DEST2 { get; set; }
        public decimal HAFTA_DEST3 { get; set; }

        // Aylık
        public decimal AY_DIR1 { get; set; }
        public decimal AY_DIR2 { get; set; }
        public decimal AY_DIR3 { get; set; }
        public decimal AY_PIVOT { get; set; }
        public decimal AY_DEST1 { get; set; }
        public decimal AY_DEST2 { get; set; }
        public decimal AY_DEST3 { get; set; }

    }


    [Table("IMKB_KOMPOZIT_DATA")]
    public class KompozitData
    {
        [Key, MaxLength(50)]
        public string SEMBOL { get; set; }
        public decimal GUN_EMA5 { get; set; }
        public decimal GUN_EMA13 { get; set; }
        public decimal GUN_EMA21 { get; set; }
        public decimal GUN_EMA34 { get; set; }
        public decimal GUN_EMA55 { get; set; }
        public decimal GUN_EMA63 { get; set; }
        public decimal GUN_EMA89 { get; set; }
        public decimal GUN_EMA100 { get; set; }
        public decimal GUN_EMA126 { get; set; }
        public decimal GUN_EMA144 { get; set; }
        public decimal GUN_EMA200 { get; set; }
        public decimal GUN_EMA233 { get; set; }
        public decimal GUN_EMA252 { get; set; }
        public decimal GUN_EMA756 { get; set; }

        public decimal HAFTA_EMA5 { get; set; }
        public decimal HAFTA_EMA13 { get; set; }
        public decimal HAFTA_EMA21 { get; set; }
        public decimal HAFTA_EMA34 { get; set; }
        public decimal HAFTA_EMA55 { get; set; }
        public decimal HAFTA_EMA63 { get; set; }
        public decimal HAFTA_EMA89 { get; set; }
        public decimal HAFTA_EMA100 { get; set; }
        public decimal HAFTA_EMA126 { get; set; }
        public decimal HAFTA_EMA144 { get; set; }
        public decimal HAFTA_EMA200 { get; set; }
        public decimal HAFTA_EMA233 { get; set; }
        public decimal HAFTA_EMA252 { get; set; }
        public decimal HAFTA_EMA756 { get; set; }

        public decimal AY_EMA5 { get; set; }
        public decimal AY_EMA13 { get; set; }
        public decimal AY_EMA21 { get; set; }
        public decimal AY_EMA34 { get; set; }
        public decimal AY_EMA55 { get; set; }
        public decimal AY_EMA63 { get; set; }
        public decimal AY_EMA89 { get; set; }
        public decimal AY_EMA100 { get; set; }
        public decimal AY_EMA126 { get; set; }
        public decimal AY_EMA144 { get; set; }
        public decimal AY_EMA200 { get; set; }
        public decimal AY_EMA233 { get; set; }
        public decimal AY_EMA252 { get; set; }
        public decimal AY_EMA756 { get; set; }

        public decimal PD { get; set; }
        public decimal PW { get; set; }
        public decimal PM { get; set; }

        public decimal BOGA_AYI_GUN { get; set; }
        public decimal BOGA_AYI_HAFTA { get; set; }
        public decimal BOGA_AYI_AY { get; set; }

    }


    [Table("IMKB_ALGOBARDATA_ALL")]
    public class AlgoBarDataAll
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(50)]
        [Index("UX_IMKB_ALGOBARDATA_UNIQUE", 1, IsUnique = true)]
        public string SEMBOL { get; set; }

        [MaxLength(10)]
        [Index("UX_IMKB_ALGOBARDATA_UNIQUE", 2, IsUnique = true)]
        public string PERIYOT { get; set; }

        [Index("UX_IMKB_ALGOBARDATA_UNIQUE", 3, IsUnique = true)]
        public DateTime ZAMAN { get; set; }

        public decimal ACILIS { get; set; }
        public decimal YUKSEK { get; set; }
        public decimal DUSUK { get; set; }
        public decimal KAPANIS { get; set; }
        public decimal HACIM { get; set; }

        public decimal GEIST1 { get; set; }
        public decimal GEIST2 { get; set; }
        public decimal GEIST3 { get; set; }
        public decimal GEIST_TOPLAM { get; set; }

        public int MULTI_SISTEM_EK { get; set; }
        public decimal HACIM_YON { get; set; }
        public int HACIM_YON_EK { get; set; }
        public decimal MULTI_ROKET { get; set; }
        public int MULTI_ROKET_EK { get; set; }
        public decimal MULTI_EX { get; set; }
        public int MULTI_EX_EK { get; set; }
        public decimal GEIST_TR { get; set; }
        public int GEIST_TR_EK { get; set; }
        public decimal TERMINATOR { get; set; }
        public int TERMINATOR_EK { get; set; }
        public int HACIM_SIRA { get; set; }
        public int BOGA_AYI_TL_GUN { get; set; }
        public int BOGA_AYI_USD_GUN { get; set; }
        public int BOGA_AYI_TL_HAFTA { get; set; }
        public int BOGA_AYI_USD_HAFTA { get; set; }
        public int BOGA_AYI_TL_AY { get; set; }
        public int BOGA_AYI_USD_AY { get; set; }
        public int P1M { get; set; }
        public int P5M { get; set; }
        public int P15M { get; set; }
        public int P20M { get; set; }
        public int P30M { get; set; }
        public int P60M { get; set; }
        public int P120M { get; set; }
        public int P240M { get; set; }
        public int PD { get; set; }
        public int PW { get; set; }
        public int PM { get; set; }
        public decimal EMA5 { get; set; }
        public decimal EMA21 { get; set; }
        public decimal EMA63 { get; set; }
        public decimal EMA126 { get; set; }
        public decimal EMA252 { get; set; }
        public decimal EMA756 { get; set; }
        public decimal EMA100 { get; set; }
        public decimal EMA200 { get; set; }
        public decimal RSI { get; set; }
        public decimal SRSI { get; set; }
    }


    [Table("IMKB_GUNLUK_ALGOBARDATA")]
    public class GunlukAlgoBarData
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(50)]
        [Index("UX_IMKB_ALGOBARDATA_UNIQUE", 1, IsUnique = true)]
        public string SEMBOL { get; set; }

        [MaxLength(10)]
        [Index("UX_IMKB_ALGOBARDATA_UNIQUE", 2, IsUnique = true)]
        public string PERIYOT { get; set; }

        [Index("UX_IMKB_ALGOBARDATA_UNIQUE", 3, IsUnique = true)]
        public DateTime ZAMAN { get; set; }

        public decimal ACILIS { get; set; }
        public decimal YUKSEK { get; set; }
        public decimal DUSUK { get; set; }
        public decimal KAPANIS { get; set; }
        public decimal HACIM { get; set; }

        public decimal GEIST1 { get; set; }
        public decimal GEIST2 { get; set; }
        public decimal GEIST3 { get; set; }
        public decimal GEIST_TOPLAM { get; set; }

        public int MULTI_SISTEM_EK { get; set; }
        public decimal HACIM_YON { get; set; }
        public int HACIM_YON_EK { get; set; }
        public decimal MULTI_ROKET { get; set; }
        public int MULTI_ROKET_EK { get; set; }
        public decimal MULTI_EX { get; set; }
        public int MULTI_EX_EK { get; set; }
        public decimal GEIST_TR { get; set; }
        public int GEIST_TR_EK { get; set; }
        public decimal TERMINATOR { get; set; }
        public int TERMINATOR_EK { get; set; }
        public int HACIM_SIRA { get; set; }
        public int BOGA_AYI_TL_GUN { get; set; }
        public int BOGA_AYI_USD_GUN { get; set; }
        public int BOGA_AYI_TL_HAFTA { get; set; }
        public int BOGA_AYI_USD_HAFTA { get; set; }
        public int BOGA_AYI_TL_AY { get; set; }
        public int BOGA_AYI_USD_AY { get; set; }
        public int P1M { get; set; }
        public int P5M { get; set; }
        public int P15M { get; set; }
        public int P20M { get; set; }
        public int P30M { get; set; }
        public int P60M { get; set; }
        public int P120M { get; set; }
        public int P240M { get; set; }
        public int PD { get; set; }
        public int PW { get; set; }
        public int PM { get; set; }
        public decimal EMA5 { get; set; }
        public decimal EMA21 { get; set; }
        public decimal EMA63 { get; set; }
        public decimal EMA126 { get; set; }
        public decimal EMA252 { get; set; }
        public decimal EMA756 { get; set; }
        public decimal EMA100 { get; set; }
        public decimal EMA200 { get; set; }
        public decimal RSI { get; set; }
        public decimal SRSI { get; set; }
    }

    [Table("IMKB_TARAMA_UYUMSUZLUK")]
    public class TaramaUyumsuzluk
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string SEMBOL { get; set; }

        [Required]
        [MaxLength(10)]
        public string PERIYOT { get; set; }
        public DateTime ILK_TARIH { get; set; }
        public DateTime SON_TARIH { get; set; }        
        public bool UYUMSUZLUK_TIPI { get; set; }
        public decimal ILK_FIYAT { get; set; }
        public decimal SON_FIYAT { get; set; }
        public decimal FIYAT_DEGISIM { get; set; }
        public decimal ILK_RSI { get; set; }
        public decimal SON_RSI { get; set; }
        public decimal RSI_DEGISIM { get; set; }
        public decimal GUC { get; set; }        
        public DateTime TARAMA_ZAMANI { get; set; }
    }

  
    [Table("IMKB_TARAMA_RSI_SINYAL")]
    public class TaramaSRSI
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(50)]
        public string SEMBOL { get; set; }

        [Required]
        [MaxLength(10)]
        public string PERIYOT { get; set; }
        public DateTime SINYAL_TARIHI { get; set; }       
        public byte SINYAL_TURU { get; set; }
        public int GUN_SAYISI { get; set; }
        public decimal RSI_DEGERI { get; set; }
        public decimal SRSI_DEGERI { get; set; }
        public decimal SRSI_EMA_DEGERI { get; set; }
        public decimal GIRIS_FIYATI { get; set; }
        public decimal GUNCEL_FIYAT { get; set; }
        public decimal DEGISIM_YUZDESI { get; set; }       
        public DateTime TARAMA_ZAMANI { get; set; }
    }


}
