namespace AtikProj.Models
{
    public class SevkiyatPlanlaViewModel
    {
        public List<AtikNoktaDetay> AtikNoktaGruplari { get; set; } = new List<AtikNoktaDetay>();
        public decimal ToplamAktifAtik { get; set; }
    }
}