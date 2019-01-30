using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Main.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExternalLogin(string provider)
        {
            string callbackUrl = Url.Action(nameof(ExternalLoginCallback), "Home" ,new { provider }, Request.Scheme);

            
            return new ChallengeResult(provider, new AuthenticationProperties()
            {
                RedirectUri = callbackUrl
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string provider)
        {
            if (string.IsNullOrEmpty(provider)) return BadRequest();

            var authenticateResult = await HttpContext.AuthenticateAsync(provider);
             
            if (!authenticateResult.Succeeded)
            {
                return Content("登录失败");
            }

            string name = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);


            return Content(name);
        }
    }
}
