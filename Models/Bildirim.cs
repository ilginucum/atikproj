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
        
        // Admin bildirimleri için kullanılacak
        public decimal ToplamMiktar { get; set; }
        
        public string BildirimTipi { get; set; } = "10TonUyarisi"; // 10TonUyarisi, SevkiyatBildirimi vs.
    }
}