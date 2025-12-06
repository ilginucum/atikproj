using Microsoft.AspNetCore.Mvc;
using AtikProj.Models;
using AtikProj.Services;
using AtikProj.Filters;

namespace AtikProj.Controllers
{
    [Authorize]
    public class AtikNoktasiController : Controller
    {
        private readonly IAtikKayitService _atikKayitService;
        private readonly IBildirimService _bildirimService;

        public AtikNoktasiController(IAtikKayitService atikKayitService, IBildirimService bildirimService)
        {
            _atikKayitService = atikKayitService;
            _bildirimService = bildirimService;
        }
        private (string kullaniciId, string atikNoktasiId, string firmaAdi) GetCurrentUser()
        {
            var kullaniciId = HttpContext.Session.GetString("KullaniciId") ?? "";
            var atikNoktasiId = HttpContext.Session.GetString("AtikNoktasiId") ?? "";
            var firmaAdi = HttpContext.Session.GetString("FirmaAdi") ?? "";
            
            return (kullaniciId, atikNoktasiId, firmaAdi);
        }

        public IActionResult AtikGirisi()
        {
            return View();
        }

        private static readonly List<(string Kod, string Ad)> sabitAtikKodlari = new List<(string, string)>
        {
            // Atık Elektrikli ve Elektronik Eşya İşleme (R12)
            ("16.02.11", "Kloroflorokarbon, HCFC, HFC içeren ıskarta ekipmanlar"),
            ("16.02.13", "16 02 09'dan 16 02 12'ye kadar olanların dışındaki tehlikeli parçalar içeren ıskarta ekipmanlar"),
            ("16.02.14", "16 02 09'dan 16 02 13'e kadar olanların dışındaki ıskarta ekipmanlar"),
            ("16.02.15", "Iskarta ekipmanlardan çıkartılmış tehlikeli parçalar"),
            ("16.02.16", "16 02 15 dışındaki ıskarta ekipmanlardan çıkartılmış parçalar"),
            ("20.01.21", "Flüoresan lambalar ve diğer cıva içeren atıklar"),
            ("20.01.23", "Kloroflorokarbonlar içeren ıskartaya çıkartılmış ekipmanlar"),
            ("20.01.35", "20 01 21 ve 20 01 23 dışındaki tehlikeli parçalar içeren ve ıskartaya çıkmış elektrikli ve elektronik ekipmanlar"),
            ("20.01.36", "20 01 21, 20 01 23 ve 20 01 35 dışındaki ıskarta elektrikli ve elektronik ekipmanlar"),

            // Hurda Metal /ÖTA İşleme (R12)
            ("02.01.10", "Atık metal"),
            ("10.03.02", "Anot hurdaları"),
            ("11.05.01", "Katı çinko"),
            ("12.01.01", "Demir metal çapakları ve talaşları"),
            ("12.01.02", "Demir metal toz ve parçacıklar"),
            ("12.01.03", "Demir dışı metal çapakları ve talaşları"),
            ("12.01.04", "Demir dışı metal toz ve parçacıklar"),
            ("12.01.13", "Kaynak atıkları"),
            ("12.01.21", "12 01 20 dışındaki öğütme parçaları ve öğütme maddeleri"),
            ("16.01.06", "Sıvı ya da tehlikeli maddeler içermeyen ömrünü tamamlamış araçlar"),
            ("16.01.12", "16 01 11 dışındaki fren balataları"),
            ("16.01.16", "Sıvılaştırılmış gaz tankları"),
            ("16.01.17", "Demir metaller"),
            ("16.01.18", "Demir olmayan metaller"),
            ("16.01.22", "Başka bir şekilde tanımlanmamış parçalar"),
            ("17.04.01", "Bakır, bronz, pirinç"),
            ("17.04.02", "Alüminyum"),
            ("17.04.03", "Kurşun"),
            ("17.04.04", "Çinko"),
            ("17.04.05", "Demir ve çelik"),
            ("17.04.06", "Kalay"),
            ("17.04.07", "Karışık metaller"),
            ("19.10.01", "Demir ve çelik atıkları"),
            ("19.10.02", "Demir olmayan atıklar"),
            ("19.12.02", "Demir metali"),
            ("19.12.03", "Demir dışı metal"),
            ("20.01.40", "Metaller"),

            // Ömrünü Tamamlamış Araç Geçici Depolama (R12)
            ("16.01.04", "Ömrünü tamamlamış araçlar"),
            ("16.01.10", "Patlayıcı parçalar (örneğin hava yastıkları)"),
            ("16.01.03", "Lastikler"),
            ("16.01.19", "Plastik"),
            ("16.01.20", "Cam"),

            // PCB Arındırma (R12)
            ("13.01.01", "PCB (2) içeren hidrolik yağlar"),
            ("16.01.09", "PCB içeren parçalar"),
            ("16.02.09", "PCB'ler içeren transformatörler ve kapasitörler"),
            ("17.09.02", "PCB içeren inşaat ve yıkım atıkları (örneğin PCB içeren dolgu macunları, PCB içeren reçine bazlı taban kaplama malzemeleri, PCB içeren kaplanmış sırlama birimleri, PCB içeren kapasitörler)"),
            ("16.02.10", "16 02 09 dışındaki PCB içeren ya da PCB ile kontamine olmuş ıskarta ekipmanlar"),

            // Tanker Temizleme (R3)
            ("15.01.10", "Tehlikeli maddelerin kalıntılarını içeren ya da tehlikeli maddelerle kontamine olmuş ambalajlar"),

            // Tehlikeli Atık Geri Kazanım (R3,R5)
            ("08.03.17", "Tehlikeli maddeler içeren atık baskı tonerleri"),
            ("15.02.02", "Tehlikeli maddelerle kirlenmiş emiciler, filtre malzemeleri (başka şekilde tanımlanmamış ise yağ filtreleri), temizleme bezleri, koruyucu giysiler"),
            ("16.05.04", "Basınçlı tanklar içinde tehlikeli maddeler içeren gazlar (halonlar dahil)"),
            ("17.02.04", "Tehlikeli maddeler içeren ya da tehlikeli maddelerle kontamine olmuş ahşap, cam ve plastik"),
            ("17.04.10", "Yağ, katran ve diğer tehlikeli maddeler içeren kablolar"),
            ("17.06.03", "Tehlikeli maddelerden oluşan ya da tehlikeli maddeler içeren diğer yalıtım malzemeleri"),

            // Tehlikeli Atık Ön İşlem Tesisleri (R12)
            ("12.01.20", "Tehlikeli maddeler içeren öğütme parçaları ve öğütme maddeleri"),
            ("15.01.11", "Boş basınçlı konteynırlar dahil olmak üzere tehlikeli gözenekli katı yapılı (örneğin asbest) metalik ambalajlar"),
            ("16.01.07", "Yağ filtreleri"),
            ("16.01.21", "16 01 07'den 16 01 11'e ve 16 01 13 ile 16 01 14 dışındaki tehlikeli parçalar"),
            ("16.03.03", "Tehlikeli maddeler içeren anorganik atıklar"),
            ("16.03.05", "Tehlikeli maddeler içeren organik atıklar"),
            ("16.08.02", "Tehlikeli geçiş metalleri ya da tehlikeli geçiş metal bileşenlerini içeren bitik katalizörler"),
            ("16.08.07", "Tehlikeli maddelerle kontamine olmuş bitik katalizörler"),
            ("17.04.09", "Tehlikeli maddelerle kontamine olmuş metal atıkları"),
            ("17.05.03", "Tehlikeli maddeler içeren toprak ve kayalar"),
            ("17.09.03", "Tehlikeli maddeler içeren diğer inşaat ve yıkım atıkları (karışık atıklar dahil)"),

            // Tehlikesiz Atık Geri Kazanım (R3)
            ("02.01.04", "Atık plastikler (ambalajlar hariç)"),
            ("07.02.13", "Atık plastik"),
            ("08.03.18", "08 03 17 dışındaki atık baskı tonerleri"),
            ("12.01.05", "Plastik yongalar ve çapaklar"),
            ("15.01.02", "Plastik ambalaj"),
            ("15.01.03", "Ahşap ambalaj"),
            ("17.02.03", "Plastik"),
            ("17.04.11", "17 04 10 dışındaki kablolar"),
            ("19.12.04", "Plastik ve lastik"),
            ("20.01.39", "Plastikler"),

            // Tehlikesiz Atık Ön İşlem Tesisleri (R12)
            ("02.02.03", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.02.04", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.03.04", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.05.01", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.06.01", "Tüketime ve işlenmeye uygun olmayan maddeler"),
            ("02.07.04", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("16.03.04", "16 03 03 dışındaki anorganik atıklar"),
            ("16.03.06", "16 03 05 dışındaki organik atıklar"),
            ("16.08.01", "Altın, gümüş, renyum, rodyum, paladyum, iridyum ya da platin içeren bitik katalizörler (16 08 07 hariç)"),
            ("16.08.03", "Başka bir şekilde tanımlanmamış ara metaller ve ara metal bileşenleri içeren bitik katalizörler"),
            ("17.06.04", "17 06 01 ve 17 06 03 dışındaki yalıtım malzemeleri"),

            // Toplama Ayırma Tesisi Tip 3 (R12)
            ("03.01.01", "Ağaç kabuğu ve mantar atıkları"),
            ("03.01.05", "03 01 04 dışındaki talaş, yonga, kıymık, ahşap, kontraplak ve kaplamalar"),
            ("03.03.01", "Ağaç kabuğu ve odun atıkları"),
            ("04.02.09", "Kompozit malzeme atıkları (emprenye edilmiş tekstil, elastomer, plastomer)"),
            ("04.02.21", "İşlenmemiş tekstil elyafı atıkları"),
            ("04.02.22", "İşlenmiş tekstil elyafı atıkları"),
            ("10.11.05", "Partiküller ve toz"),
            ("10.11.12", "10 11 11 dışındaki atık camlar"),
            ("15.01.01", "Kağıt ve karton ambalaj"),
            ("15.01.04", "Metalik ambalaj"),
            ("15.01.05", "Kompozit ambalaj"),
            ("15.01.06", "Karışık ambalaj"),
            ("15.01.07", "Cam ambalaj"),
            ("15.01.09", "Tekstil ambalaj"),
            ("17.01.01", "Beton"),
            ("17.02.01", "Ahşap"),
            ("17.02.02", "Cam"),
            ("19.12.01", "Kağıt ve karton"),
            ("19.12.05", "Cam"),
            ("19.12.07", "19 12 06 dışındaki ahşap"),
            ("19.12.08", "Tekstil malzemeleri"),
            ("20.01.01", "Kâğıt ve karton"),
            ("20.01.02", "Cam"),
            ("20.01.10", "Giysiler"),
            ("20.01.11", "Tekstil ürünleri"),
            ("20.01.38", "20 01 37 dışındaki ahşap"),
            ("20.03.07", "Hacimli atıklar"),

            // Yeniden Kullanıma Hazırlama (R3)
            ("15.01.03", "Ahşap ambalaj"),

            // Atık Yakma ve Beraber Yakma (D10,R1)
            ("01.05.05", "Yağ içeren sondaj çamurları ve atıkları"),
            ("01.05.06", "Tehlikeli maddeler içeren sondaj çamurları ve diğer sondaj atıkları"),
            ("01.05.07", "01 05 05 ve 01 05 06 dışındaki barit içeren sondaj çamurları ve atıkları"),
            ("01.05.08", "01 05 05 ve 01 05 06 dışındaki klorür içeren sondaj çamurları ve atıkları"),
            ("02.01.01", "Yıkama ve temizleme işlemlerinden kaynaklanan çamurlar"),
            ("02.01.03", "Bitki dokusu atıkları"),
            ("02.01.04", "Atık plastikler (ambalajlar hariç)"),
            ("02.01.07", "Ormancılık atıkları"),
            ("02.01.08", "Zirai kimyasal atıklar"),
            ("02.01.09", "02 01 08 dışındaki zirai kimyasal atıkları"),
            ("02.02.01", "Yıkama ve temizlemeden kaynaklanan çamurlar"),
            ("02.02.03", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.03.01", "Yıkama, temizleme, soyma, santrifüj ve ayırma işlemlerinden kaynaklanan çamurlar"),
            ("02.03.02", "Koruyucu katkı maddelerinden kaynaklanan atıklar"),
            ("02.03.03", "Çözücü ekstraksiyonundan kaynaklanan atıklar"),
            ("02.03.04", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.03.05", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan atıklar"),
            ("02.04.03", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.05.01", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.05.02", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.06.01", "Tüketime ve işlenmeye uygun olmayan maddeler"),
            ("02.06.02", "Koruyucu katkı maddelerinden kaynaklanan atıklar"),
            ("02.06.03", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.07.01", "Hammaddelerin yıkanmasından, temizlenmesinden ve mekanik olarak sıkılmasından kaynaklanan atıklar"),
            ("02.07.02", "Alkol damıtılmasından kaynaklanan atıklar"),
            ("02.07.03", "Kimyasal işlem atıkları"),
            ("02.07.04", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.07.05", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("03.01.04", "Tehlikeli maddeler içeren talaş, yonga, kıymık, ahşap, kontraplak ve kaplamalar"),
            ("03.01.05", "03 01 04 dışındaki talaş, yonga, kıymık, ahşap, kontraplak ve kaplamalar"),
            ("03.02.01", "Halojenlenmiş organik ahşap koruyucu maddeler"),
            ("03.02.02", "Organoklorlu ahşap koruyucu maddeler"),
            ("03.02.03", "Organometal içeren ahşap koruyucu maddeler"),
            ("03.02.04", "Anorganik ahşap koruyucu maddeler"),
            ("03.02.05", "Tehlikeli maddeler içeren diğer ahşap koruyucuları"),
            ("03.03.01", "Atık kabuk ve odun atıkları"),
            ("03.03.02", "Yeşil sıvı çamuru (pişirme sıvısı geri kazanımından)"),
            ("03.03.05", "Kağıt geri kazanım işleminden kaynaklanan mürekkep giderme çamurları"),
            ("03.03.07", "Atık kağıt ve kartonun hamur haline getirilmesi sırasında mekanik olarak ayrılan ıskartalar"),
            ("03.03.08", "Geri dönüşüm amaçlı kağıt ve kartonun ayrıştırılmasında kaynaklanan atıklar"),

            // Sulu Yıkama Sıvıları (R12,R3)
            ("12.03.01", "Sulu yıkama sıvıları"),
            ("12.03.02", "Buhar yağ alma atıkları"),
            ("13.01.01", "PCB (2) içeren hidrolik yağlar"),
            ("13.01.04", "Klor içeren emülsiyonlar"),
            ("13.01.05", "Klor içermeyen emülsiyonlar"),
            ("13.01.09", "Mineral esaslı klor içeren hidrolik yağlar"),
            ("13.01.10", "Mineral esaslı klor içermeyen hidrolik yağlar"),
            ("13.01.11", "Sentetik hidrolik yağlar"),
            ("13.01.12", "Kolayca biyolojik olarak bozunabilir hidrolik yağlar"),
            ("13.01.13", "Diğer hidrolik yağlar"),
            ("13.02.04", "Mineral esaslı klor içeren motor, şanzıman ve yağlama yağları"),
            ("13.02.05", "Mineral esaslı klor içermeyen motor, şanzıman ve yağlama yağları"),
            ("13.02.06", "Sentetik motor, şanzıman ve yağlama yağları"),
            ("13.02.07", "Kolayca biyolojik olarak bozunabilir motor, şanzıman ve yağlama yağları"),
            ("13.02.08", "Diğer motor, şanzıman ve yağlama yağları"),
            ("13.03.01", "PCB'ler içeren yalıtım ya da ısı iletim yağları"),
            ("13.03.06", "13 03 01 dışındaki mineral esaslı klor içeren yalıtım ve ısı iletim yağları"),
            ("13.03.07", "Mineral esaslı klor içermeyen yalıtım ve ısı iletim yağları"),
            ("13.03.08", "Sentetik yalıtım ve ısı iletim yağları"),
            ("13.03.09", "Kolayca biyolojik olarak bozunabilir yalıtım ve ısı iletim yağları"),
            ("13.03.10", "Diğer yalıtım ve ısı iletim yağları"),
            ("13.04.01", "İç su yolu denizciğinden kaynalanan sintine yağları"),
            ("13.04.02", "İskele kanalizasyonlarından kaynalanan sintine yağları"),
            ("13.04.03", "Diğer denizcilik seyrüseferinden kaynalanan sintine yağları"),
            ("13.05.01", "Kum odacığından ve yağ/su ayrıcısından çıkan katılar"),
            ("13.05.02", "Yağ/su ayrıcılarından çıkan çamurlar"),
            ("13.05.03", "Yakalayıcı (interseptör) çamurları"),
            ("13.05.06", "Yağ/su ayrıcılarından çıkan yağ"),
            ("13.05.07", "Kum odacığından ve yağ/su ayrıcılarından çıkan karışık atıklar"),
            ("13.05.08", "Kum odacığından ve yağ/su ayrıcılarından çıkan karışık atıklar"),
            ("13.07.01", "Fuel-oil ve mazot"),
            ("13.07.02", "Benzin"),
            ("13.07.03", "Diğer yakıtlar (karışımlar dahil)"),
            ("13.08.01", "Tuz giderim çamurları ya da emülsiyonları"),
            ("13.08.02", "Diğer emülsiyonlar"),
            ("14.06.02", "Diğer halojenli çözücüler ve çözücü karışımları"),
            ("14.06.03", "Diğer çözücüler ve çözücü karışımları"),
            ("14.06.04", "Halojenli çözücüler içeren çamurlar veya katı atıklar"),
            ("14.06.05", "Diğer çözücüler içeren çamurlar veya katı atıklar"),
            ("15.01.10", "Tehlikeli maddelerin kalıntılarını içeren ya da tehlikeli maddelerle kontamine olmuş ambalajlar"),
            ("15.02.02", "Tehlikeli maddelerle kirlenmiş emiciler, filtre malzemeleri (başka şekilde tanımlanmamış ise yağ filtreleri), temizleme bezleri, koruyucu giysiler"),
            ("15.02.03", "15 02 02 dışındaki emiciler, filtre malzemeleri, temizleme bezleri, koruyucu giysiler"),
            ("16.01.03", "Ömrünü tamamlamış lastikler"),
            ("16.01.07", "Yağ filtreleri"),
            ("16.01.13", "Fren sıvıları"),
            ("16.01.14", "Antifriz sıvıları"),
            ("16.01.15", "16 01 14 dışındaki antifriz sıvıları"),
            ("16.01.19", "Plastik"),
            ("16.03.03", "Tehlikeli maddeler içeren anorganik atıklar"),
            ("16.03.05", "Tehlikeli maddeler içeren organik atıklar"),
            ("16.03.06", "16 03 05 dışındaki organik atıklar"),
            ("16.05.06", "Laboratuvar kimyasalları karışımları dahil tehlikeli maddelerden oluşan ya da tehlikeli maddeler içeren kimyasallar"),
            ("16.05.07", "Tehlikeli maddeler içeren ya da bunlardan oluşan ıskarta anorganik kimyasallar"),
            ("16.05.08", "Tehlikeli maddeler içeren ya da bunlardan oluşan ıskarta organik kimyasallar"),
            ("16.05.09", "16 05 06, 16 05 07 ya da 16 05 08 dışında tehlikeli maddeler içeren ya da tehlikeli maddelerle kontamine olmuş ahşap, cam ve plastik"),
            ("16.07.08", "Yağ içeren atıklar"),
            ("16.07.09", "Diğer tehlikeli maddeler içeren atıklar"),
            ("16.08.05", "Fosforik asit içeren bitik katalizörler"),
            ("16.08.06", "Katalizör olarak bitik sıvılar"),
            ("16.08.07", "Tehlikeli maddelerle kontamine olmuş bitik katalizörler"),
            ("16.09.01", "Permanganatlar (örneğin potasyum permanganat)"),
            ("16.09.02", "Kromatlar (örneğin potasyum kromat, potasyum veya sodyum dikromat)"),
            ("16.09.03", "Peroksitler (örneğin hidrojen peroksit)"),
            ("16.09.04", "Başka bir şekilde tanımlanmamış oksitleyici malzemeler"),
            ("16.10.01", "Tehlikeli maddeler içeren sulu sıvı atıklar"),
            ("16.10.02", "16 10 01 dışındaki sulu sıvı atıkları"),
            ("16.10.03", "Tehlikeli madde içeren sulu derişik maddeler"),
            ("17.02.04", "Tehlikeli maddeler içeren ya da tehlikeli maddelerle kontamine olmuş ahşap, cam ve plastik"),
            ("17.03.01", "Kömür katranı içeren bitümlü karışımlar"),
            ("17.03.02", "17 03 01 dışındaki bitümlü karışımlar"),
            ("17.03.03", "Kömür katranı ve katranlı ürünler"),
            ("17.04.10", "Yağ, katran ve diğer tehlikeli maddeler içeren kablolar"),
            ("17.06.03", "Tehlikeli maddelerden oluşan ya da tehlikeli maddeler içeren diğer yalıtım malzemeleri"),
            ("17.09.01", "Cıva içeren inşaat ve yıkım atıkları"),
            ("17.09.02", "PCB içeren inşaat ve yıkım atıkları (örneğin PCB içeren dolgu macunları, PCB içeren reçine bazlı taban kaplama malzemeleri, PCB içeren kaplanmış sırlama birimleri, PCB içeren kapasitörler)"),
            ("17.09.03", "Tehlikeli maddeler içeren diğer inşaat ve yıkım atıkları (karışık atıklar dahil)"),
            ("18.01.06", "18 01 06 dışındaki kimyasallar"),
            ("18.01.07", "18 01 06 dışındaki kimyasallar"),
            ("18.01.08", "Sitotoksik ve sitotstatik ilaçlar"),
            ("18.01.09", "18 01 08 dışındaki ilaçlar"),
            ("18.01.10", "Diş tedavisinden kaynaklanan amalgam atıkları"),
            ("18.02.05", "18 02 05 dışındaki kimyasallar"),
            ("18.02.06", "18 02 05 dışındaki kimyasallar"),
            ("18.02.07", "Sitotoksik ve sitotstatik ilaçlar"),
            ("19.01.05", "Gaz arıtımından kaynaklanan filtre kekleri"),
            ("19.01.06", "Gaz arıtımından kaynaklanan sulu sıvı atıklar ve diğer sulu sıvı atıklar"),
            ("19.01.07", "Gaz arıtımından kaynaklanan katı atıklar"),
            ("19.01.10", "Baca gazı arıtımından kaynaklanan kullanılmış aktif karbon"),
            ("19.02.05", "Fiziksel ve kimyasal işlemlerden kaynaklanan çamurları"),
            ("19.02.06", "19 02 05 dışındaki fiziksel ve kimyasal işlemlerden kaynaklanan çamurları"),
            ("19.02.07", "Yağ ve sıvı ayırıcısından kaynaklanan sadece yenilenebilir yağlar içeren yağ karışımları ve gres"),
            ("19.02.08", "19 02 07 dışındaki yağ ve su ayrışmasından çıkan yağ karışımları ve gres"),
            ("19.08.06", "Doymuş ya da kullanılmış iyon değiştirici reçineler"),
            ("19.08.07", "19 02 07 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("19.08.09", "Yağ ve su ayrışmasından kaynaklanan yağ karışımları ve gres"),
            ("19.08.10", "19 08 09 dışındaki yağ ve su ayrışmasından çıkan yağ karışımları ve gres"),
            ("19.08.11", "Endüstriyel atık suyun biyolojik arıtılmasından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("19.08.13", "Endüstriyel atık suyun diğer yöntemlerle arıtılmasından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("19.08.14", "19 08 13 dışındaki endüstriyel atık su arıtımından kaynaklanan çamurlar"),
            ("10.01.07", "Sülfürik asit ve sulfüroz asit"),
            ("10.01.09", "02 01 08 dışındaki zirai kimyasal atıkları"),
            ("10.01.14", "Asitler"),
            ("10.01.15", "Alkalinler"),
            ("10.01.17", "Foto kimyasallar"),
            ("10.01.19", "Pestisitler"),
            ("10.01.27", "Tehlikeli maddeler içeren boya, mürekkeplar, yapıştırıcılar ve reçineler"),
            ("10.01.28", "20 01 27 dışındaki boya, mürekkeplar, yapıştırıcılar ve reçineler"),
            ("10.01.29", "Tehlikeli maddeler içeren deterjanlar"),
            ("10.01.30", "20 01 29 dışındaki deterjanlar"),
            ("10.01.31", "Sitotoksik ve sitotstatik ilaçlar"),
            ("10.01.32", "20 01 31 dışındaki ilaçlar"),
            ("10.01.37", "Tehlikeli maddeler içeren ahşap"),
            ("19.02.04", "Tehlikeli maddeler içeren deterjanlar"),
            ("19.02.05", "Fiziksel ve kimyasal işlemlerden kaynaklanan çamurları"),
            ("19.02.06", "19 02 05 dışındaki fiziksel ve kimyasal işlemlerden kaynaklanan çamurları"),
            ("19.02.07", "Yağ ve sıvı ayırıcısından kaynaklanan sadece yenilenebilir yağlar içeren yağ karışımları ve gres"),
            ("19.02.08", "19 02 07 dışındaki yağ ve su ayrışmasından çıkan yağ karışımları ve gres"),
            ("19.02.09", "Tehlikeli maddeler içeren sıvı yanabilir atıklar"),
            ("19.02.10", "19 02 09 dışındaki yanabilir atıklar"),
            ("19.03.04", "Tehlikeli olarak tanımlanmış çamurlar"),
            ("19.03.05", "Tehlikeli olarak tanımlanmış olmayan çamurlar"),
            ("19.03.06", "Yersiz suyunun ıslahından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("19.03.07", "Yeraltı suyunun ıslahından kaynaklanan tehlikeli maddeler içeren sulu sıvı atıklar ve sulu konsantrasyonlar"),
            ("19.04.01", "Vitriye olmuş atıklar"),
            ("19.04.02", "Hayvansal ve bitkisel atıklarının kompostlanmamış fraksiyonları"),
            ("19.04.03", "Standart dışı kompost"),
            ("19.05.02", "Hayvansal ve bitkisel atıklarının kompostlanmamış fraksiyonları"),
            ("19.05.03", "Standart dışı kompost"),
            ("19.06.04", "Hayvansal ve bitkisel atıklarının anaerobik arıtımından kaynaklanan posalar"),
            ("19.06.06", "Hayvansal ve bitkisel atıkların anaerobik arıtımından kaynaklanan posalar"),
            ("19.08.01", "Izgaralardan gelen iri katılar"),
            ("19.08.06", "Doymuş ya da kullanılmış iyon değiştirici reçineler"),
            ("19.08.07", "19 02 07 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("19.08.08", "Yağ ve su ayrışmasından kaynaklanan sadece yenilenebilir yağlar içeren yağ karışımları ve gres"),
            ("19.08.09", "Yağ ve su ayrışmasından kaynaklanan yağ karışımları ve gres"),
            ("19.08.10", "19 08 09 dışındaki yağ ve su ayrışmasından çıkan yağ karışımları ve gres"),
            ("19.08.11", "Endüstriyel atık suyun biyolojik arıtılmasından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("19.08.12", "19 08 11 dışındaki endüstriyel atık suyun biyolojik arıtılmasından kaynaklanan çamurlar"),
            ("19.08.13", "Endüstriyel atık suyun diğer yöntemlerle arıtılmasından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("19.08.14", "19 08 13 dışındaki endüstriyel atık su arıtımından kaynaklanan çamurlar"),
            ("19.09.01", "İlk filtreleme ve süzme işlemlerinden kaynaklanan katı atıklar"),
            ("19.09.02", "Su berraklaştırılmasından kaynaklanan çamurlar"),
            ("19.09.03", "Karbonat gidermeden kaynaklanan çamurlar"),
            ("19.09.04", "Kullanılmış aktif karbon"),
            ("19.09.05", "Doymuş ya da kullanılmış iyon değitirme reçineleri"),
            ("19.09.06", "İyon değiştirimin rejenerasyonundan kaynaklanan solüsyonlar ve çamurlar"),
            ("19.10.01", "Demir ve çelik atıkları"),
            ("19.10.03", "Tehlikeli maddeler içeren uçucu atık parçacıkları ve tozlar"),
            ("19.10.04", "19 10 03 dışındaki uçucu atık parçacıkları ve tozlar"),
            ("19.11.01", "Kullanılmış filtre killeri"),
            ("19.11.02", "Asit katranları"),
            ("19.11.03", "Sulu sıvı atıklar"),
            ("19.11.04", "Yakıtların bazlarla temizlenmesinden kaynaklanan atıklar"),
            ("19.11.05", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("19.11.06", "19 11 05 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("19.11.07", "Baca gazı temizleme atıkları"),
            ("19.12.01", "Kağıt ve karton"),
            ("19.12.04", "Plastik ve lastik"),
            ("19.12.06", "Halojen içeren işleme emülsiyon ve solüsyonlar hariç"),
            ("19.12.07", "19 12 06 dışındaki ahşap"),
            ("19.12.08", "Tekstil malzemeleri"),
            ("19.12.09", "Mineraller (örn. kum, taşlar)"),
            ("19.12.10", "Yanabilir atıklar (atıktan türetilmiş yakıt)"),
            ("19.12.11", "Atıkların mekanik işlenmesinden kaynaklanana tehlikeli maddeler içeren diğer atıklar (karışık malzemeler dahil)"),
            ("19.12.12", "19 12 11 dışındaki atıkların mekanik işlenmesinden kaynaklanan diğer atıklar (karışık malzemeler dahil)"),

            // TESİSE KABUL EDİLECEK ATIKLAR VE KODLARI
            ("01.03.05", "Yağ içeren sondaj çamurları ve atıkları"),
            ("01.05.06", "Tehlikeli maddeler içeren sondaj çamurları ve diğer sondaj atıkları"),
            ("01.05.07", "01 05 05 ve 01 05 06 dışındaki barit içeren sondaj çamurları ve atıkları"),
            ("01.05.08", "01 05 05 ve 01 05 06 dışındaki klorür içeren sondaj çamurları ve atıkları"),
            ("02.01.01", "Yıkama ve temizleme işlemlerinden kaynaklanan çamurlar"),
            ("02.01.03", "Bitki dokusu atıkları"),
            ("02.01.04", "Atık plastikler (ambalajlar hariç)"),
            ("02.01.07", "Ormancılık atıkları"),
            ("02.01.08", "Zirai kimyasal atıklar"),
            ("02.01.09", "02 01 08 dışındaki zirai kimyasal atıkları"),
            ("02.02.01", "Yıkama ve temizlemeden kaynaklanan çamurlar"),
            ("02.02.03", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.03.01", "Yıkama, temizleme, soyma, santrifüj ve ayırma işlemlerinden kaynaklanan çamurlar"),
            ("02.03.02", "Koruyucu katkı maddelerinden kaynaklanan atıklar"),
            ("02.03.03", "Çözücü ekstraksiyonundan kaynaklanan atıklar"),
            ("02.03.04", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.03.05", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan atıklar"),
            ("02.04.03", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.05.01", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.05.02", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.06.01", "Tüketime ve işlenmeye uygun olmayan maddeler"),
            ("02.06.02", "Koruyucu katkı maddelerinden kaynaklanan atıklar"),
            ("02.06.03", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("02.07.01", "Hammaddelerin yıkanmasından, temizlenmesinden ve mekanik olarak sıkılmasından kaynaklanan atıklar"),
            ("02.07.02", "Alkol damıtılmasından kaynaklanan atıklar"),
            ("02.07.03", "Kimyasal işlem atıkları"),
            ("02.07.04", "Tüketime ya da işlenmeye uygun olmayan maddeler"),
            ("02.07.05", "İşletme sahası içerisindeki atık su arıtımından kaynaklanan çamurlar"),
            ("03.01.04", "Tehlikeli maddeler içeren talaş, yonga, kıymık, ahşap, kontraplak ve kaplamalar"),
            ("03.01.05", "03 01 04 dışındaki talaş, yonga, kıymık, ahşap, kontraplak ve kaplamalar"),
            ("03.02.01", "Halojenlenmiş organik ahşap koruyucu maddeler"),
            ("03.02.02", "Organoklorlu ahşap koruyucu maddeler"),
            ("03.02.03", "Organometal içeren ahşap koruyucu maddeler"),
            ("03.02.04", "Anorganik ahşap koruyucu maddeler"),
            ("03.02.05", "Tehlikeli maddeler içeren diğer ahşap koruyucuları"),
            ("03.03.01", "Atık kabuk ve odun atıkları"),
            ("03.03.02", "Yeşil sıvı çamuru (pişirme sıvısı geri kazanımından)"),
            ("03.03.05", "Kağıt geri kazanım işleminden kaynaklanan mürekkep giderme çamurları"),
            ("03.03.07", "Atık kağıt ve kartonun hamur haline getirilmesi sırasında mekanik olarak ayrılan ıskartalar"),
            ("03.03.08", "Geri dönüşüm amaçlı kağıt ve kartonun ayrıştırılmasında kaynaklanan atıklar"),
            ("03.03.10", "Mekanik ayırma sonucu oluşan elyaf iskartaları, elyaf, dolgu ve yüzey kaplama maddesi çamurları"),
            ("03.03.11", "03 03 10 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("04.01.01", "Soymadan kaynaklanan atıklar"),
            ("04.01.03", "Sıvı halde olmayan çözücüler içeren yağ giderme atıkları"),
            ("04.01.04", "Krom içeren sepi şerbeti"),
            ("04.01.05", "Krom içermeyen sepi şerbeti"),
            ("04.01.06", "Saha içi atıksu arıtımından kaynaklanan krom içeren çamurlar"),
            ("04.01.07", "04 01 06 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("04.01.08", "Krom içeren tabaklanmış atık deri (çivitli parçalar, traşlamalar, kesimler, parlatma tozu)"),
            ("04.01.09", "Perdah ve boyama atıkları"),
            ("04.02.09", "Kompozit malzeme atıkları (emprenye edilmiş tekstil, elastomer, plastomer)"),
            ("04.02.10", "Doğal ürünlerden oluşan organik maddeler (örneğin yağ, mum)"),
            ("04.02.14", "Organik çözücüler içeren perdah atıkları"),
            ("04.02.15", "04 02 14 dışındaki perdah atıkları"),
            ("04.02.16", "Tehlikeli maddeler içeren boya maddeleri ve pigmentler"),
            ("04.02.17", "04 02 16 dışındaki boya maddeleri ve pigmentler"),
            ("04.02.19", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("04.02.20", "04 02 19 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("04.02.21", "İşlenmemiş tekstil elyafı atıkları"),
            ("04.02.22", "İşlenmiş tekstil elyafı atıkları"),
            ("05.01.02", "Tuz arındırma(tuz giderici) çamurları"),
            ("05.01.03", "Tank dibi çamurları"),
            ("05.01.04", "Asit alkil çamurları"),
            ("05.01.05", "Petrol dökülmülleri"),
            ("05.01.06", "İşletme ya da ekipman bakım çalışmalarından kaynaklanan yağlı çamurlar"),
            ("05.01.07", "Asit ziftleri"),
            ("05.01.09", "Saha içi atıksu arıtımından kaynaklanan tehlikeli madde içeren çamurlar"),
            ("05.01.10", "05 01 09 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("05.01.11", "Yakıtların bazlar ile temizlemesi sonucu oluşan atıklar"),
            ("05.01.12", "Yağ içeren asitler"),
            ("05.01.13", "Kazan besleme suyu çamurları"),
            ("05.01.14", "Soğutma kolonlarından kaynaklanan atıklar"),
            ("05.01.15", "Kullanılmış filtre killeri"),
            ("05.01.16", "Sülfür içeren rafineri işlemlerinden kaynaklanan atıklar"),
            ("05.01.17", "Bitüm"),
            ("05.06.01", "Asit ziftleri"),
            ("05.06.03", "Diğer ziftler"),
            ("05.06.04", "Soğutma kolonlarından kaynaklanan atıklar"),
            ("06.01.01", "Sülfürik asit ve sülfüroz asit"),
            ("06.01.02", "Hidroklorik asit"),
            ("06.01.03", "Hidroflorik asit"),
            ("06.01.04", "Fosforik ve fosforöz asit"),
            ("06.01.05", "Nitrik asit ve nitröz asit"),
            ("06.01.06", "Diğer asitler"),
            ("06.02.03", "Amonyum hidroksit"),
            ("06.02.04", "Sodyum ve potasyum hidroksit"),
            ("06.02.05", "Diğer bazlar"),
            ("06.03.11", "03 03 10 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("06.03.12", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("06.05.02", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("06.05.03", "06 05 02 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("06.06.02", "Kükürt bileşenlerinin içeren atıklar"),
            ("06.06.03", "06 06 02 dışındaki kükürt bileşenlerinin içeren atıklar"),
            ("07.01.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.01.03", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.01.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.01.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.01.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.01.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.01.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.01.11", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("07.01.12", "07 01 11 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("07.02.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.02.03", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.02.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.02.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.02.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.02.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.02.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.02.11", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("07.02.12", "07 02 11 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("07.02.13", "Atık plastik"),
            ("07.02.14", "Tehlikeli maddeler içeren katı maddelerinin atıkları"),
            ("07.02.15", "07 02 14 dışındaki katı maddelerin atıkları"),
            ("07.02.16", "Zararlı silikonlar içeren atıklar"),
            ("07.02.17", "07 02 16 dışında silikon içeren atıklar"),
            ("07.03.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.03.03", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.03.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.03.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.03.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.03.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.03.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.04.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.04.03", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.04.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.04.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.04.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.04.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.04.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.04.11", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("07.04.12", "07 04 11 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("07.04.13", "Tehlikeli madde içeren katı atıklar"),
            ("07.05.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.05.03", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.05.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.05.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.05.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.05.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.05.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.05.11", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("07.05.12", "07 05 11 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("07.05.13", "Tehlikeli maddeler içeren katı atıklar"),
            ("07.05.14", "07 05 13 dışındaki katı atıklar"),
            ("07.06.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.06.03", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.06.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.06.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.06.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.06.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.06.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.06.11", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("07.06.12", "07 06 11 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("07.07.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("07.07.03", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.07.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("07.07.07", "Halojenli dip tortusu ve reaksiyon kalıntıları"),
            ("07.07.08", "Diğer dip tortusu ve reaksiyon kalıntıları"),
            ("07.07.09", "Halojenli filtre kekleri ve kullanılmış absorbanlar"),
            ("07.07.10", "Diğer filtre kekleri ve kullanılmış absorbanlar"),
            ("07.07.11", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("07.07.12", "07 07 11 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("08.01.11", "Tehlikeli maddeler içeren atık boya ve vernik çamurları"),
            ("08.01.12", "08 01 11 dışındaki atık boya ve vernikler"),
            ("08.01.13", "Organik çözücüler ya da diğer tehlikeli maddeler içeren atık boya ve vernik çamurları"),
            ("08.01.14", "08 01 13 dışındaki atık boya ve vernik çamurları"),
            ("08.01.15", "Organik çözücüler veya diğer tehlikeli maddeler içeren boya ve vernik sulu çamurları"),
            ("08.01.16", "08 01 15 dışındaki boya ve vernik sulu çamurları"),
            ("08.01.17", "Boya ve vernik sökülmesinden kaynaklanan atıklar"),
            ("08.01.18", "08 01 17 dışındaki boya ve vernik sökülmesinden kaynaklanan atıklar"),
            ("08.01.19", "Su süspansiyonları içeren boya ve vernikler"),
            ("08.01.20", "08 01 19 dışındaki sulu boya ya da vernik süspansiyonları"),
            ("08.01.21", "Boya ya da vernik sökücü atıkları"),
            ("08.03.12", "Tehlikeli maddeler içeren atık boya ve vernik çamurları"),
            ("08.03.13", "08 03 12 dışındaki mürekkep atıkları"),
            ("08.03.14", "Tehlikeli maddeler içeren mürekkep çamurları"),
            ("08.03.15", "Tehlikeli maddeler içeren mürekkep çamurları"),
            ("08.03.16", "Atık aşındırma solüsyonları"),
            ("08.03.17", "Tehlikeli maddeler içeren atık baskı tonerleri"),
            ("08.03.18", "08 03 17 dışındaki atık baskı tonerleri"),
            ("08.03.19", "Dispersiyon yağları"),
            ("08.04.09", "Organik çözücüler ya da diğer tehlikeli maddeler içeren atık yapışkan ve sızdırmazlık maddeleri"),
            ("08.04.10", "08 04 09 dışındaki atık yapışkan ve dolgu macunları"),
            ("08.04.11", "Organik çözücüler ya da diğer tehlikeli maddeler içeren sulu yapışkan ve dolgu macunu çamurları"),
            ("08.04.12", "08 04 11 dışındaki yapışkan ve dolgu macunu çamurları"),
            ("08.04.13", "Organik çözücüler ya da diğer tehlikeli maddeler içeren sulu yapışkan ve dolgu macunu"),
            ("08.04.14", "08 04 13 dışındaki sulu yapışkan veya dolgu macunları"),
            ("08.04.15", "Organik çözücüler içeren sıvı atık yapışkan ve dolgu macunları"),
            ("08.04.16", "08 04 15 dışındaki sıvı atık yapışkan ve dolgu macunları"),
            ("08.04.17", "Reçine yağı"),
            ("09.01.01", "Su bazlı banyo ve aktifleştirici solüsyonları"),
            ("09.01.02", "Su bazlı ofset plakası banyo solüsyonu"),
            ("09.01.03", "Çözücü bazlı banyo solüsyonları"),
            ("09.01.04", "Sabitleme solüsyonlar"),
            ("09.01.05", "Ağartma solüsyonları ve ağartıcı-sabitleme banyoları"),
            ("09.01.06", "Fotoğrafçılık atıklarının saha içi arıtılmasından oluşan gümüş içeren atıklar"),
            ("09.01.07", "Gümüş veya da gümüş bileşenleri içeren fotoğraf filmi ve kağıdı"),
            ("09.01.08", "Gümüş veya gümüş bileşenleri içermeyen fotoğraf filmi ve kağıdı"),
            ("09.01.10", "Pilsiz çalışan tek kullanımlık fotoğraf makineleri"),
            ("09.01.11", "16 01 06, 16 06 02 ve 16 06 03'ün altında geçen pillerle çalışan tek kullanımlık fotoğraf makineleri"),
            ("09.01.12", "09 01 11 dışındaki pille çalışan tek kullanımlık fotoğraf makineleri"),
            ("09.01.13", "16 06 01, 16 06 02 ve 16 06 03'ün altında geçen pilleri içeren sıvı atık fotoğraf makineleri"),
            ("10.01.07", "Sülfürik asit ve sülfüroz asit (emülsiyon ve solüsyonlar hariç)"),
            ("10.01.05", "Plastik yongalar ve çapaklar"),
            ("10.01.06", "Halojen içeren işleme emülsiyon ve solüsyonlar"),
            ("10.01.07", "Halojen içermeyen işleme emülsiyon ve solüsyonlar"),
            ("10.01.08", "Halojen içeren işleme emülsiyon ve solüsyonları"),
            ("10.01.09", "Halojen içermeyen işleme emülsiyon ve solüsyonları"),
            ("10.01.10", "Sentetik işleme yağları"),
            ("10.01.12", "Kullanılmış (mum) parafin ve yağlar"),
            ("10.01.14", "Tehlikeli maddeler içeren işleme çamurları"),
            ("10.01.15", "12 01 14 dışındaki işleme çamurları"),
            ("10.01.19", "Biyolojik olarak kolay bozunur işleme yağı"),
            ("10.03.17", "Anot üretiminden kaynaklanan katranlı atıklar"),
            ("10.03.18", "10 03 17 dışındaki anot üretiminden kaynaklanan karbon içerikli atıklar"),
            ("10.03.23", "Tehlikeli maddeler içeren gaz arıtımı katı atıkları"),
            ("10.03.27", "Soğutma suyunun arıtılmasından kaynaklanan yağ içerikli atıklar"),
            ("10.04.03", "Kalsiyum arsenat"),
            ("10.04.04", "Soğutma suyunun arıtılmasından kaynaklanan yağ içerikli atıklar"),
            ("10.05.05", "Soğutma suyunun arıtılmasından kaynaklanan yağ içerikli atıklar"),
            ("10.06.09", "Soğutma suyunun ıslahından kaynaklanan yağ içerikli atıklar"),
            ("10.06.10", "10 07 07 dışındaki soğutma suyu arıtımından kaynaklanan atıklar"),
            ("10.08.10", "Soğutma suyunun ıslahından kaynaklanan yağ içerikli atıklar"),
            ("10.08.12", "10 08 11 dışındaki soğutma suyu arıtımından kaynaklanan çamurlar"),
            ("10.08.19", "Soğu"),
            ("10.09.03", "Fırın harçları"),
            ("10.10.03", "Kalsiyum arsenat"),
            ("10.10.09", "Soğutma suyunun arıtılmasından kaynaklanan yağ içerikli atıklar"),
            ("10.10.10", "10 07 07 dışındaki soğutma suyu arıtımından kaynaklanan atıklar"),
            ("10.07.08", "10 07 07 dışındaki soğutma suyu arıtımından kaynaklanan atıklar"),
            ("10.08.10", "Soğutma suyunun ıslahından kaynaklanan yağ içerikli atıklar"),
            ("10.08.12", "10 08 11 dışındaki soğutma suyu arıtımından kaynaklanan çamurlar"),
            ("10.08.13", "Baca gazı arıtımından kaynaklanan çamurlar"),
            ("10.08.14", "Soğutma kolonlarından kaynaklanan atıklar"),
            ("10.08.15", "Kullanılmış filtre killeri"),
            ("10.08.17", "Bitüm"),
            ("10.09.01", "Su bazlı banyo ve aktifleştirici solüsyonları"),
            ("10.09.02", "Çözücü bazlı ofset plakası banyo solüsyonu"),
            ("10.09.03", "Çözücü bazlı banyo solüsyonları"),
            ("10.09.04", "Sabitleme solüsyonlar"),
            ("10.09.05", "Ağartma solüsyonları ve ağartıcı-sabitleme banyoları"),
            ("10.09.06", "Fotoğrafçılık atıklarının saha içi arıtılmasından oluşan gümüş içeren atıklar"),
            ("10.09.07", "Gümüş veya da gümüş bileşenleri içeren fotoğraf filmi ve kağıdı"),
            ("10.09.08", "Gümüş veya gümüş bileşenleri içermeyen fotoğraf filmi ve kağıdı"),
            ("10.09.10", "Pilsiz çalışan tek kullanımlık fotoğraf makineleri"),
            ("10.09.11", "16 06 01, 16 06 02 ve 16 06 03'ün altında geçen pillerle çalışan tek kullanımlık fotoğraf makineleri"),
            ("10.09.13", "16 06 01, 16 06 02 ve 16 06 03'ün altında geçen pilleri içeren sıvı atık fotoğraf makineleri"),
            ("10.09.14", "Tehlikeli maddeler içeren atık bağlayıcılar"),
            ("10.09.15", "10 09 14 dışındaki atık bağlayıcılar"),
            ("10.09.16", "10 09 15 dışındaki çatlak belirleme kimyasalları atığı"),
            ("10.10.14", "10 10 13 dışındaki bağlayıcı atıklar"),
            ("10.10.15", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("10.10.16", "10 10 15 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("10.11.03", "Sulu sıvı atıklar"),
            ("10.11.05", "Partiküller ve toz"),
            ("10.11.06", "Başka bir şekilde tanımlanmamış asitler"),
            ("10.11.07", "Sıyanıra atıkları (pamları, asitler)"),
            ("10.11.08", "Fosfatlama çamurları"),
            ("10.11.09", "Tetikli maddeler içeren çamurlar ve filtre kekleri"),
            ("10.11.10", "Tehlikeli maddeler içeren sulu durulama sıvıları"),
            ("10.11.11", "Tehlikeli maddeler içeren sulu durulama sıvıları"),
            ("10.11.12", "10 11 11 dışındaki atık camlar"),
            ("10.11.13", "Cam cila çamurları"),
            ("10.11.14", "11 01 13 dışındaki bağlayıcı atıklar"),
            ("10.11.15", "Saha içi atıksu arıtımından kaynaklanan tehlikeli maddeler içeren çamurlar"),
            ("10.11.16", "10 10 15 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("10.11.17", "Diğer atıklar (karışımlar dahil)"),
            ("10.11.18", "Fosforlu çamurları"),
            ("10.11.19", "Tetikli maddeler içeren çamurlar ve yarı katı haldeki yerini bulamayan"),
            ("10.11.20", "Kullanılmış (mum) parafin ve yağlar"),
            ("10.12.05", "Plastik yongalar ve çapaklar"),
            ("10.12.06", "Halojen içeren işleme emülsiyon ve solüsyonlar hariç"),
            ("10.12.07", "Halojen içermeyen madeni bazlı işleme yağları (emülsiyon ve solüsyonlar hariç)"),
            ("10.12.08", "Halojen içeren işleme emülsiyonları ve solüsyonları"),
            ("10.12.09", "Halojen içermeyen işleme emülsiyon ve solüsyonları"),
            ("10.12.10", "Sentetik işleme yağları"),
            ("10.12.11", "Kullanılmış (mum) parafin ve yağlar"),
            ("10.12.12", "Kullanılmış mumlar"),
            ("10.12.14", "Tehlikeli maddeler içeren işleme çamurları"),
            ("10.12.19", "Biyolojik olarak kolay bozunur işleme yağı"),
            ("10.03.10", "Mekanik ayırma sonucu oluşan elyaf iskartaları"),
            ("10.05.02", "Buhar yağ alma atıkları"),
            ("10.06.01", "PCB (2) içeren hidrolik yağlar"),
            ("10.13.01", "Anorganik bitki koruma ürünleri, ahşap koruma ürünleri ve diğer biositleri"),
            ("10.13.02", "Kullanılmış aktif karbon (06 07 02 hariç)"),
            ("10.13.03", "Karbon siyahı"),
            ("06.07.01", "Su bazlı yıkama sıvıları ve ana çözeltiler"),
            ("06.07.02", "Halojenli organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("06.07.03", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("06.07.04", "Diğer organik çözücüler, yıkama sıvıları ve ana çözeltiler"),
            ("06.09.02", "Zararlı silikonlar içeren atıklar"),
            ("06.09.03", "06 05 02 dışındaki saha içi atıksu arıtımından kaynaklanan çamurlar"),
            ("06.09.04", "Zararlı silikonlar içeren atıklar"),
            ("06.10.02", "Tehlikeli maddeler içeren atıklar"),
            ("10.01.05", "Plastik yongalar ve çapaklar"),
            ("10.01.07", "Halojen içermeyen işleme emülsiyon ve solüsyonlar"),
            ("10.01.09", "Halojen içermeyen işleme emülsiyon ve solüsyonları"),
            ("10.01.12", "Kullanılmış (mum) parafin ve yağlar"),
            ("10.01.18", "Sentetik işleme yağları"),
            ("10.01.19", "Biyolojik olarak kolay bozunur işleme yağı"),
            ("10.05.01", "Katı çinko"),
            ("10.11.11", "Tehlikeli maddeler içeren sulu durulama sıvıları"),
            ("10.11.13", "Cam cila çamurları"),
            ("10.11.14", "11 01 13 dışındaki cam cila çamurları"),
            ("10.11.15", "Saha içi atıksu arıtımından kaynaklanan katı atıklar")
        };
        
        
        // AJAX Validation Endpoint - HİBRİT YAKLAŞIM
        [HttpPost]
        public async Task<JsonResult> ValidateAtikKodu(string atikKodu)
        {
            if (string.IsNullOrWhiteSpace(atikKodu))
            {
                return Json(new { 
                    valid = false, 
                    message = "Atık kodu boş olamaz!" 
                });
            }

            // 1. SABİT LİSTEDE ARA
            var atik = sabitAtikKodlari.FirstOrDefault(a => a.Kod.Equals(atikKodu, StringComparison.OrdinalIgnoreCase));
            
            if (atik != default)
            {
                return Json(new { 
                    valid = true, 
                    atikAdi = atik.Ad,
                    kaynak = "Sistem Listesi" 
                });
            }

            // 2. MONGODB'DE ARA (Gelecekte özel kodlar için)
            
            return Json(new { 
                valid = false, 
                message = "Bu atık kodu sistemde bulunamadı!" 
            });
        }

        [HttpPost]
        public async Task<IActionResult> AtikKaydet(string atikKodu, string atikAdi, decimal miktar, string birim, string halTipi, string adres, IFormFile? gorsel)
        {
            try
            {
                var (kullaniciId, atikNoktasiId, _) = GetCurrentUser();

                string? gorselUrl = null;

                if (gorsel != null && gorsel.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(gorsel.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await gorsel.CopyToAsync(stream);
                    }

                    gorselUrl = "/uploads/" + uniqueFileName;
                }

                var atikKayit = new AtikKayit
                {
                    AtikKodu = atikKodu,
                    AtikAdi = atikAdi,
                    Miktar = miktar,
                    Birim = birim,
                    HalTipi = halTipi,
                    Adres = adres,
                    GorselUrl = gorselUrl,
                    GirisTarihi = DateTime.Now,
                    AtikNoktasiId = atikNoktasiId,
                    GirenKullaniciId = kullaniciId,
                    SevkEdildiMi = false
                };

                await _atikKayitService.CreateAsync(atikKayit);

                // TOPLAM MİKTARI HESAPLA
                var tumAktifAtiklar = await _atikKayitService.GetAktifAtiklar();
                var toplamMiktar = tumAktifAtiklar.Sum(k => k.MiktarTon);

                // Daha önce oluşturulan bildirimleri kontrol et
                var mevcutBildirimler = await _bildirimService.GetAllAsync();
                
                // ⭐ 10 TON KONTROLÜ
                if (toplamMiktar >= 10)
                {
                    var son10TonBildirimi = mevcutBildirimler
                        .Where(b => b.BildirimTipi == "10TonUyarisi")
                        .OrderByDescending(b => b.OlusturmaTarihi)
                        .FirstOrDefault();

                    bool yeni10TonBildirimi = false;
                    
                    if (son10TonBildirimi == null)
                    {
                        yeni10TonBildirimi = true;
                    }
                    else
                    {
                        var sonBildirimdenSonrakiAtiklar = tumAktifAtiklar
                            .Where(a => a.GirisTarihi > son10TonBildirimi.OlusturmaTarihi)
                            .ToList();
                        
                        if (sonBildirimdenSonrakiAtiklar.Any())
                        {
                            yeni10TonBildirimi = true;
                        }
                    }

                    if (yeni10TonBildirimi)
                    {
                        var bildirim = new Bildirim
                        {
                            Mesaj = $"🚨 KRİTİK! Toplam aktif atık miktarı {toplamMiktar:F2} tona ulaştı. Sevkiyat planlaması ZORUNLUDUR!",
                            OlusturmaTarihi = DateTime.Now,
                            Okundu = false,
                            ToplamMiktar = toplamMiktar,
                            BildirimTipi = "10TonUyarisi",
                            HedefKullaniciId = null 
                        };

                        await _bildirimService.CreateAsync(bildirim);
                    }
                }
                // ⭐ 5 TON KONTROLÜ (10 tondan önce)
                else if (toplamMiktar >= 5)
                {
                    var son5TonBildirimi = mevcutBildirimler
                        .Where(b => b.BildirimTipi == "5TonUyarisi")
                        .OrderByDescending(b => b.OlusturmaTarihi)
                        .FirstOrDefault();

                    bool yeni5TonBildirimi = false;
                    
                    if (son5TonBildirimi == null)
                    {
                        yeni5TonBildirimi = true;
                    }
                    else
                    {
                        var sonBildirimdenSonrakiAtiklar = tumAktifAtiklar
                            .Where(a => a.GirisTarihi > son5TonBildirimi.OlusturmaTarihi)
                            .ToList();
                        
                        if (sonBildirimdenSonrakiAtiklar.Any())
                        {
                            yeni5TonBildirimi = true;
                        }
                    }

                    if (yeni5TonBildirimi)
                    {
                        var bildirim = new Bildirim
                        {
                            Mesaj = $"⚠️ DİKKAT! Toplam aktif atık miktarı {toplamMiktar:F2} tona ulaştı. Sevkiyat planlaması yapmaya hazırlanın.",
                            OlusturmaTarihi = DateTime.Now,
                            Okundu = false,
                            ToplamMiktar = toplamMiktar,
                            BildirimTipi = "5TonUyarisi",
                            HedefKullaniciId = null 
                        };

                        await _bildirimService.CreateAsync(bildirim);
                    }
                }

                var kullaniciKayitlari = await _atikKayitService.GetByNoktaIdAsync(atikNoktasiId);
                var kullaniciToplamMiktar = kullaniciKayitlari.Where(k => !k.SevkEdildiMi).Sum(k => k.MiktarTon);

                return Json(new
                {
                    success = true,
                    message = "Atık başarıyla kaydedildi!",
                    toplamMiktar = kullaniciToplamMiktar,
                    uyari = kullaniciToplamMiktar >= 10 ? "DİKKAT: Toplam atık 10 tonu aştı!" : null
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
        public async Task<IActionResult> Panel()
        {
            var (kullaniciId, atikNoktasiId, firmaAdi) = GetCurrentUser();
            
            var kayitlar = await _atikKayitService.GetByNoktaIdAsync(atikNoktasiId);
            var toplamAktif = kayitlar.Where(k => !k.SevkEdildiMi).Sum(k => k.MiktarTon);
            
            // Kullanıcıya özel bildirimleri getir
            var tumBildirimler = await _bildirimService.GetAllAsync();
            var kullaniciBildirimleri = tumBildirimler
                .Where(b => b.HedefKullaniciId == kullaniciId || b.BildirimTipi == "SevkiyatBildirimi")
                .OrderByDescending(b => b.OlusturmaTarihi)
                .Take(5)
                .ToList();
            
            ViewBag.FirmaAdi = firmaAdi;
            ViewBag.ToplamAktifAtik = toplamAktif;
            ViewBag.Bildirimler = kullaniciBildirimleri;
            
            return View(kayitlar.OrderByDescending(k => k.GirisTarihi).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> AtikSil(string id)
        {
            try
            {
                // Güvenlik kontrolü: Silinecek atık bu kullanıcıya ait mi?
                var (_, atikNoktasiId, _) = GetCurrentUser();
                
                var atik = await _atikKayitService.GetByIdAsync(id);
                
                if (atik == null)
                {
                    return Json(new { success = false, message = "Atık bulunamadı!" });
                }
                
                // Eğer atık bu kullanıcıya ait değilse silme izni verme
                if (atik.AtikNoktasiId != atikNoktasiId)
                {
                    return Json(new { success = false, message = "Bu atığı silme yetkiniz yok!" });
                }

                await _atikKayitService.DeleteAsync(id);

                // ⭐ SİLME SONRASI BİLDİRİM KONTROLÜ
                var tumAktifAtiklar = await _atikKayitService.GetAktifAtiklar();
                var toplamMiktar = tumAktifAtiklar.Sum(k => k.MiktarTon);

                // Eğer miktar 10 tonun altına düştüyse ve 10 ton bildirimi varsa, bilgi bildirimi oluştur
                if (toplamMiktar < 10)
                {
                    var mevcutBildirimler = await _bildirimService.GetAllAsync();
                    var aktif10TonBildirimi = mevcutBildirimler
                        .Where(b => b.BildirimTipi == "10TonUyarisi" && !b.Okundu)
                        .OrderByDescending(b => b.OlusturmaTarihi)
                        .FirstOrDefault();

                    if (aktif10TonBildirimi != null)
                    {
                        // Okunmamış 10 ton bildirimini okundu yap
                        await _bildirimService.BildirimOkunduIsaretle(aktif10TonBildirimi.Id);

                        // Yeni bilgi bildirimi oluştur
                        var bilgiBildirimi = new Bildirim
                        {
                            Mesaj = $"✅ BİLGİ: Atık silme işlemi sonrası toplam aktif atık {toplamMiktar:F2} tona düştü. Sevkiyat aciliyeti azaldı.",
                            OlusturmaTarihi = DateTime.Now,
                            Okundu = false,
                            ToplamMiktar = toplamMiktar,
                            BildirimTipi = "BilgiMesaji",
                            HedefKullaniciId = null 
                        };

                        await _bildirimService.CreateAsync(bilgiBildirimi);
                    }
                }

                // Eğer 5 tonun altına düştüyse ve 5 ton bildirimi varsa
                if (toplamMiktar < 5)
                {
                    var mevcutBildirimler = await _bildirimService.GetAllAsync();
                    var aktif5TonBildirimi = mevcutBildirimler
                        .Where(b => b.BildirimTipi == "5TonUyarisi" && !b.Okundu)
                        .OrderByDescending(b => b.OlusturmaTarihi)
                        .FirstOrDefault();

                    if (aktif5TonBildirimi != null)
                    {
                        // Okunmamış 5 ton bildirimini okundu yap
                        await _bildirimService.BildirimOkunduIsaretle(aktif5TonBildirimi.Id);
                    }
                }

                return Json(new { success = true, message = "Atık başarıyla silindi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Bildirim sil
        [HttpPost]
        public async Task<JsonResult> BildirimSil(string id)
        {
            try
            {
                var (kullaniciId, _, _) = GetCurrentUser();
                
                // Güvenlik kontrolü: Bildirim bu kullanıcıya ait mi?
                var bildirim = await _bildirimService.GetByIdAsync(id);
                
                if (bildirim == null)
                {
                    return Json(new { success = false, message = "Bildirim bulunamadı!" });
                }
                
                if (bildirim.HedefKullaniciId != kullaniciId)
                {
                    return Json(new { success = false, message = "Bu bildirimi silme yetkiniz yok!" });
                }
                
                await _bildirimService.DeleteAsync(id);
                return Json(new { success = true, message = "Bildirim silindi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Tüm bildirimleri temizle
        [HttpPost]
        public async Task<JsonResult> TumBildirimleriTemizle()
        {
            try
            {
                var (kullaniciId, _, _) = GetCurrentUser();
                
                var tumBildirimler = await _bildirimService.GetAllAsync();
                var kullaniciBildirimleri = tumBildirimler.Where(b => b.HedefKullaniciId == kullaniciId).ToList();
                
                foreach (var bildirim in kullaniciBildirimleri)
                {
                    await _bildirimService.DeleteAsync(bildirim.Id);
                }
                
                return Json(new { success = true, message = $"{kullaniciBildirimleri.Count} bildirim temizlendi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
