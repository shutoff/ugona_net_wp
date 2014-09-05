using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ugona_net.Resources;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;

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

        static public DateTime JavaTimeToDateTime(long javaMS)
        {
            DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime dt = UTCBaseTime.Add(new TimeSpan(javaMS * TimeSpan.TicksPerMillisecond)).ToLocalTime();
            return dt;

        }

        static public String formatTime(long time)
        {
            if (time == 0)
                return "";
            DateTime dt = JavaTimeToDateTime(time);
            String res = dt.ToString("t");
            String date = dt.ToString("d");
            if (date != DateTime.Now.ToString("d"))
                res += " " + date;
            return res;
        }

        static private HttpClient httpClient = null;

        async static public Task<JObject> GetJson(String url, params Object[] values)
        {
            for (int n = 1; ; n++)
            {
                int pos = url.IndexOf("$" + n);
                if (pos < 0)
                    break;
                url = url.Replace("$" + n, HttpUtility.UrlEncode(values[n - 1].ToString()));
            }

            Uri iru = new Uri(url, UriKind.Absolute);
            if (httpClient == null)
            {
                HttpMessageHandler handler = new HttpClientHandler();
                httpClient = new HttpClient(handler);
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
