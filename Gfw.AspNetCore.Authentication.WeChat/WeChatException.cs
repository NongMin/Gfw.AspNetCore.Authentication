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

        public WeChatException(string message, string errcode, string errmsg) : base(message)
        {
            this.errcode = errcode;
            this.errmsg = errmsg;
        }

        public string errcode { get;private set; }

        public string errmsg { get; private set; }
    }
}
