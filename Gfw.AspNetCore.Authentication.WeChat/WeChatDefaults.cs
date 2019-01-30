using System;

namespace Gfw.AspNetCore.Authentication.WeChat
{
    public class WeChatDefaults
    {
        public const string AuthenticationScheme = "WeChat";

        public static readonly string DisplayName = "微信";

        public static readonly string AuthorizationEndpoint = "https://open.weixin.qq.com/connect/oauth2/authorize";

        public static readonly string TokenEndpoint = "https://api.weixin.qq.com/sns/oauth2/access_token";

        public static readonly string UserInformationEndpoint = "https://api.weixin.qq.com/sns/userinfo";

        public static readonly string ScopeLoginAuthorizationEndpoint = "https://open.weixin.qq.com/connect/qrconnect";

    }
}
