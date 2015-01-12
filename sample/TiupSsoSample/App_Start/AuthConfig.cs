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
            var client = new TiupSso("cn.tiup.ruc", "15d00623a33b1396d66042ec1dd581b71ec3ed68ce6099afcb0217857a375d17", extraData, new[] { "all" });
            OAuthWebSecurity.RegisterClient(client, "Tiup.cn", null);
        }
    }
}
