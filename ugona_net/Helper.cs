using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
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

        static public T GetSetting<T>(String name, T defaultValue)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(name))
                return defaultValue;
            return (T)IsolatedStorageSettings.ApplicationSettings[name];
        }


        static public void PutSetting(String name, Object value)
        {
            if (value == null)
            {
                IsolatedStorageSettings.ApplicationSettings.Remove(name);
                return;
            }
            IsolatedStorageSettings.ApplicationSettings[name] = value;
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

        async static public Task<JObject> PostData(String url, String postBody)
        {
            if (httpClient == null)
            {
                HttpMessageHandler handler = new BypassCacheClientHandler();
                httpClient = new HttpClient(handler);
                if (user_agent != null)
                    httpClient.DefaultRequestHeaders.Add("User-Agent", user_agent);
            }
            HttpResponseMessage response = await httpClient.PostAsync(url, 
                new StringContent(postBody, Encoding.UTF8, "application/json"));
            String result = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(result);
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

        static public void SetData(Object to, JObject obj)
        {
            Delegate[] delegates = null;
            FieldInfo info = to.GetType().GetField("PropertyChanged",
                       System.Reflection.BindingFlags.Instance |
                       System.Reflection.BindingFlags.NonPublic);
            if (info != null)
            {
                MulticastDelegate eventDelagate =
                          (MulticastDelegate)info.GetValue(to);
                if (eventDelagate != null)
                    delegates = eventDelagate.GetInvocationList();
            }
            SetData(to, obj, "", delegates);
        }

        static public void SetData(Object to, JObject obj, String prefix, Delegate[] delegates)
        {
            Type obj_type = to.GetType();
            MethodInfo fromJson = obj_type.GetMethod("FromJson");
            if (fromJson != null)
            {
                Object[] args = { obj };
                fromJson.Invoke(to, args);
                if ((delegates != null) && (prefix != null))
                {
                    String pname = prefix.Substring(0, prefix.Length - 1);
                    foreach (Delegate dlg in delegates)
                    {
                        dlg.Method.Invoke(dlg.Target, new object[] { to, new PropertyChangedEventArgs(pname) });
                    }
                }              
                return;
            }

            PropertyInfo[] props = obj_type.GetProperties();
            foreach (PropertyInfo p in props)
            {
                JToken v = obj[p.Name];
                if (v == null)
                    continue;
                Type type = p.PropertyType;
                if (type.IsClass && (type != typeof(String)) && (type != typeof(string)))
                {
                    String name = p.Name;
                    if (prefix != null)
                        name = prefix + name;
                    SetData(p.GetValue(to), v.ToObject<JObject>(), name + ".", delegates);
                    continue;
                }
                if (!p.CanWrite)
                    continue;
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    type = Nullable.GetUnderlyingType(type);
                if (type == typeof(long))
                {
                    long val = v.ToObject<long>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (long)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(float))
                {
                    float val = v.ToObject<float>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (float)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(int))
                {
                    int val = v.ToObject<int>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (int)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(bool))
                {
                    bool val = v.ToObject<bool>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (bool)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(double))
                {
                    double val = v.ToObject<double>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (double)old))
                        continue;
                    p.SetValue(to, val);
                }
                else
                {
                    String val = v.ToString();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == old.ToString()))
                        continue;
                    p.SetValue(to, val);
                }
                String pname = p.Name;
                if (prefix != null)
                    pname = prefix + pname;
                if (delegates != null)
                {
                    foreach (Delegate dlg in delegates)
                    {
                        dlg.Method.Invoke(dlg.Target, new object[] { to, new PropertyChangedEventArgs(pname) });
                    }
                }
            }
        }

    }
}
