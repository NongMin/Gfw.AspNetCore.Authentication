using System;
using System.Collections.Generic;
using System.Text;

namespace Gfw.AspNetCore.Authentication.WeChat
{
    /// <summary>
    /// 当微信API返回错误信息时抛出此异常
    /// </summary>
    public class WeChatException : Exception
    {
        public WeChatException(string message) : base(message)
        {
        }
    }
}
