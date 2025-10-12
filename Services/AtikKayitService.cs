using AtikProj.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AtikProj.Services
{
    public class AtikKayitService : IAtikKayitService
    {
        private readonly IMongoCollection<AtikKayit> _atikKayitlar;

        public AtikKayitService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _atikKayitlar = mongoDatabase.GetCollection<AtikKayit>("AtikKayitlari");
        }

        public async Task<List<AtikKayit>> GetAllAsync()
        {
            return await _atikKayitlar.Find(_ => true).ToListAsync();
        }

        public async Task<AtikKayit> GetByIdAsync(string id)
        {
            return await _atikKayitlar.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<AtikKayit>> GetByNoktaIdAsync(string noktaId)
        {
            return await _atikKayitlar.Find(x => x.AtikNoktasiId == noktaId).ToListAsync();
        }

        public async Task<List<AtikKayit>> GetAktifAtiklar()
        {
            return await _atikKayitlar.Find(x => x.SevkEdildiMi == false).ToListAsync();
        }

        public async Task CreateAsync(AtikKayit atikKayit)
        {
            await _atikKayitlar.InsertOneAsync(atikKayit);
        }

        public async Task UpdateAsync(string id, AtikKayit atikKayit)
        {
            await _atikKayitlar.ReplaceOneAsync(x => x.Id == id, atikKayit);
        }

        public async Task DeleteAsync(string id)
        {
            await _atikKayitlar.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<decimal> GetToplamAktifMiktarAsync()
        {
            var aktifAtiklar = await GetAktifAtiklar();
            return aktifAtiklar.Sum(a => a.MiktarTon);
        }
    }
}