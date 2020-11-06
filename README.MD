### Asp.Net Core Gfw 微信 OAuth2.0中间件
* 支持为普通开发者开放的接口（scope：snsapi_login）
* 支持为公众号开放的接口（scope：snsapi_base、snsapi_userinfo）
* 支持客户端使用从微信获取的code登录。

#### 使用方法

Startup.cs（详情及注意事项请查看示例）

```csharp

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

#### 最佳实践

无法通过授权的原因大都是Cookie造成的，需要根据具体情况对Cookie进行配置。如：某些版本的MVC默认Cookie参数会改变造成授权失败，此时需要设置

```csharp
options.CorrelationCookie.SameSite =  SameSiteMode.Lax;
```
还有一种办法，就是直接重写RemoteAuthenticationHandler的下面两个方法，可牺牲部分安全换取解决大部分微信内置浏览器兼容问题
```csharp
protected override void GenerateCorrelationId(AuthenticationProperties properties)
{
}

protected override bool ValidateCorrelationId(AuthenticationProperties properties)
{
    return true;
}
```

#### 1.xx 升级到 2.00注意事项

* ClaimTypes.NameIdentifier和ClaimTypes.Name现在默认的映射属性是openid
* 不再依赖基类库

#### 2.xx 升级到 3.00注意事项

* 3.0版本仅支持.Net Core 3.0，.Net Core 3.0以下版本请使用旧版