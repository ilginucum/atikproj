using AtikProj.Models;

namespace AtikProj.Models
{
    public class GecmisSevkiyatlarViewModel
    {
        public List<SevkiyatDetayDto> Sevkiyatlar { get; set; } = new List<SevkiyatDetayDto>();
        
        // Filtreleme için
        public string? AtikKoduFiltre { get; set; }
        public string? AtikAdiFiltre { get; set; }
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
        public string? DurumFiltre { get; set; }
        public string? HalTipiFiltre { get; set; }
        public string? FirmaAdiFiltre { get; set; }
        
        // Özet bilgiler
        public int ToplamSevkiyatSayisi { get; set; }
        public decimal ToplamAtikMiktari { get; set; }
        public int ToplamAtikKayitSayisi { get; set; }
    }

    public class SevkiyatDetayDto
    {
        public string SevkiyatId { get; set; } = string.Empty;
        public DateTime SevkiyatTarihi { get; set; }
        public string Durum { get; set; } = string.Empty;
        public decimal ToplamMiktar { get; set; }
        public string? Notlar { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public int FirmaSayisi { get; set; }
        public int AtikKayitSayisi { get; set; }
        
        // Atık kayıtları detayları
        public List<AtikKayitDetayDto> AtikKayitlari { get; set; } = new List<AtikKayitDetayDto>();
    }

    public class AtikKayitDetayDto
    {
        public string Id { get; set; } = string.Empty;
        public string AtikKodu { get; set; } = string.Empty;
        public string AtikAdi { get; set; } = string.Empty;
        public decimal Miktar { get; set; }
        public string Birim { get; set; } = string.Empty;
        public string HalTipi { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string? GorselUrl { get; set; }
        public DateTime GirisTarihi { get; set; }
        public string FirmaAdi { get; set; } = string.Empty;
        public bool SevkEdildiMi { get; set; }
    }
}