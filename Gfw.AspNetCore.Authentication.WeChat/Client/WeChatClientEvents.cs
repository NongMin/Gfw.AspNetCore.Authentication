using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Gfw.AspNetCore.Authentication.WeChat.Client
{
    public class WeChatClientEvents : OAuthEvents
    {
        public async override Task TicketReceived(TicketReceivedContext context)
        {
            //如果不调用HandleResponse，默认的行为是重定向到RedirectUri属性指定的地址。
            //在这里不需要定向
            var handleResponseContext = new HandleResponseContext(context.HttpContext
                , context.Scheme
                , context.Options as WeChatClientOptions
                , context.Principal);
            await HandleResponse(handleResponseContext);
            context.HandleResponse();

            await base.TicketReceived(context);
        }


        public Func<HandleResponseContext, Task> OnHandleResponse { get; set; } = context =>
        {
            context.HttpContext.Response.StatusCode = 204;
            return Task.CompletedTask;
        };


        public virtual Task HandleResponse(HandleResponseContext context) => OnHandleResponse(context);

        public Func<HttpContext, Task<string>> OnGetCode { get; set; } = context =>
        {
            return Task.FromResult((string)context.Request.Query["code"]);
        };

        public virtual Task<string> GetCode(HttpContext context) => OnGetCode(context);
    }
}
