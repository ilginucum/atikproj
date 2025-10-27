using AtikProj.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using BCrypt.Net;

namespace AtikProj.Services
{
    public class KullaniciService : IKullaniciService
    {
        private readonly IMongoCollection<Kullanici> _kullanicilar;

        public KullaniciService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _kullanicilar = mongoDatabase.GetCollection<Kullanici>("Kullanicilar");
        }

        public async Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi)
        {
            return await _kullanicilar.Find(x => x.KullaniciAdi == kullaniciAdi).FirstOrDefaultAsync();
        }

        public async Task<Kullanici?> GetByEmailAsync(string email)
        {
            return await _kullanicilar.Find(x => x.Email == email).FirstOrDefaultAsync();
        }

        public async Task<Kullanici?> ValidateLoginAsync(string kullaniciAdi, string sifre)
        {
            var kullanici = await GetByKullaniciAdiAsync(kullaniciAdi);
            
            if (kullanici == null || !kullanici.AktifMi)
                return null;

            bool sifreDogrumu = BCrypt.Net.BCrypt.Verify(sifre, kullanici.SifreHash);
            return sifreDogrumu ? kullanici : null;
        }

        public async Task CreateAsync(Kullanici kullanici)
        {
            await _kullanicilar.InsertOneAsync(kullanici);
        }
        public async Task<List<Kullanici>> GetAllAsync()
        {
            return await _kullanicilar.Find(_ => true).ToListAsync();
        }
    }
}