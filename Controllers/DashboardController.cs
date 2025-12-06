using Microsoft.AspNetCore.Mvc;
using AtikProj.Models;
using AtikProj.Filters;
using AtikProj.Services;

namespace AtikProj.Controllers
{
    [AdminAuthorize]
    public class DashboardController : Controller
    {
         private readonly IAtikKayitService _atikKayitService;
         private readonly IKullaniciService _kullaniciService;
         private readonly IBildirimService _bildirimService;
         private readonly ISevkiyatService _sevkiyatService;


        public DashboardController(IAtikKayitService atikKayitService, IKullaniciService kullaniciService, IBildirimService bildirimService, ISevkiyatService sevkiyatService) 
        {
            _atikKayitService = atikKayitService;
            _kullaniciService = kullaniciService;
            _bildirimService = bildirimService;
            _sevkiyatService = sevkiyatService;
        }
        public async Task<IActionResult> Index()
        {
            // MongoDB'den tüm aktif atık kayıtlarını getir
            var tumAtiklar = await _atikKayitService.GetAktifAtiklar();
            
            // Toplam aktif atık miktarını hesapla
            var toplamAktifAtik = tumAtiklar.Sum(a => a.MiktarTon);
            
            // Atık noktalarına göre grupla
            var atikNoktaGruplari = new List<AtikNoktaDetay>();

            foreach (var grup in tumAtiklar.GroupBy(a => a.AtikNoktasiId))
            {
                var firmaAdi = await GetFirmaAdiAsync(grup.Key);
                
                atikNoktaGruplari.Add(new AtikNoktaDetay
                {
                    NoktaAdi = firmaAdi,
                    ToplamMiktar = grup.Sum(a => a.MiktarTon),
                    AtikSayisi = grup.Count(),
                    AtikKayitlari = grup.ToList() // Her atık kaydını ayrı ayrı sakla
                });
            }

            atikNoktaGruplari = atikNoktaGruplari.OrderByDescending(n => n.ToplamMiktar).ToList();
            
            // Model oluştur
            var model = new DashboardViewModel
            {
                ToplamAktifAtik = toplamAktifAtik,
                HedefMiktar = 10m,
                AtikNoktaSayisi = atikNoktaGruplari.Count,
                AtikNoktaları = atikNoktaGruplari,
                GecmisSevkiyatlar = new List<GecmisSevkiyat>()
            };
            
            // Atık türlerine göre dağılım hesapla
            model.AtikDagilim = tumAtiklar
                .GroupBy(a => a.AtikAdi)
                .Select(g => new AtikDagılım
                {
                    AtikAdi = g.Key,
                    Miktar = g.Sum(a => a.MiktarTon)
                })
                .OrderByDescending(a => a.Miktar)
                .ToList();
            
            return View(model);
        }
        private async Task<string> GetFirmaAdiAsync(string atikNoktasiId)
        {
            // Kullanıcı tablosundan firma adını bul
            var kullanicilar = await _kullaniciService.GetAllAsync();
            var kullanici = kullanicilar.FirstOrDefault(k => k.AtikNoktasiId == atikNoktasiId);
            
            return kullanici?.FirmaAdi ?? $"Firma {atikNoktasiId.Substring(0, Math.Min(8, atikNoktasiId.Length))}";
        }

        // Yeni action ekle:
        public async Task<IActionResult> Bildirimler()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            var rol = HttpContext.Session.GetString("Rol");
            
            var tumBildirimler = await _bildirimService.GetAllAsync();
            
            List<Bildirim> bildirimler;
            
            if (rol == "Admin")
            {
                // Admin sadece HedefKullaniciId null olan veya kendi ID'sine özel bildirimleri görsün
                bildirimler = tumBildirimler
                    .Where(b => string.IsNullOrEmpty(b.HedefKullaniciId) || b.HedefKullaniciId == kullaniciId)
                    .OrderByDescending(b => b.OlusturmaTarihi)
                    .ToList();
            }
            else
            {
                // User sadece kendi bildirimlerini görsün
                bildirimler = tumBildirimler
                    .Where(b => b.HedefKullaniciId == kullaniciId)
                    .OrderByDescending(b => b.OlusturmaTarihi)
                    .ToList();
            }
            
            return View(bildirimler);
        }

        // API endpoint - okunmamış sayısı için
        [HttpGet]
        public async Task<JsonResult> GetOkunmamisBildirimSayisi()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            var rol = HttpContext.Session.GetString("Rol");
            
