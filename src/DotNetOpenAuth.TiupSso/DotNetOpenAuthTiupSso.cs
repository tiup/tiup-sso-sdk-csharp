using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json;

namespace DotNetOpenAuth.TiupSso
{
    public class TiupSso : OAuth2Client
    {
        #region Constants and Fields
        private const string AuthorizationEndpoint = "https://uc.tiup.cn/oauth/authorize";
        private const string TokenEndpoint = "http://uc.tiup.cn:18080/oauth/token";
        private const string UserInfoEndpoint = "http://uc.tiup.cn:18080/api/users/me";

        private readonly string _appId;
        private readonly string _appSecret;
        private readonly string[] _requestedScopes;
        private readonly string _schoolCode;
        private readonly string _theme;
        private readonly string _sso;
        #endregion

        public TiupSso(string appId, string appSecret)
            : this(appId, appSecret, null, new[] { "all" }) { }

        public TiupSso(string appId, string appSecret, Dictionary<string, string> extraData, params string[] requestedScopes)
            : base("tiup")
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentNullException("appId");

            if (string.IsNullOrWhiteSpace(appSecret))
                throw new ArgumentNullException("appSecret");

            if (requestedScopes == null)
                throw new ArgumentNullException("requestedScopes");

            if (requestedScopes.Length == 0)
                throw new ArgumentException("One or more scopes must be requested.", "requestedScopes");

            _appId = appId;
            _appSecret = appSecret;
            if (extraData.ContainsKey("school_code")) _schoolCode = extraData["school_code"];
            if (extraData.ContainsKey("theme")) _theme = extraData["theme"];
            if (extraData.ContainsKey("sso")) _sso = extraData["sso"];
            _requestedScopes = requestedScopes;

        }


        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var state = string.IsNullOrEmpty(returnUrl.Query) ? string.Empty : returnUrl.Query.Substring(1);
            //加入空白?&, 防止服务器段没有做 ?/& 区分处理
            var redirectUri = returnUrl.GetLeftPart(UriPartial.Path) + "?target=_blank";
            return BuildUri(AuthorizationEndpoint, new NameValueCollection
                {
                    { "client_id", _appId },
                    { "scope", string.Join(" ", _requestedScopes) },
                    { "redirect_uri", redirectUri },
                    { "state", state },
                    { "response_type", "code" },
                    { "theme", _theme },
                    { "school_code", _schoolCode },
                    { "sso", _sso },
                });
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            //获取用户信息
            var uri = BuildUri(UserInfoEndpoint, new NameValueCollection { { "access_token", accessToken } });
            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            return null;
                        StreamReader streamReader = new StreamReader(responseStream);
                        var responseString = streamReader.ReadToEnd();
                        dynamic response = JsonConvert.DeserializeObject<dynamic>(responseString);
                        try
                        {
                            var email = (string)response.data.email;
                            var values = new Dictionary<string, string>();
                            values.Add("username", email);
                            values.Add("email", email);
                            values.Add("id", (string)response.data.id);
                            values.Add("response_string", responseString);
                            values.Add("school_code", _schoolCode);
                            return values;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }

            return null;

        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var redirectUri = returnUrl.GetLeftPart(UriPartial.Path) + "?target=_blank";
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("code", authorizationCode);
            values.Add("client_id", _appId);
            values.Add("client_secret", _appSecret);
            values.Add("redirect_uri", redirectUri);
            values.Add("grant_type", "authorization_code");
            values.Add("response_type", "code");
            values.Add("theme", _theme);
            values.Add("school_code", _schoolCode);
            values.Add("sso", _sso);
            string postData = String.Join("&",
                values.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value))
                      .ToArray());
            WebRequest webRequest = WebRequest.Create(TokenEndpoint);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = postData.Length;
            webRequest.Method = "POST";
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                StreamWriter streamWriter = new StreamWriter(requestStream);
                streamWriter.Write(postData);
                streamWriter.Flush();
            }
            try
            {
                using (var webResponse = webRequest.GetResponse())
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        if (responseStream == null)
                            return null;
                        StreamReader streamReader = new StreamReader(responseStream);
                        dynamic response = JsonConvert.DeserializeObject<dynamic>(streamReader.ReadToEnd());
                        return (string)response.access_token;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static Uri BuildUri(string baseUri, NameValueCollection queryParameters)
        {
            var keyValuePairs = queryParameters.AllKeys.Select(k => HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(queryParameters[k]));
            var qs = String.Join("&", keyValuePairs);
            var builder = new UriBuilder(baseUri) { Query = qs };
            return builder.Uri;
        }

        public static void RewriteRequest()
        {
            var ctx = HttpContext.Current;
            var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString == null || !stateString.Contains("__provider__=tiup"))
                return;
            var q = HttpUtility.ParseQueryString(stateString);
            q.Add(ctx.Request.QueryString);
            q.Remove("state");
            ctx.RewritePath(ctx.Request.Path + "?" + q);
        }
    }
}
