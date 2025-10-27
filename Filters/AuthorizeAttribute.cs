using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AtikProj.Filters
{
    // Genel Authorization - Tüm kullanıcılar için login kontrolü
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var kullaniciId = context.HttpContext.Session.GetString("KullaniciId");

            if (string.IsNullOrEmpty(kullaniciId))
            {
                // Login olmamış, login sayfasına yönlendir
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }

    // Admin Authorization - Sadece Admin rolü için
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var kullaniciId = context.HttpContext.Session.GetString("KullaniciId");
            var rol = context.HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(kullaniciId))
            {
                // Login olmamış
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            else if (rol != "Admin")
            {
                // Login olmuş ama Admin değil - yetkisiz erişim
                context.Result = new ContentResult
                {
                    Content = "Bu sayfaya erişim yetkiniz yok!",
                    StatusCode = 403
                };
            }

            base.OnActionExecuting(context);
        }
    }
}