using System.ComponentModel.DataAnnotations;

namespace AtikProj.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı gerekli")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string KullaniciAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gerekli")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalı")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı gerekli")]
        [DataType(DataType.Password)]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor")]
        public string SifreTekrar { get; set; } = string.Empty;

        [Required(ErrorMessage = "Firma adı gerekli")]
        [StringLength(100, ErrorMessage = "Firma adı en fazla 100 karakter olabilir")]
        public string FirmaAdi { get; set; } = string.Empty;

        public string? Telefon { get; set; }
    }
}