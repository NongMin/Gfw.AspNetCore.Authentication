### Asp.Net Core Gfw 微信 OAuth2.0中间件
* 支持为普通开发者开放的接口（scope：snsapi_login）
* 支持为公众号开放的接口（scope：snsapi_base、snsapi_userinfo）
* 支持客户端使用从微信获取的code登录。

#### 使用方法

Startup.cs（详情及注意事项请查看示例）

```cs

services.AddAuthentication()
.AddWeChat(AuthenticationScheme_WeChat, options =>
           {
               options.ClientId = "";
               options.ClientSecret = ""; 
           })
.AddWeChat(AuthenticationScheme_WeChatBase, options =>
           {
               options.ClientId = "";
               options.ClientSecret = "";
           })
.AddWeChatClient(options =>
           {
               options.ClientId = "";
               options.ClientSecret = ""; 
           });

 ```

#### 1.xx 升级到 2.00注意事项

* ClaimTypes.NameIdentifier和ClaimTypes.Name现在默认的映射属性是openid
* 不再依赖基类库