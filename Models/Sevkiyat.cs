using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtikProj.Models
{
    public class Sevkiyat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        
        public DateTime SevkiyatTarihi { get; set; }
        
        public string Durum { get; set; } = "Planlandı"; // Planlandı, Tamamlandı, İptal
        
        public decimal ToplamMiktar { get; set; }
        
        public List<string> AtikKayitIds { get; set; } = new List<string>(); // Seçilen atık kayıtlarının ID'leri
        
        public List<string> AtikNoktasiIds { get; set; } = new List<string>(); // Hangi firmaların atıkları alınacak
        
        public DateTime OlusturmaTarihi { get; set; }
        
        public string OlusturanKullaniciId { get; set; } = string.Empty;
        
        public string? Notlar { get; set; }
    }
}