using AtikProj.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace AtikProj.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"✅ Email gönderildi: {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email gönderme hatası: {ex.Message}");
                throw;
            }
        }

        public async Task SendBildirimEmailAsync(string toEmail, string firmaAdi, Bildirim bildirim)
        {
            string subject = bildirim.BildirimTipi switch
            {
                "10TonUyarisi" => "🚨 KRİTİK! Atık Sevkiyatı Gerekli",
                "5TonUyarisi" => "⚠️ DİKKAT! Atık Miktarı Artıyor",
                "SevkiyatBildirimi" => "🚚 Sevkiyat Planlandı",
                "BilgiMesaji" => "ℹ️ Bilgilendirme",
                _ => "📧 Bildirim"
            };

            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
                        .container {{ background: white; padding: 30px; border-radius: 10px; max-width: 600px; margin: 0 auto; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }}
                        .content {{ margin-top: 20px; line-height: 1.6; }}
                        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 12px; }}
                        .miktar {{ background: #fef3c7; padding: 15px; border-left: 4px solid #f59e0b; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Huş Mühendislik - Atık Yönetim Sistemi</h2>
                        </div>
                        <div class='content'>
                            <p><strong>Sayın {firmaAdi},</strong></p>
                            <p>{bildirim.Mesaj.Replace("\n", "<br>")}</p>
                            
                            {(bildirim.ToplamMiktar > 0 ? $@"
                            <div class='miktar'>
                                <strong>📊 Toplam Atık Miktarı:</strong> {bildirim.ToplamMiktar:F2} ton
                            </div>" : "")}
                            
                            <p>Detaylı bilgi için lütfen sisteme giriş yapınız.</p>
                        </div>
                        <div class='footer'>
                            <p>Bu bir otomatik bildirimdir, lütfen yanıtlamayınız.</p>
                            <p>&copy; 2024 Huş Mühendislik</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}