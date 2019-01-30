using Gfw.AspNetCore.Authentication.WeChatClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Gfw.AspNetCore.Authentication.WeChat.Client
{
    public class HandleResponseContext 
    {
        public HandleResponseContext(HttpContext httpContext, AuthenticationScheme scheme
            , WeChatClientOptions options, ClaimsPrincipal claimsPrincipal)
        {
            this.HttpContext = httpContext;
            this.Scheme = scheme;
            this.Options = options;
            this.ClaimsPrincipal = claimsPrincipal; 
        }

        public HttpContext HttpContext { get; }
        public AuthenticationScheme Scheme { get; }
        public WeChatClientOptions Options { get; }
        public ClaimsPrincipal ClaimsPrincipal { get; }
    }
}
