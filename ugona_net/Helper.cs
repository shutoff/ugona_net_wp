using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ugona_net.Resources;
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

        static public void PutSettings(String name, String value)
        {
            IsolatedStorageSettings.ApplicationSettings[name] = value;
        }

        static public void RemoveSettings(String name)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove(name);
        }

        static private HttpClient httpClient = null;

        async static public Task<JObject> GetJson(String url)
        {
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
    }
}
