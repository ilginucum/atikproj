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

        public DashboardController(IAtikKayitService atikKayitService, IKullaniciService kullaniciService, IBildirimService bildirimService) 
        {
            _atikKayitService = atikKayitService;
            _kullaniciService = kullaniciService;
            _bildirimService = bildirimService;
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
            var bildirimler = await _bildirimService.GetAllAsync();
            return View(bildirimler);
        }

        // API endpoint - okunmamış sayısı için
        [HttpGet]
        public async Task<JsonResult> GetOkunmamisBildirimSayisi()
        {
            var sayi = await _bildirimService.GetOkunmamisSayisi();
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
