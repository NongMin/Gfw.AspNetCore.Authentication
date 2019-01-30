using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Gfw.AspNetCore.Authentication.WeChat.Models;
using Microsoft.AspNetCore.Authentication;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using Microsoft.Net.Http.Headers;

namespace Gfw.AspNetCore.Authentication.WeChat
{
    public abstract class WeChatHandlerBase<T> : OAuthHandler<T>
        where T : WeChatOptions, new()
    {
        public WeChatHandlerBase(IOptionsMonitor<T> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }


        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUri)
        {
            var tokenRequestParameters = new Dictionary<string, string>()
            {
                { "appid", Options.ClientId },
                { "secret", Options.ClientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
            };

            var requestContent = new FormUrlEncodedContent(tokenRequestParameters);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = requestContent;
            var response = await Backchannel.SendAsync(requestMessage, Context.RequestAborted);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var payload = JObject.Parse(content);
                var errcode = payload.Value<string>("errcode");
                if (errcode != null)
                {
                    var weChatException = new WeChatException("使用code换取access_token失败", errcode, payload.Value<string>("errmsg"));

                    Logger.LogDebug(weChatException, "WeChatHandler ExchangeCodeAsync");

                    return OAuthTokenResponse.Failed(weChatException);
                }

                return OAuthTokenResponse.Success(payload);
            }
            else
            {
                var errorMessage = "使用code换取access_token失败：HttpClient返回错误代码，" +
                    $"Status：{response.StatusCode}，Headers：{response.Headers.ToString()}，Body：{content}";

                var ex = new Exception(errorMessage);

                Logger.LogDebug(ex, "WeChatHandler ExchangeCodeAsync");

                return OAuthTokenResponse.Failed(ex);
            }
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            OAuthCreatingTicketContext context = null;

            if (Options.Scope.First() == WeChatScopes.snsapi_userinfo)
            {
                var url = Options.UserInformationEndpoint +
                    $"?access_token={UrlEncoder.Default.Encode(tokens.AccessToken)}" +
                    $"&openid={UrlEncoder.Default.Encode(tokens.Response.Value<string>("openid"))}" +
                    $"&lang=" + Options.Language;

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await Backchannel.SendAsync(request, Context.RequestAborted);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var payload = JObject.Parse(content);

                    context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload);

                    context.RunClaimActions(payload);
                }
                else
                {
                    var weChatException = new WeChatException("获取微信用户信息失败：HttpClient返回错误代码，" +
                    $"Status：{response.StatusCode}，Headers：{response.Headers.ToString()}，Body：{content}");

                    Logger.LogDebug(weChatException, "WeChatHandler CreateTicketAsync");


                    throw weChatException;
                }
            }

            if (context == null)
            {
                context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, tokens.Response);

                context.RunClaimActions(tokens.Response);
            }

            await Events.CreatingTicket(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }
    }
}