            var tumBildirimler = await _bildirimService.GetOkunmamisBildirimlerAsync();
            
            int sayi;
            
            if (rol == "Admin")
            {
                // Admin sadece HedefKullaniciId null olan veya kendi ID'sine özel bildirimleri sayar
                sayi = tumBildirimler
                    .Count(b => string.IsNullOrEmpty(b.HedefKullaniciId) || b.HedefKullaniciId == kullaniciId);
            }
            else
            {
                // User sadece kendi bildirimlerini sayar
                sayi = tumBildirimler
                    .Count(b => b.HedefKullaniciId == kullaniciId);
            }
            
            return Json(new { sayi });
}

        // Bildirimi okundu işaretle
        [HttpPost]
        public async Task<JsonResult> BildirimOkundu(string id)
        {
            await _bildirimService.BildirimOkunduIsaretle(id);
            return Json(new { success = true });
        }

        // Bildirimi tamamen sil (opsiyonel - admin isterse silebilir)
        [HttpPost]
        public async Task<JsonResult> BildirimSil(string id)
        {
            try
            {
                await _bildirimService.DeleteAsync(id);
                return Json(new { success = true, message = "Bildirim silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Tüm okunmuş bildirimleri temizle
        [HttpPost]
        public async Task<JsonResult> OkunmuslariTemizle()
        {
            try
            {
                var tumBildirimler = await _bildirimService.GetAllAsync();
                var okunmuslar = tumBildirimler.Where(b => b.Okundu).ToList();
                
                foreach (var bildirim in okunmuslar)
                {
                    await _bildirimService.DeleteAsync(bildirim.Id);
                }
                
                return Json(new { success = true, message = $"{okunmuslar.Count} okunmuş bildirim temizlendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Sevkiyat planlama sayfası
        public async Task<IActionResult> SevkiyatPlanla()
        {
            var tumAktifAtiklar = await _atikKayitService.GetAktifAtiklar();
            
            // Atık noktalarına göre grupla
            var atikNoktaGruplari = new List<AtikNoktaDetay>();

            foreach (var grup in tumAktifAtiklar.GroupBy(a => a.AtikNoktasiId))
            {
                var firmaAdi = await GetFirmaAdiAsync(grup.Key);
                
                atikNoktaGruplari.Add(new AtikNoktaDetay
                {
                    NoktaAdi = firmaAdi,
                    ToplamMiktar = grup.Sum(a => a.MiktarTon),
                    AtikSayisi = grup.Count(),
                    AtikKayitlari = grup.ToList()
                });
            }

            atikNoktaGruplari = atikNoktaGruplari.OrderByDescending(n => n.ToplamMiktar).ToList();
            
            var model = new SevkiyatPlanlaViewModel
            {
                AtikNoktaGruplari = atikNoktaGruplari,
                ToplamAktifAtik = tumAktifAtiklar.Sum(a => a.MiktarTon)
            };
            
            return View(model);
        }
        // Sevkiyat oluştur

        [HttpPost]
        public async Task<IActionResult> SevkiyatOlustur(DateTime sevkiyatTarihi, string seciliAtikIds, string notlar)
        {
            try
            {
                var atikIdler = seciliAtikIds.Split(',').ToList();
                
                if (!atikIdler.Any())
                {
                    return Json(new { success = false, message = "Lütfen en az bir atık seçin!" });
                }

                // Seçilen atıkları getir
                var seciliAtiklar = new List<AtikKayit>();
                var atikNoktasiIds = new HashSet<string>();
                
                foreach (var atikId in atikIdler)
                {
                    var atik = await _atikKayitService.GetByIdAsync(atikId);
                    if (atik != null)
                    {
                        seciliAtiklar.Add(atik);
                        atikNoktasiIds.Add(atik.AtikNoktasiId);
                    }
                }

                var toplamMiktar = seciliAtiklar.Sum(a => a.MiktarTon);
                var kullaniciId = HttpContext.Session.GetString("KullaniciId");

                // Sevkiyat oluştur
                var sevkiyat = new Sevkiyat
                {
                    SevkiyatTarihi = sevkiyatTarihi,
                    Durum = "Planlandı",
                    ToplamMiktar = toplamMiktar,
                    AtikKayitIds = atikIdler,
                    AtikNoktasiIds = atikNoktasiIds.ToList(),
                    OlusturmaTarihi = DateTime.Now,
                    OlusturanKullaniciId = kullaniciId ?? "",
                    Notlar = notlar
                };

                await _sevkiyatService.CreateAsync(sevkiyat);

                // Seçilen atıkları "sevk edildi" olarak işaretle
                foreach (var atikId in atikIdler)
                {
                    var atik = await _atikKayitService.GetByIdAsync(atikId);
                    if (atik != null)
                    {
                        atik.SevkEdildiMi = true;
                        await _atikKayitService.UpdateAsync(atikId, atik);
                    }
                }

                // ⭐ SADECE SEVKİYATA DAHİL OLAN FİRMALARA BİLDİRİM GÖNDER
                var tumKullanicilar = await _kullaniciService.GetAllAsync();
                
                // Sevkiyata dahil olan atık noktalarının kullanıcılarını bul
                var sevkiyatKullanicilari = tumKullanicilar
                    .Where(k => atikNoktasiIds.Contains(k.AtikNoktasiId))
                    .ToList();
                
                foreach (var kullanici in sevkiyatKullanicilari)
                {
                    // Bu kullanıcının sevkiyattaki atıklarını hesapla
                    var kullanicininAtiklari = seciliAtiklar
                        .Where(a => a.AtikNoktasiId == kullanici.AtikNoktasiId)
                        .ToList();
                    
                    var kullaniciAtikMiktari = kullanicininAtiklari.Sum(a => a.MiktarTon);
                    
                    // Firma için özel mesaj oluştur
                    var mesaj = $"🚚 Sevkiyat Planlandı! {sevkiyatTarihi.ToString("dd.MM.yyyy")} tarihinde {kullaniciAtikMiktari:F2} ton atığınız toplanacaktır. Lütfen hazır bulunun.";
                    
                    // Not varsa ekle
                    if (!string.IsNullOrEmpty(notlar))
                    {
                        mesaj += $"\n\n📝 Not: {notlar}";
                    }
                    
                    var fabrikaBildirimi = new Bildirim
                    {
                        Mesaj = mesaj,
                        OlusturmaTarihi = DateTime.Now,
                        Okundu = false,
                        ToplamMiktar = kullaniciAtikMiktari, // Kullanıcının kendi atık miktarı
                        BildirimTipi = "SevkiyatBildirimi",
                        IlgiliSevkiyatId = sevkiyat.Id,
                        HedefKullaniciId = kullanici.Id // ⭐ Sadece bu kullanıcıya özel
                    };

                    await _bildirimService.CreateAsync(fabrikaBildirimi);
                }

                // Admin'e de onay bildirimi
                var adminMesaj = $"✅ Sevkiyat başarıyla planlandı! {sevkiyatKullanicilari.Count} firma, {seciliAtiklar.Count} atık kaydı. Toplam: {toplamMiktar:F2} ton";
                
                if (!string.IsNullOrEmpty(notlar))
                {
                    adminMesaj += $"\n\n📝 Not: {notlar}";
                }
                
                var adminBildirimi = new Bildirim
                {
                    Mesaj = adminMesaj,
                    OlusturmaTarihi = DateTime.Now,
                    Okundu = false,
                    ToplamMiktar = toplamMiktar,
                    BildirimTipi = "BilgiMesaji",
                    IlgiliSevkiyatId = sevkiyat.Id,
                    HedefKullaniciId = null // ⭐ Admin bildirimi (herkese görünür değil, sadece admin'e)
                };

                await _bildirimService.CreateAsync(adminBildirimi);

                return Json(new { 
                    success = true, 
                    message = $"Sevkiyat planlandı! {sevkiyatKullanicilari.Count} firmaya bildirim gönderildi.",
                    sevkiyatId = sevkiyat.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        public async Task<IActionResult> GecmisSevkiyatlar(
            string? atikKoduFiltre, 
            string? atikAdiFiltre, 
            DateTime? baslangicTarihi, 
            DateTime? bitisTarihi,
            string? durumFiltre,
            string? halTipiFiltre,
            string? firmaAdiFiltre)
        {
            // Tüm sevkiyatları getir
            var tumSevkiyatlar = await _sevkiyatService.GetAllAsync();
            var sevkiyatDetaylar = new List<SevkiyatDetayDto>();

            foreach (var sevkiyat in tumSevkiyatlar)
            {
                var atikKayitlari = new List<AtikKayitDetayDto>();
                
                // Sevkiyattaki her atık kaydını getir
                foreach (var atikId in sevkiyat.AtikKayitIds)
                {
                    var atik = await _atikKayitService.GetByIdAsync(atikId);
                    if (atik != null)
                    {
                        var firmaAdi = await GetFirmaAdiAsync(atik.AtikNoktasiId);
                        
                        atikKayitlari.Add(new AtikKayitDetayDto
                        {
                            Id = atik.Id ?? "",
                            AtikKodu = atik.AtikKodu,
                            AtikAdi = atik.AtikAdi,
                            Miktar = atik.Miktar,
                            Birim = atik.Birim,
                            HalTipi = atik.HalTipi,
                            Adres = atik.Adres,
                            GorselUrl = atik.GorselUrl,
                            GirisTarihi = atik.GirisTarihi,
                            FirmaAdi = firmaAdi,
                            SevkEdildiMi = atik.SevkEdildiMi
                        });
                    }
                }

                sevkiyatDetaylar.Add(new SevkiyatDetayDto
                {
                    SevkiyatId = sevkiyat.Id ?? "",
                    SevkiyatTarihi = sevkiyat.SevkiyatTarihi,
                    Durum = sevkiyat.Durum,
                    ToplamMiktar = sevkiyat.ToplamMiktar,
                    Notlar = sevkiyat.Notlar,
                    OlusturmaTarihi = sevkiyat.OlusturmaTarihi,
                    FirmaSayisi = sevkiyat.AtikNoktasiIds.Count,
                    AtikKayitSayisi = sevkiyat.AtikKayitIds.Count,
                    AtikKayitlari = atikKayitlari
                });
            }

            // FİLTRELEME
            var filtreliSevkiyatlar = sevkiyatDetaylar.AsEnumerable();

            // Atık Kodu Filtresi
            if (!string.IsNullOrWhiteSpace(atikKoduFiltre))
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.AtikKayitlari.Any(a => 
                        a.AtikKodu.Contains(atikKoduFiltre, StringComparison.OrdinalIgnoreCase)));
            }

            // Atık Adı Filtresi
            if (!string.IsNullOrWhiteSpace(atikAdiFiltre))
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.AtikKayitlari.Any(a => 
                        a.AtikAdi.Contains(atikAdiFiltre, StringComparison.OrdinalIgnoreCase)));
            }

            // Başlangıç Tarihi Filtresi
            if (baslangicTarihi.HasValue)
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.SevkiyatTarihi.Date >= baslangicTarihi.Value.Date);
            }

            // Bitiş Tarihi Filtresi
            if (bitisTarihi.HasValue)
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.SevkiyatTarihi.Date <= bitisTarihi.Value.Date);
            }

            // Durum Filtresi
            if (!string.IsNullOrWhiteSpace(durumFiltre))
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.Durum.Equals(durumFiltre, StringComparison.OrdinalIgnoreCase));
            }

            // Hal Tipi Filtresi
            if (!string.IsNullOrWhiteSpace(halTipiFiltre))
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.AtikKayitlari.Any(a => 
                        a.HalTipi.Equals(halTipiFiltre, StringComparison.OrdinalIgnoreCase)));
            }

            // Firma Adı Filtresi
            if (!string.IsNullOrWhiteSpace(firmaAdiFiltre))
            {
                filtreliSevkiyatlar = filtreliSevkiyatlar
                    .Where(s => s.AtikKayitlari.Any(a => 
                        a.FirmaAdi.Contains(firmaAdiFiltre, StringComparison.OrdinalIgnoreCase)));
            }

            var filtreliListe = filtreliSevkiyatlar
                .OrderByDescending(s => s.SevkiyatTarihi)
                .ToList();

            var model = new GecmisSevkiyatlarViewModel
            {
                Sevkiyatlar = filtreliListe,
                AtikKoduFiltre = atikKoduFiltre,
                AtikAdiFiltre = atikAdiFiltre,
                BaslangicTarihi = baslangicTarihi,
                BitisTarihi = bitisTarihi,
                DurumFiltre = durumFiltre,
                HalTipiFiltre = halTipiFiltre,
                FirmaAdiFiltre = firmaAdiFiltre,
                ToplamSevkiyatSayisi = filtreliListe.Count,
                ToplamAtikMiktari = filtreliListe.Sum(s => s.ToplamMiktar),
                ToplamAtikKayitSayisi = filtreliListe.Sum(s => s.AtikKayitSayisi)
            };

            return View(model);
        }
    public IActionResult Map()
    {
        var atiklar1 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 3.2m },
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 2.8m },
            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 1.5m },
            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 1.0m }
        };

        var nokta1 = new AtikNoktaHarita
        {
            NoktaAdi = "GEBAŞ Ana Tesis",
            Enlem = 40.8023,
            Boylam = 29.4310,
            ToplamMiktar = 8.5m,
            Atiklar = atiklar1
        };

        var atiklar2 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "17.01.01", AtikAdi = "Beton", Miktar = 2.5m },
            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 1.7m },
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 1.0m }
        };

        var nokta2 = new AtikNoktaHarita
        {
            NoktaAdi = "Gebze OSB Depo-A",
            Enlem = 40.8156,
            Boylam = 29.4589,
            ToplamMiktar = 5.2m,
            Atiklar = atiklar2
        };

        var atiklar3 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 1.5m },
            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 0.8m },
            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 0.5m }
        };

        var nokta3 = new AtikNoktaHarita
        {
            NoktaAdi = "Gebze Merkez Depo",
            Enlem = 40.7953,
            Boylam = 29.4203,
            ToplamMiktar = 2.8m,
            Atiklar = atiklar3
        };

        var atiklar4 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 3.5m },
            new AtikDetay { AtikKodu = "17.01.01", AtikAdi = "Beton", Miktar = 2.0m },
            new AtikDetay { AtikKodu = "15.01.02", AtikAdi = "Plastik Ambalaj", Miktar = 1.8m }
        };

        var nokta4 = new AtikNoktaHarita
        {
            NoktaAdi = "Çayırova Atık Noktası",
            Enlem = 40.8234,
            Boylam = 29.3845,
            ToplamMiktar = 7.3m,
            Atiklar = atiklar4
        };

        var atiklar5 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 0.8m },
            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 0.7m }
        };

        var nokta5 = new AtikNoktaHarita
        {
            NoktaAdi = "Dilovası Sanayi",
            Enlem = 40.7812,
            Boylam = 29.5234,
            ToplamMiktar = 1.5m,
            Atiklar = atiklar5
        };

        var atiklar6 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 2.3m },
            new AtikDetay { AtikKodu = "15.01.02", AtikAdi = "Plastik Ambalaj", Miktar = 1.4m },
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 1.0m }
        };

        var nokta6 = new AtikNoktaHarita
        {
            NoktaAdi = "Plastik OSB Depo-B",
            Enlem = 40.8089,
            Boylam = 29.4678,
            ToplamMiktar = 4.7m,
            Atiklar = atiklar6
        };

        var atiklar7 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 4.0m },
            new AtikDetay { AtikKodu = "17.01.01", AtikAdi = "Beton", Miktar = 3.2m },
            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 2.0m }
        };

        var nokta7 = new AtikNoktaHarita
        {
            NoktaAdi = "Kimya Sanayi Atık Alanı",
            Enlem = 40.8267,
            Boylam = 29.4456,
            ToplamMiktar = 9.2m,
            Atiklar = atiklar7
        };

        var atiklar8 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 2.5m },
            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 2.0m },
            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 1.6m }
        };

        var nokta8 = new AtikNoktaHarita
        {
            NoktaAdi = "Darıca Transfer İstasyonu",
            Enlem = 40.7623,
            Boylam = 29.3892,
            ToplamMiktar = 6.1m,
            Atiklar = atiklar8
        };

        var atiklar9 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 1.8m },
            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 1.0m },
            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 0.6m }
        };

        var nokta9 = new AtikNoktaHarita
        {
            NoktaAdi = "Teknoloji Merkezi Depo",
            Enlem = 40.7934,
            Boylam = 29.4534,
            ToplamMiktar = 3.4m,
            Atiklar = atiklar9
        };

        var atiklar10 = new List<AtikDetay>
        {
            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 3.8m },
            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 2.5m },
            new AtikDetay { AtikKodu = "15.01.02", AtikAdi = "Plastik Ambalaj", Miktar = 1.5m }
        };

        var nokta10 = new AtikNoktaHarita
        {
            NoktaAdi = "Otomotiv OSB",
            Enlem = 40.8345,
            Boylam = 29.4123,
            ToplamMiktar = 7.8m,
            Atiklar = atiklar10
        };

        var model = new MapViewModel();
        model.AtikNoktaları.Add(nokta1);
        model.AtikNoktaları.Add(nokta2);
        model.AtikNoktaları.Add(nokta3);
        model.AtikNoktaları.Add(nokta4);
        model.AtikNoktaları.Add(nokta5);
        model.AtikNoktaları.Add(nokta6);
        model.AtikNoktaları.Add(nokta7);
        model.AtikNoktaları.Add(nokta8);
        model.AtikNoktaları.Add(nokta9);
        model.AtikNoktaları.Add(nokta10);

        return View(model);
    }
    }
}
