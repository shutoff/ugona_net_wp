using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Threading;
using Windows.Devices.Geolocation;
using System.Threading.Tasks;

namespace ugona_net
{
    public partial class MapPage : PhoneApplicationPage
    {
        public MapPage()
        {
            InitializeComponent();
            map.Loaded += Map_OnLoaded;
            DataContext = App.ViewModel;
        }

        DispatcherTimer refreshTimer;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (refreshTimer == null)
            {
                refreshTimer = new DispatcherTimer();
                refreshTimer.Interval = TimeSpan.FromMinutes(1);
                refreshTimer.Tick += OnRefresh;
            }
            refreshTimer.Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            refreshTimer.Stop();
        }

        void OnRefresh(Object sender, EventArgs args)
        {
            App.ViewModel.refresh();
        }

        private void Map_OnLoaded(object sender, RoutedEventArgs e)
        {
            SaveFilesToIsoStore();
            map.ScriptNotify += Map_OnScriptNotify;
            map.LoadCompleted += Map_OnLoadCompleted;
            map.IsScriptEnabled = true;
            map.IsGeolocationEnabled = true;
            map.Navigate(new Uri("html/map.html", UriKind.Relative));
        }

        private void Map_OnScriptNotify(object sender, NotifyEventArgs e)
        {
            String[] data = e.Value.Split(separator);
            if (data[0] == "init"){
                App.ViewModel.PropertyChanged += CarPropertyChanged;
            }
        }

        private void CarPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Address")
            {
                SetData();
            }
        }

        async private void Map_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            SetConfig();
            SetData();
            CallJs("init");
        }

        private void SetConfig()
        {
            CallJs("setConfig", App.ViewModel.MapType, 1, Helper.GetString("km/h"), 1, Helper.GetString("ResourceLanguage"));
        }

        private char[] separator = {'|'};

        private void SetData()
        {
            String data = String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", App.ViewModel.Latitude) + ";";
            data += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", App.ViewModel.Longitude) + ";";
            if (App.ViewModel.Course != null)
                data += App.ViewModel.Course;
            data += ";";
            String[] parts = App.ViewModel.Address.Split(separator);
            bool odd = true;
            foreach (String part in parts){
                String p = part.Replace("\n", "<br/>");
                if (odd){
                    odd = false;
                    if (part.Length == 0)
                        continue;
                    data += "<b>";
                    data += p;
                    data += "</b>";
                    continue;
                }
                data += p;
                odd = true;
            }
            data += ";";
            CallJs("setData", data);
        }

        private void PositionClick(object sender, EventArgs e)
        {
            CallJs("setPosition");
        }

        private void CenterClick(object sender, EventArgs e)
        {
            CallJs("center");
        }

        private void GoogleClick(object sender, EventArgs e)
        {
            SetMapType("Google");
        }

        private void YandexClick(object sender, EventArgs e)
        {
            SetMapType("Yandex");
        }

        private void BingClick(object sender, EventArgs e)
        {
            SetMapType("Bing");
        }

        private void OsmClick(object sender, EventArgs e)
        {
            SetMapType("OSM");
        }

        private void TrafficClick(object sender, EventArgs e)
        {
            App.ViewModel.Traffic = !App.ViewModel.Traffic;
            CallJs("setTraffic", App.ViewModel.Traffic);
        }

        private void SetMapType(String type)
        {
            if (type == App.ViewModel.MapType)
                return;
            App.ViewModel.MapType = type;
            CallJs("setMapType", type);
        }

        private void CallJs(String func, params Object[] values)
        {
            String url = "javascript:";
            url += func;
            url += '(';
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                    url += ',';
                Object v = values[i];
                Type t = v.GetType();
                if (t == typeof(int))
                {
                    url += v;
                }
                else if (t == typeof(bool))
                {
                    url += (bool)v ? "true" : "false";
                }
                else if (t == typeof(double))
                {
                    url += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", (double)v);
                }
                else
                {
                    url += JsEscape(v.ToString());
                }
            }
            url += ')';
            map.Navigate(new Uri(url, UriKind.Absolute));
        }

        private void SaveFilesToIsoStore()
        {
            //These files must match what is included in the application package,
            //or BinaryStream.Dispose below will throw an exception.
            string[] files = {
                "html/map.html",
                "html/leaflet/leaflet.css",
                "html/leaflet/leaflet-src.js",
                "html/leaflet/Location.js",
                "html/leaflet/Traffic.js",
                "html/leaflet/Points.js",
                "html/leaflet/Map.js",
                "html/leaflet/Bing.js",
                "html/leaflet/Google.js",
                "html/leaflet/Yandex.js",
                "html/leaflet/images/layers-2x.png",
                "html/leaflet/images/layers.png",
                "html/leaflet/images/marker-icon-2x.png",
                "html/leaflet/images/marker-icon.png",
                "html/leaflet/images/marker-shadow.png",
                "html/leaflet/images/arrow.png",
                "html/leaflet/images/marker.png",
                "html/leaflet/images/cur_arrow.png",
                "html/leaflet/images/cur_marker.png",
                "html/leaflet/images/person.png",
            };

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            foreach (string f in files)
            {
                StreamResourceInfo sr = Application.GetResourceStream(new Uri(f, UriKind.Relative));
 //               if (!isoStore.FileExists(f))
                {
                    using (BinaryReader br = new BinaryReader(sr.Stream))
                    {
                        byte[] data = br.ReadBytes((int)sr.Stream.Length);
                        SaveToIsoStore(f, data);
                    }
                }
            }
        }

        private void SaveToIsoStore(string fileName, byte[] data)
        {
            string strBaseDir = string.Empty;
            string delimStr = "/";
            char[] delimiter = delimStr.ToCharArray();
            string[] dirsPath = fileName.Split(delimiter);

            //Get the IsoStore.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            //Re-create the directory structure.
            for (int i = 0; i < dirsPath.Length - 1; i++)
            {
                strBaseDir = System.IO.Path.Combine(strBaseDir, dirsPath[i]);
                isoStore.CreateDirectory(strBaseDir);
            }

            //Remove the existing file.
            if (isoStore.FileExists(fileName))
            {
                isoStore.DeleteFile(fileName);
            }

            //Write the file.
            using (BinaryWriter bw = new BinaryWriter(isoStore.CreateFile(fileName)))
            {
                bw.Write(data);
                bw.Close();
            }
        }

        static private String JsEscape(String s)
        {
            String res = "'";
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            for (int i = 0; i < bytes.Length; i += 2)
            {
                if ((bytes[i + 1] == 0) && ((bytes[i] >= 0x20) && (bytes[i] != ':')))
                {
                    res += (char)bytes[i];
                    continue;
                }
                res += "\\u" + bytes[i + 1].ToString("X2") + bytes[i].ToString("X2");
            }
            res += "'";
            return res;
        }
    }

}