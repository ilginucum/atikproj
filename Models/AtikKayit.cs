using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtikProj.Models
{
    public class AtikKayit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string AtikNoktasiId { get; set; } = string.Empty;
        
        public string AtikKodu { get; set; } = string.Empty;
        
        public string AtikAdi { get; set; } = string.Empty;
        
        public decimal Miktar { get; set; }
        
        public string Birim { get; set; } = string.Empty;
        
        public DateTime GirisTarihi { get; set; } = DateTime.Now;
        
        public string GirenKullaniciId { get; set; } = string.Empty;
        
        public bool SevkEdildiMi { get; set; } = false;
        
        public string? SevkiyatId { get; set; }

        // Ton cinsine çevirme
        [BsonIgnore]
        public decimal MiktarTon
        {
            get
            {
                if (Birim == "Kg")
                    return Miktar / 1000;
                return Miktar;
            }
        }
    }
}