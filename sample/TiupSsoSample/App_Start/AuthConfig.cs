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
            var client = new TiupSso("yourClientId", "yourClientSecret");
            var extraData = new Dictionary<string, object>();
            OAuthWebSecurity.RegisterClient(client, "Tiup.cn", extraData);
        }
    }
}
