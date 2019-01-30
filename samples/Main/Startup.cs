using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Gfw.AspNetCore.Authentication.WeChat;
using Gfw.AspNetCore.Authentication.WeChat.Client;
using Gfw.AspNetCore.Authentication.WeChat.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Main
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthentication("ApplicationCookie")
               .AddCookie("ApplicationCookie", options =>
               {
                   options.AccessDeniedPath = "/";
                   options.LoginPath = "/";
               })

            #region 


           //scope=snsapi_userinfo，一般公众号都会有此权限
           .AddWeChat(AuthenticationScheme_WeChat, options =>
           {
               options.ClientId = "ClientId";
               options.ClientSecret = "ClientSecret";

               options.Events.OnCreatingTicket = OnWeChatCreatingTicketHandler;
               options.Events.OnRemoteFailure = OnRemoteFailureHandler;

               MapWeChatJsonKey(options.ClaimActions);
           })
           .AddCookie("WeChatCookie.WeChat")


           .AddWeChat(AuthenticationScheme_WeChatBase, options =>
           {
               options.ClientId = "ClientId";
               options.ClientSecret = "ClientSecret";
               options.ChangeScope(WeChatScopes.snsapi_base); //注意：应调用此方法修改
           })
           .AddCookie("WeChatCookie.WeChatBase")


           .AddWeChatClient(options =>
           {
               options.ClientId = "ClientId";
               options.ClientSecret = "ClientSecret";

               options.Events.OnCreatingTicket = OnWeChatCreatingTicketHandler;
               options.Events.OnRemoteFailure = OnRemoteFailureHandler;

               MapWeChatJsonKey(options.ClaimActions);

               //路径
               options.CallbackPath = new PathString("/api/wechat/code");
               options.Events.OnHandleResponse = ExchangeWeChatCodeHandler;
               options.Events.OnGetCode = GetCodeHandler;
           });

            #endregion
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        #region 配置微信登录，为了简单起见全部放在这里

        public const string AuthenticationScheme_WeChat = "WeChat";
        public const string AuthenticationScheme_WeChatBase = "WeChatBase";

        /// <summary>
        /// 将微信返回的JSON属性映射到<see cref="ClaimsPrincipal"/>
        /// 默认值：
        /// ClaimTypes.Name : openid
        /// ClaimTypes.NameIdentifier : openid。
        /// 这里假设我们已绑定开发者账号，微信返回的JSON含有unionid属性。
        /// </summary>
        /// <param name="claimActions"></param>
        private void MapWeChatJsonKey(ClaimActionCollection claimActions)
        {
            claimActions.Clear();
            claimActions.MapJsonKey(ClaimTypes.Name, "unionid");
            claimActions.MapJsonKey(ClaimTypes.NameIdentifier, "unionid");
            claimActions.MapJsonKey(WeChatClaimTypes.UnionId, "unionid");
            claimActions.MapJsonKey(WeChatClaimTypes.OpenId, "openid");
        }



        /// <summary>
        /// 微信票据被创建事件处理程序
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task OnWeChatCreatingTicketHandler(OAuthCreatingTicketContext oAuthCreatingTicketContext)
        {
            //示例
            string unionid = oAuthCreatingTicketContext.Principal.FindFirstValue(WeChatClaimTypes.UnionId);
            string openid = oAuthCreatingTicketContext.Principal.FindFirstValue(WeChatClaimTypes.OpenId);

            string json = oAuthCreatingTicketContext.User.ToString();

            return Task.CompletedTask;
        }


        /// <summary>
        /// 远程服务器（如授权失败时）错误处理程序。
        /// </summary>
        /// <param name="remoteFailureContext"></param>
        /// <returns></returns>
        private Task OnRemoteFailureHandler(RemoteFailureContext remoteFailureContext)
        {
            remoteFailureContext.HandleResponse();

            if (!remoteFailureContext.HttpContext.Response.HasStarted)
            {
                //TODO 写入日志
                //TODO 向HttpContext.Response写入友好的错误提示信息展示给用户
            }
            return Task.CompletedTask;
        }



        /// <summary>
        /// 从用户请求上下文中获取code
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private Task<string> GetCodeHandler(HttpContext httpContext)
        {
            //假设code是放在查询字符串中时的获取方式
            string code = httpContext.Request.Query["code"]; //这么做显然是不安全的，客户端应该将code通过POST方式发送给服务器

            return Task.FromResult(code);
        }


        private async Task ExchangeWeChatCodeHandler(HandleResponseContext handleResponseContext)
        {
            var user = handleResponseContext.ClaimsPrincipal;

            string unionid = user.FindFirstValue(WeChatClaimTypes.UnionId);
            string openid = user.FindFirstValue(WeChatClaimTypes.OpenId);

            if (!handleResponseContext.HttpContext.Response.HasStarted)
            {
                //TODO 向客户端返回响应数据
                await handleResponseContext.HttpContext.Response.WriteAsync("openid:" + openid);
            }
             
        }

        #endregion
    }
}
