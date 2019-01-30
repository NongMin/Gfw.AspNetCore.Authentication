using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using Gfw.AspNetCore.Authentication.WeChat.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Gfw.AspNetCore.Authentication.WeChat;
using Gfw.AspNetCore.Authentication.WeChat.Client;

namespace Gfw.AspNetCore.Authentication.WeChatClient
{
    public class WeChatClientOptions : WeChatOptions
    {
        public WeChatClientOptions()
        {
            CallbackPath = new PathString("/signin-wechatclient");
            Events = new WeChatClientEvents();
        }


        public new WeChatClientEvents Events
        {
            get { return (WeChatClientEvents)base.Events; }
            set { base.Events = value; }
        }
    }
}
