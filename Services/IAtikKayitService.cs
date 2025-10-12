using AtikProj.Models;

namespace AtikProj.Services
{
    public interface IAtikKayitService
    {
        Task<List<AtikKayit>> GetAllAsync();
        Task<AtikKayit> GetByIdAsync(string id);
        Task<List<AtikKayit>> GetByNoktaIdAsync(string noktaId);
        Task<List<AtikKayit>> GetAktifAtiklar(); // Sevk edilmemiş
        Task CreateAsync(AtikKayit atikKayit);
        Task UpdateAsync(string id, AtikKayit atikKayit);
        Task DeleteAsync(string id);
        Task<decimal> GetToplamAktifMiktarAsync(); // 10 ton kontrolü için
    }
}