using Microsoft.AspNetCore.Http;

namespace Gfw.AspNetCore.Authentication.WeChat.Client
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
