using AtikProj.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AtikProj.Services
{
    public class SevkiyatService : ISevkiyatService
    {
        private readonly IMongoCollection<Sevkiyat> _sevkiyatlar;

        public SevkiyatService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _sevkiyatlar = mongoDatabase.GetCollection<Sevkiyat>("Sevkiyatlar");
        }

        public async Task<List<Sevkiyat>> GetAllAsync()
        {
            return await _sevkiyatlar.Find(_ => true)
                .SortByDescending(s => s.OlusturmaTarihi)
                .ToListAsync();
        }

        public async Task<Sevkiyat> GetByIdAsync(string id)
        {
            return await _sevkiyatlar.Find(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Sevkiyat sevkiyat)
        {
            await _sevkiyatlar.InsertOneAsync(sevkiyat);
        }

        public async Task UpdateAsync(string id, Sevkiyat sevkiyat)
        {
            await _sevkiyatlar.ReplaceOneAsync(s => s.Id == id, sevkiyat);
        }

        public async Task DeleteAsync(string id)
        {
            await _sevkiyatlar.DeleteOneAsync(s => s.Id == id);
        }

        public async Task<List<Sevkiyat>> GetAktifSevkiyatlarAsync()
        {
            return await _sevkiyatlar.Find(s => s.Durum == "Planlandı")
                .SortBy(s => s.SevkiyatTarihi)
                .ToListAsync();
        }
    }
}