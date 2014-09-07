using Newtonsoft.Json.Linq;
using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ugona_net.Resources;

namespace ugona_net
{
    class Helper
    {
        static public String GetString(String message)
        {
            String localized = AppResources.ResourceManager.GetString(message, AppResources.Culture);
            if (localized != null)
                return localized;
            return message;
        }

        static public String GetSetting(String name)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(name))
                return null;
            return IsolatedStorageSettings.ApplicationSettings[name] as String;
        }

        static public String GetSetting(String name, String defaultValue)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(name))
                return defaultValue;
            return IsolatedStorageSettings.ApplicationSettings[name] as String;
        }

        static public long GetLongSetting(String name)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(name))
                return 0;
            return Convert.ToInt64(IsolatedStorageSettings.ApplicationSettings[name]);
        }

        static public void PutSettings(String name, Object value)
        {
            IsolatedStorageSettings.ApplicationSettings[name] = value;
        }

        static public void RemoveSettings(String name)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove(name);
        }

        static public void Flush()
        {
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        static String user_agent = null;

        private const string Html =
@"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>
<head>
<script language=""javascript"" type=""text/javascript"">
function notifyUA() {
    window.external.notify(navigator.userAgent);
}
</script>
</head>
<body onload=""notifyUA();""></body>
</html>";

        static public void Init(Panel rootElement)
        {
            if (user_agent != null)
                return;
            var browser = new Microsoft.Phone.Controls.WebBrowser();
            browser.IsScriptEnabled = true;
            browser.Visibility = Visibility.Collapsed;
            browser.Loaded += (sender, args) => browser.NavigateToString(Html);
            browser.ScriptNotify += (sender, args) =>
            {
                string userAgent = args.Value;
                rootElement.Children.Remove(browser);
                user_agent = userAgent;
                if (httpClient != null)
                    httpClient.DefaultRequestHeaders.Add("User-Agent", user_agent);
            };
            rootElement.Children.Add(browser);
        }

        static private HttpClient httpClient = null;

        class BypassCacheClientHandler : HttpClientHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Headers.IfModifiedSince == null)
                    request.Headers.IfModifiedSince = new DateTimeOffset(DateTime.Now);
                var browser = new Microsoft.Phone.Controls.WebBrowser();
                return base.SendAsync(request, cancellationToken);
            }
        }

        async static public Task<JObject> GetJson(String url, params Object[] values)
        {
            for (int n = 1; ; n++)
            {
                int pos = url.IndexOf("$" + n);
                if (pos < 0)
                    break;
                url = url.Replace("$" + n, HttpUtility.UrlEncode(values[n - 1].ToString()));
            }

            if (httpClient == null)
            {
                HttpMessageHandler handler = new BypassCacheClientHandler();
                httpClient = new HttpClient(handler);
                if (user_agent != null)
                    httpClient.DefaultRequestHeaders.Add("User-Agent", user_agent);
            }
            HttpResponseMessage response = await httpClient.GetAsync(url);
            String body = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(body);
            if (obj["error"] != null)
            {
                throw new Exception(obj["error"].ToString());
            }
            return obj;
        }

        static public Task<JObject> GetApi(String method, params Object[] values)
        {
            String url = "https://car-online.ugona.net/";
            url += method;
            bool first = true;
            for (int i = 0; i < values.Length; i += 2)
            {
                if (values[i + 1] == null)
                    continue;
                if (first)
                {
                    first = false;
                    url += "?";
                }
                else
                {
                    url += "&";
                }
                url += values[i];
                url += "=";
                url += HttpUtility.UrlEncode(values[i + 1].ToString());
            }
            return GetJson(url);
        }
    }
}
