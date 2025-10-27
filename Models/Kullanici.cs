using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AtikProj.Models
{
    public class Kullanici
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string KullaniciAdi { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string SifreHash { get; set; } = string.Empty;
        
        public string Rol { get; set; } = string.Empty; // "Admin" veya "User"
        
        public string? AtikNoktasiId { get; set; } // User için gerekli
        
        public string? FirmaAdi { get; set; } // User için
        
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        
        public bool AktifMi { get; set; } = true;
    }
}