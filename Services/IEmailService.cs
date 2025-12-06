using AtikProj.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace AtikProj.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendBildirimEmailAsync(string toEmail, string firmaAdi, Bildirim bildirim);
    }
}