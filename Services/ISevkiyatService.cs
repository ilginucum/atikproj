using AtikProj.Models;

namespace AtikProj.Services
{
    public interface ISevkiyatService
    {
        Task<List<Sevkiyat>> GetAllAsync();
        Task<Sevkiyat> GetByIdAsync(string id);
        Task CreateAsync(Sevkiyat sevkiyat);
        Task UpdateAsync(string id, Sevkiyat sevkiyat);
        Task DeleteAsync(string id);
        Task<List<Sevkiyat>> GetAktifSevkiyatlarAsync();
    }
}