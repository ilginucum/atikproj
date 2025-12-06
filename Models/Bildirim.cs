using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtikProj.Models
{
    public class Bildirim
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        public string Mesaj { get; set; } = string.Empty;
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public bool Okundu { get; set; } = false;
        
        public decimal ToplamMiktar { get; set; }
        
        public string BildirimTipi { get; set; } = "10TonUyarisi"; // 10TonUyarisi, 5TonUyarisi, SevkiyatBildirimi, BilgiMesaji
        
        public string? IlgiliSevkiyatId { get; set; } // Sevkiyat bildirimleri için
        
        public string? HedefKullaniciId { get; set; } // Fabrika kullanıcılarına özel bildirim için
    }
}