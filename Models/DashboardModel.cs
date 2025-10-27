using System;
using System.Collections.Generic;
using System.Linq;

namespace AtikProj.Models
{
    public class DashboardViewModel
    {
        public decimal ToplamAktifAtik { get; set; }
        public decimal HedefMiktar { get; set; }
        public int AtikNoktaSayisi { get; set; }
        public List<AtikNoktaDetay> AtikNoktaları { get; set; } = new List<AtikNoktaDetay>();
        public List<GecmisSevkiyat> GecmisSevkiyatlar { get; set; } = new List<GecmisSevkiyat>();
        public List<AtikDagılım> AtikDagilim { get; set; } = new List<AtikDagılım>();

        public decimal DolulukYuzdesi => HedefMiktar > 0 ? (ToplamAktifAtik / HedefMiktar) * 100 : 0;
        public decimal KalanMiktar => HedefMiktar - ToplamAktifAtik;
    }

    public class AtikNoktaDetay
    {
        public string NoktaAdi { get; set; } = string.Empty;
        public decimal ToplamMiktar { get; set; }
        public int AtikSayisi { get; set; }
        public List<AtikDetay> Atiklar { get; set; } = new List<AtikDetay>();
        public List<AtikKayit> AtikKayitlari { get; set; } = new List<AtikKayit>();
    }

    public class AtikDetay
    {
        public string AtikKodu { get; set; } = string.Empty;
        public string AtikAdi { get; set; } = string.Empty;
        public decimal Miktar { get; set; }
    }

    public class GecmisSevkiyat
    {
        public DateTime Tarih { get; set; }
        public decimal ToplamMiktar { get; set; }
        public int NoktaSayisi { get; set; }
        public string Detaylar { get; set; } = string.Empty;
    }

    public class AtikDagılım
    {
        public string AtikAdi { get; set; } = string.Empty;
        public decimal Miktar { get; set; }
    }

    // Harita için yeni model
    public class MapViewModel
    {
        public List<AtikNoktaHarita> AtikNoktaları { get; set; } = new List<AtikNoktaHarita>();
    }

    public class AtikNoktaHarita
    {
        public string NoktaAdi { get; set; } = string.Empty;
        public double Enlem { get; set; }
        public double Boylam { get; set; }
        public decimal ToplamMiktar { get; set; }
        public List<AtikDetay> Atiklar { get; set; } = new List<AtikDetay>();

        // Renk belirleme (Yeşil < 3 ton, Sarı 3-7 ton, Kırmızı >= 7 ton)
        public string MarkerRenk
        {
            get
            {
                if (ToplamMiktar >= 7) return "red";
                if (ToplamMiktar >= 3) return "yellow";
                return "green";
            }
        }
    }
}