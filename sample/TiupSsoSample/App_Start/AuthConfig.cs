using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using TiupSsoSample.Models;
using DotNetOpenAuth;
using DotNetOpenAuth.TiupSso;

namespace TiupSsoSample
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            var extraData = new Dictionary<string, string>();
            //input your extra data
            extraData["school_code"] = "ruc";
            extraData["theme"] = "schools";
            extraData["sso"] = "true";
            //input your appId and  app secret
            var client = new TiupSso("Your AppId", "You App Secret", extraData, new[] { "all" });
            OAuthWebSecurity.RegisterClient(client, "Tiup.cn", null);
        }
    }
}
