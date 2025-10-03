using Microsoft.AspNetCore.Mvc;
using AtikProj.Models;

namespace AtikProj.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Mock Data
            var model = new DashboardViewModel
            {
                ToplamAktifAtik = 7.8m,
                HedefMiktar = 10m,
                AtikNoktaSayisi = 20,
                
                AtikNoktaları = new List<AtikNoktaDetay>
                {
                    new AtikNoktaDetay
                    {
                        NoktaAdi = "Depo-1",
                        ToplamMiktar = 2.3m,
                        AtikSayisi = 3,
                        Atiklar = new List<AtikDetay>
                        {
                            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 1.5m },
                            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 0.5m },
                            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 0.3m }
                        }
                    },
                    new AtikNoktaDetay
                    {
                        NoktaAdi = "Depo-2",
                        ToplamMiktar = 1.5m,
                        AtikSayisi = 2,
                        Atiklar = new List<AtikDetay>
                        {
                            new AtikDetay { AtikKodu = "17.01.01", AtikAdi = "Beton", Miktar = 1.0m },
                            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 0.5m }
                        }
                    },
                    new AtikNoktaDetay
                    {
                        NoktaAdi = "Üretim Alanı-A",
                        ToplamMiktar = 1.8m,
                        AtikSayisi = 4,
                        Atiklar = new List<AtikDetay>
                        {
                            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 0.8m },
                            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 0.4m },
                            new AtikDetay { AtikKodu = "15.01.02", AtikAdi = "Plastik Ambalaj", Miktar = 0.3m },
                            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 0.3m }
                        }
                    },
                    new AtikNoktaDetay
                    {
                        NoktaAdi = "Depo-3",
                        ToplamMiktar = 0.9m,
                        AtikSayisi = 2,
                        Atiklar = new List<AtikDetay>
                        {
                            new AtikDetay { AtikKodu = "20.01.01", AtikAdi = "Kağıt", Miktar = 0.6m },
                            new AtikDetay { AtikKodu = "15.01.01", AtikAdi = "Plastik", Miktar = 0.3m }
                        }
                    },
                    new AtikNoktaDetay
                    {
                        NoktaAdi = "Üretim Alanı-B",
                        ToplamMiktar = 1.3m,
                        AtikSayisi = 3,
                        Atiklar = new List<AtikDetay>
                        {
                            new AtikDetay { AtikKodu = "12.01.01", AtikAdi = "Metal", Miktar = 0.7m },
                            new AtikDetay { AtikKodu = "17.01.01", AtikAdi = "Beton", Miktar = 0.4m },
                            new AtikDetay { AtikKodu = "16.01.03", AtikAdi = "Lastikler", Miktar = 0.2m }
                        }
                    }
                },

                GecmisSevkiyatlar = new List<GecmisSevkiyat>
                {
                    new GecmisSevkiyat
                    {
                        Tarih = DateTime.Now.AddDays(-5),
                        ToplamMiktar = 10.5m,
                        NoktaSayisi = 12,
                        Detaylar = "Depo-1, Depo-2, Üretim-A, ..."
                    },
                    new GecmisSevkiyat
                    {
                        Tarih = DateTime.Now.AddDays(-18),
                        ToplamMiktar = 11.2m,
                        NoktaSayisi = 15,
                        Detaylar = "Depo-1, Depo-3, Üretim-B, ..."
                    },
                    new GecmisSevkiyat
                    {
                        Tarih = DateTime.Now.AddDays(-32),
                        ToplamMiktar = 9.8m,
                        NoktaSayisi = 11,
                        Detaylar = "Depo-2, Üretim-A, Depo-5, ..."
                    }
                }
            };

            // Atık türlerine göre dağılım hesapla
            model.AtikDagilim = model.AtikNoktaları
                .SelectMany(n => n.Atiklar)
                .GroupBy(a => a.AtikAdi)
                .Select(g => new AtikDagılım
                {
                    AtikAdi = g.Key,
                    Miktar = g.Sum(a => a.Miktar)
                })
                .OrderByDescending(a => a.Miktar)
                .ToList();

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
