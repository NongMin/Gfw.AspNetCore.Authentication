
using Gfw.AspNetCore.Authentication.WeChat.Client;
using Gfw.AspNetCore.Authentication.WeChatClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Gfw.AspNetCore.Authentication.WeChat
{
    public static class WeChatExtensions
    {
        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder)
            => builder.AddWeChat(WeChatDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder, Action<WeChatOptions> configureOptions)
            => builder.AddWeChat(WeChatDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder, string authenticationScheme, Action<WeChatOptions> configureOptions)
            => builder.AddWeChat(authenticationScheme, WeChatDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddWeChat(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeChatOptions> configureOptions)
            => builder.AddOAuth<WeChatOptions, WeChatHandler>(authenticationScheme, displayName, configureOptions);

        //region WeChatClient
        public static AuthenticationBuilder AddWeChatClient(this AuthenticationBuilder builder)
            => builder.AddWeChatClient(WeChatClientDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddWeChatClient(this AuthenticationBuilder builder, Action<WeChatClientOptions> configureOptions)
            => builder.AddWeChatClient(WeChatClientDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddWeChatClient(this AuthenticationBuilder builder, string authenticationScheme, Action<WeChatClientOptions> configureOptions)
            => builder.AddWeChatClient(authenticationScheme, WeChatClientDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddWeChatClient(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeChatClientOptions> configureOptions)
            => builder.AddOAuth<WeChatClientOptions, WeChatClientHandler>(authenticationScheme, displayName, configureOptions);
        //endregion
    }
}
