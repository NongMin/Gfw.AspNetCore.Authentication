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
    public class WeChatHandler : WeChatHandlerBase<WeChatOptions>
    {
        //private const string CorrelationProperty = ".xsrf"; 

        private static readonly string CorrelationProperty =
            (string)typeof(RemoteAuthenticationHandler<>)
            .GetField(nameof(CorrelationProperty), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .GetRawConstantValue();

        public WeChatHandler(IOptionsMonitor<WeChatOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        public override Task<bool> ShouldHandleRequestAsync()
        {
            //如果不是公从号接口则不使用PathState，下同
            if (Options.Scope.First() != WeChatScopes.snsapi_login)
            {
                return Task.FromResult(Request.Path.StartsWithSegments(Options.CallbackPath));
            }
            else
            {
                return Task.FromResult(Options.CallbackPath == Request.Path);
            }
        }

        /// <summary>
        /// 将state数据（已编码）存放在以斜杠分隔的路径中
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual string ToPathState(string state)
        {
            if (string.IsNullOrEmpty(state)) return state;
            int segmentLength = 64;
            if (state.Length <= segmentLength) return state;

            int index = segmentLength;
            var sb = new StringBuilder(state, state.Length + state.Length / segmentLength);

            while (index < sb.Length - 1)
            {
                sb.Insert(index, '/');
                index++;
                index += segmentLength;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 从PathState中提取State数据
        /// </summary>
        /// <returns></returns>
        protected virtual string GetStateFromPathState(string pathState)
        {
            if (pathState == null) throw new ArgumentNullException(nameof(pathState));

            return pathState.Substring(Options.CallbackPath.Value.Length + 1).Replace("/", string.Empty);
        }

        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (redirectUri == null) throw new ArgumentNullException(nameof(redirectUri));

            var scope = Options.Scope.First();
            string url;
            if (scope != WeChatScopes.snsapi_login)
            {
                var realState = Options.StateDataFormat.Protect(properties);
                redirectUri += "/" + ToPathState(UrlEncoder.Default.Encode(realState));

                url = Options.AuthorizationEndpoint +
                    $"?appid={Options.ClientId}" +
                    $"&redirect_uri={UrlEncoder.Default.Encode(redirectUri)}" +
                    $"&response_type=code&scope={scope}&connect_redirect=1#wechat_redirect";
            }
            else
            {
                var realState = Options.StateDataFormat.Protect(properties);

                url = Options.AuthorizationEndpoint +
                     $"?appid={Options.ClientId}" +
                     $"&redirect_uri={UrlEncoder.Default.Encode(redirectUri)}" +
                     $"&response_type=code&scope={scope}&state={UrlEncoder.Default.Encode(realState)}&connect_redirect=1#wechat_redirect";
            }



            Logger.LogDebug("WeChatHandler BuildChallengeUrl:" + url);

            return url;
        }
        
        protected async override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            bool is_snsapi_login = Options.Scope.First() == WeChatScopes.snsapi_login;
            string state;
            if (is_snsapi_login)
            {
                state = Request.Query["state"];
            }
            else
            {
                //还原state
                state = GetStateFromPathState(Request.Path);
            }

            if (!Options.ForceAuthentication)
            {
                var authenticateResult = await Context.AuthenticateAsync(SignInScheme);
                if (authenticateResult?.Ticket != null)
                {
                    Logger.LogInformation("用户已认证，将跳过认证流程。");

                    //如果当前请求用户已认证，则定向到RedirectUri，或返回Ticket
                    var successResult = HandleRequestResult.Success(authenticateResult.Ticket);

                    var properties = Options.StateDataFormat.Unprotect(state);
                     
                    if (!Context.Response.HasStarted && !string.IsNullOrEmpty(properties?.RedirectUri))
                    {
                        Response.Redirect(properties.RedirectUri);
                        return HandleRequestResult.Handle();
                    }

                    return HandleRequestResult.Success(authenticateResult.Ticket);
                }
            }


            if (is_snsapi_login)
            {
                return await base.HandleRemoteAuthenticateAsync();
            }
            else
            {
                #region 标准化state参数以便使用官方库逻辑

                Dictionary<string, Microsoft.Extensions.Primitives.StringValues> dic;

                if (Request.Query.Count > 0)
                {
                    dic = Request.Query.Where(x => x.Key != "state").ToDictionary(x => x.Key, y => y.Value);
                }
                else
                {
                    dic = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(1);
                }

                dic.Add("state", state);
                var fadeQuery = new QueryCollection(dic);

                var rawQuery = Request.Query;

                Request.Query = fadeQuery;

                var handleRequestResult = await base.HandleRemoteAuthenticateAsync();

                Request.Query = rawQuery;

                #endregion

                return handleRequestResult;
            }
        }
    }
}
