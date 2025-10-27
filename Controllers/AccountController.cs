using Microsoft.AspNetCore.Mvc;
using AtikProj.Services;
using AtikProj.Models;

namespace AtikProj.Controllers
{
    public class AccountController : Controller
    {
        private readonly IKullaniciService _kullaniciService;

        public AccountController(IKullaniciService kullaniciService)
        {
            _kullaniciService = kullaniciService;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // Zaten login olmuşsa yönlendir
            if (HttpContext.Session.GetString("KullaniciId") != null)
            {
                var rol = HttpContext.Session.GetString("Rol");
                return rol == "Admin" ? RedirectToAction("Index", "Dashboard") : RedirectToAction("Panel", "AtikNoktasi");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre)
        {
            var kullanici = await _kullaniciService.ValidateLoginAsync(kullaniciAdi, sifre);

            if (kullanici == null)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            // Session'a kaydet
            HttpContext.Session.SetString("KullaniciId", kullanici.Id);
            HttpContext.Session.SetString("KullaniciAdi", kullanici.KullaniciAdi);
            HttpContext.Session.SetString("Rol", kullanici.Rol);
            HttpContext.Session.SetString("AtikNoktasiId", kullanici.AtikNoktasiId ?? "");
            HttpContext.Session.SetString("FirmaAdi", kullanici.FirmaAdi ?? "");

            // Role göre yönlendir
            if (kullanici.Rol == "Admin")
                return RedirectToAction("Index", "Dashboard");
            else
                return RedirectToAction("Panel", "AtikNoktasi");
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Account/Setup
        [HttpGet]
        public IActionResult Setup()
        {
            return View();
        }

        // POST: Account/Setup
        [HttpPost]
        public async Task<IActionResult> Setup(string kullaniciAdi, string email, string sifre, string rol, string? firmaAdi)
        {
            try
            {
                var mevcutKullanici = await _kullaniciService.GetByKullaniciAdiAsync(kullaniciAdi);
                if (mevcutKullanici != null)
                {
                    ViewBag.Error = "Bu kullanıcı adı zaten kullanılıyor!";
                    return View();
                }

                var kullanici = new Kullanici
                {
                    KullaniciAdi = kullaniciAdi,
                    Email = email,
                    SifreHash = BCrypt.Net.BCrypt.HashPassword(sifre),
                    Rol = rol,
                    FirmaAdi = firmaAdi,
                    AtikNoktasiId = rol == "User" ? Guid.NewGuid().ToString() : null,
                    OlusturmaTarihi = DateTime.Now,
                    AktifMi = true
                };

                await _kullaniciService.CreateAsync(kullanici);

                ViewBag.Success = "Kullanıcı başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
                return View();
            }
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kullanıcı adı kontrolü
            var mevcutKullanici = await _kullaniciService.GetByKullaniciAdiAsync(model.KullaniciAdi);
            if (mevcutKullanici != null)
            {
                ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı zaten kullanılıyor");
                return View(model);
            }

            // Yeni kullanıcı oluştur - Sadece kullanıcı kaydı, AtikNoktasi ayrı yok
            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = model.KullaniciAdi,
                Email = string.IsNullOrEmpty(model.Telefon) ? model.KullaniciAdi + "@atik.com" : model.Telefon + "@atik.com",
                SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre),
                Rol = "User", // Fabrika kullanıcısı
                AtikNoktasiId = Guid.NewGuid().ToString(), // Unique ID oluştur
                FirmaAdi = model.FirmaAdi,
                OlusturmaTarihi = DateTime.Now,
                AktifMi = true
            };

            // Kullanıcıyı kaydet
            await _kullaniciService.CreateAsync(yeniKullanici);

            TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }
    }
}