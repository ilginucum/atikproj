using AtikProj.Models;

namespace AtikProj.Services
{
    public interface IKullaniciService
    {
        Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi);
        Task<Kullanici?> GetByEmailAsync(string email);
        Task<Kullanici?> ValidateLoginAsync(string kullaniciAdi, string sifre);
        Task CreateAsync(Kullanici kullanici);
        Task<List<Kullanici>> GetAllAsync();
    }
}