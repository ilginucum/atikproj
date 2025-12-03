using AtikProj.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AtikProj.Services
{
    public class BildirimService : IBildirimService
    {
        private readonly IMongoCollection<Bildirim> _bildirimler;

        public BildirimService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _bildirimler = mongoDatabase.GetCollection<Bildirim>("Bildirimler");
        }

        public async Task<List<Bildirim>> GetAllAsync()
        {
            return await _bildirimler.Find(_ => true)
                .SortByDescending(b => b.OlusturmaTarihi)
                .ToListAsync();
        }

        public async Task<List<Bildirim>> GetOkunmamisBildirimlerAsync()
        {
            return await _bildirimler.Find(b => b.Okundu == false)
                .SortByDescending(b => b.OlusturmaTarihi)
                .ToListAsync();
        }

        public async Task<Bildirim> GetByIdAsync(string id)
        {
            return await _bildirimler.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Bildirim bildirim)
        {
            await _bildirimler.InsertOneAsync(bildirim);
        }

        public async Task BildirimOkunduIsaretle(string id)
        {
            var filter = Builders<Bildirim>.Filter.Eq(b => b.Id, id);
            var update = Builders<Bildirim>.Update.Set(b => b.Okundu, true);
            await _bildirimler.UpdateOneAsync(filter, update);
        }

        public async Task<int> GetOkunmamisSayisi()
        {
            return (int)await _bildirimler.CountDocumentsAsync(b => b.Okundu == false);
        }

        public async Task DeleteAsync(string id)
        {
            await _bildirimler.DeleteOneAsync(b => b.Id == id);
        }
    }
}