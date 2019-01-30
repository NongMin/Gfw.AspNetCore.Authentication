using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using Gfw.AspNetCore.Authentication.WeChat.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Gfw.AspNetCore.Authentication.WeChat
{
    public class WeChatOptions:  OAuthOptions
    {
        public WeChatOptions()
        {
            CallbackPath = new PathString("/signin-wechat");
            AuthorizationEndpoint = WeChatDefaults.AuthorizationEndpoint;
            TokenEndpoint = WeChatDefaults.TokenEndpoint;
            UserInformationEndpoint = WeChatDefaults.UserInformationEndpoint;
            Scope.Add(WeChatScopes.snsapi_userinfo);

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "openid");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "openid"); 
            ClaimActions.MapJsonKey(WeChatClaimTypes.UnionId, "unionid");
            ClaimActions.MapJsonKey(WeChatClaimTypes.OpenId, "openid");
        }
        
        /// <summary>
        /// “返回国家地区语言版本，zh_CN 简体，zh_TW 繁体，en 英语”
        /// </summary>
        public string Language { get; set; } = WeChatLanguages.zh_CN;

        /// <summary>
        /// 是否强制认证，当用户已认证时，是否仍允许其调用接口重复认证，默认false。
        /// 将此属性设为false可防止微信APP重复请求。
        /// </summary>
        public bool ForceAuthentication { get; set; } = false;


        public override void Validate()
        {
            base.Validate();

            if (Scope.Count > 1)
            {
                throw new ArgumentException($"微信API不支持同时指定多个{nameof(Scope)}", nameof(Scope));
            }
        }



        /// <summary>
        /// 设置<see cref="Scope"/>，并根据所设置的<see cref="Scope"/>值修改相应的<see cref="AuthorizationEndpoint"/>。
        /// </summary>
        /// <param name="scope"><see cref="WeChatScopes"/>的常量值之一</param>
        public void ChangeScope(string scope)
        {
            switch (scope)
            {
                case WeChatScopes.snsapi_base:
                case WeChatScopes.snsapi_userinfo:
                    Scope.Clear();
                    Scope.Add(scope);
                    AuthorizationEndpoint = WeChatDefaults.AuthorizationEndpoint; 
                    break;
                case WeChatScopes.snsapi_login:
                    Scope.Clear();
                    Scope.Add(scope);
                    AuthorizationEndpoint = WeChatDefaults.ScopeLoginAuthorizationEndpoint;
                    break;
                default:
                    throw new ArgumentException(nameof(Scope));
            }
        }
    }
}
