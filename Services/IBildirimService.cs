using AtikProj.Models;

namespace AtikProj.Services
{
    public interface IBildirimService
    {
        Task<List<Bildirim>> GetAllAsync();
        Task<List<Bildirim>> GetOkunmamisBildirimlerAsync();
        Task<Bildirim> GetByIdAsync(string id);
        Task CreateAsync(Bildirim bildirim);
        Task BildirimOkunduIsaretle(string id);
        Task<int> GetOkunmamisSayisi();
        Task DeleteAsync(string id);
    }
}