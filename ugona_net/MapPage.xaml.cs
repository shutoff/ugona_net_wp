using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Threading;

namespace ugona_net
{
    public partial class MapPage : PhoneApplicationPage
    {
        public MapPage()
        {
            InitializeComponent();
            Map.Loaded += Map_OnLoaded;
            DataContext = App.ViewModel;
        }

        Event ev;
        ObservableCollection<Track> tracks;
        Zone zone;

        DispatcherTimer refreshTimer;
        String track_data;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ev = null;
            if (PhoneApplicationService.Current.State.ContainsKey("MapEvent"))
                ev = PhoneApplicationService.Current.State["MapEvent"] as Event;
            tracks = null;
            if (PhoneApplicationService.Current.State.ContainsKey("MapTracks"))
                tracks = PhoneApplicationService.Current.State["MapTracks"] as ObservableCollection<Track>;
            zone = null;
            if (PhoneApplicationService.Current.State.ContainsKey("MapZone"))
            {
                zone = PhoneApplicationService.Current.State["MapZone"] as Zone;
                NameLabel.Visibility = Visibility.Visible;
                Name.Visibility = Visibility.Visible;
                Name.Text = zone.name;
                Sms.Visibility = Visibility.Visible;
                Sms.IsChecked = zone.sms;
            }
            if ((ev != null) || (tracks != null))
                return;
            track_data = null;
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
            if (refreshTimer != null)
                refreshTimer.Stop();
        }

        void OnRefresh(Object sender, EventArgs args)
        {
            App.ViewModel.refresh();
        }

        private void Map_OnLoaded(object sender, RoutedEventArgs e)
        {
            SaveFilesToIsoStore();
            Map.ScriptNotify += Map_OnScriptNotify;
            Map.LoadCompleted += Map_OnLoadCompleted;
            Map.IsScriptEnabled = true;
            Map.IsGeolocationEnabled = true;
            Map.Navigate(new Uri("html/map.html", UriKind.Relative));
            if ((ev == null) && (tracks == null))
                LoadTrack();
        }

        private void Map_OnScriptNotify(object sender, NotifyEventArgs e)
        {
            String[] data = e.Value.Split(separator);
            if (data[0] == "init") 
            {
                if ((ev == null) && (tracks == null))
                    App.ViewModel.PropertyChanged += CarPropertyChanged;
                Progress.Visibility = Visibility.Collapsed;
                Map.Visibility = Visibility.Visible;
            }
            if (data[0] == "error")
            {
                Progress.Visibility = Visibility.Collapsed;
                Map.Visibility = Visibility.Collapsed;
                Error.Text = data[1];
                Error.Visibility = Visibility.Visible;
            }
            if (data[0] == "setZone")
            {
                String[] coord = data[1].Split(sep);
                zone.lat1 = Double.Parse(coord[0]);
                zone.lng1 = Double.Parse(coord[1]);
                zone.lat2 = Double.Parse(coord[2]);
                zone.lng2 = Double.Parse(coord[3]);
            }
        }

        private void CarPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Address")
            {
                SetData();
                LoadTrack();
            }
        }

        private void Map_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            SetConfig();
            SetData();
            CallJs("init");
        }

        private void SetConfig()
        {
            CallJs("setConfig", App.ViewModel.MapType, App.ViewModel.Traffic, Helper.GetString("km/h"), 1, Helper.GetString("ResourceLanguage"));
        }

        private char[] separator = { '|' };
        private char[] sep = { ',' };

        private static String s(double v)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", v);
        }

        private void SetData()
        {
            String data = null;
            if (zone != null)
            {
                data = String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", zone.lat1) + ",";
                data += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", zone.lng1) + ",";
                data += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", zone.lat2) + ",";
                data += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", zone.lng2);
                CallJsParts(data, "setZone");
                return;
            }

            if (tracks != null)
            {
                List<Track.Marker> markers = new List<Track.Marker>();
                for (int i = 0; i < tracks.Count; i++)
                {
                    Track track = tracks[i];
                    String[] points = track.track.Split(separator);
                    Track.Point start = new Track.Point(points[0]);
                    Track.Point finish = new Track.Point(points[points.Length - 1]);
                    int n_start = markers.Count;
                    double d_best = 200.0;
                    for (int n = 0; n < markers.Count; n++) {
                        Track.Marker m = markers[n];
                        double delta = (double) AddressHelper.distance(start.latitude, start.longitude, m.latitude, m.longitude);
                        if (delta < d_best) {
                            d_best = delta;
                            n_start = n;
                        }
                    }
                    if (n_start >= markers.Count) {
                        Track.Marker m = new Track.Marker();
                        m.latitude = start.latitude;
                        m.longitude = start.longitude;
                        m.address = track.start;
                        m.times = new List<Track.TimeInterval>();
                        markers.Add(m);
                    }
                    Track.Marker marker = markers[n_start];
                    if ((marker.times.Count == 0) || (marker.times[marker.times.Count - 1].end > 0)) {
                        Track.TimeInterval interval = new Track.TimeInterval();
                        marker.times.Add(interval);
                    }
                    marker.times[marker.times.Count - 1].end = track.begin;

                    if (i > 0) {
                        Track prev = tracks[i - 1];
                        points = prev.track.Split(separator);
                        Track.Point last = new Track.Point(points[points.Length - 1]);
                        double delta = (double)AddressHelper.distance(start.latitude, start.longitude, last.latitude, last.longitude);
                        if (delta > 200)
                            data += "|";
                    }
                    data += track.track;

                    int n_finish = markers.Count;
                    d_best = 200;
                    for (int n = 0; n < markers.Count; n++) {
                        if (n == n_start)
                            continue;
                        marker = markers[n];
                        double delta = (double) AddressHelper.distance(finish.latitude, finish.longitude, marker.latitude, marker.longitude);
                        if (delta < d_best) {
                            n_finish = n;
                            d_best = delta;
                        }
                    }
                    if (n_finish >= markers.Count) {
                        marker = new Track.Marker();
                        marker.latitude = finish.latitude;
                        marker.longitude = finish.longitude;
                        marker.address = track.finish;
                        marker.times = new List<Track.TimeInterval>();
                        markers.Add(marker);
                    }
                    marker = markers[n_finish];
                    Track.TimeInterval ti = new Track.TimeInterval();
                    ti.begin = track.end;
                    marker.times.Add(ti);
                }
                data += "|";
                foreach (Track.Marker marker in markers) {
                    data += "|";
                    data += s(marker.latitude);
                    data += ",";
                    data += s(marker.longitude);
                    data += ",<b>";
                    foreach (Track.TimeInterval interval in marker.times) {
                        if (interval.begin > 0) {
                            data += DateUtils.formatTime(interval.begin);
                            if (interval.end > 0)
                                data += "-";
                        }
                        if (interval.end > 0) {
                            data += DateUtils.formatTime(interval.end);
                        }
                        data += " ";
                    }
                    data += "</b><br/>";
                    data += HttpUtility.HtmlEncode(marker.address).Replace(",", "&#x2C;").Replace("|", "&#x7C;");
                }
                CallJsParts(data, "setTrack");
                return;
            }

            if (ev != null)
            {
                data = String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", ev.gps.lat) + ";";
                data += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", ev.gps.lng) + ";";
                if (ev.gps.course != null)
                    data += ev.gps.course;
                data += ";<b>";
                data += ev.Time;
                data += "</b> ";
                data += ev.Name;
                data += "<br/>";
                data += ev.Info.Replace("\n", "<br/>");
                String[] parts = ev.Info.Split(separator);
                data += ";;";
            }
            else
            {
                data = String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", App.ViewModel.Latitude) + ";";
                data += String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", App.ViewModel.Longitude) + ";";
                if (App.ViewModel.Course != null)
                    data += App.ViewModel.Course;
                data += ";";
                String[] parts = App.ViewModel.Address.Split(separator);
                bool odd = true;
                foreach (String part in parts)
                {
                    String p = part.Replace("\n", "<br/>");
                    if (odd)
                    {
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
                data += ";;";
                data += track_data;
            }
            CallJsParts(data, "setData");
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
            Map.Navigate(new Uri(url, UriKind.Absolute));
        }

        bool track_load;

        async private void LoadTrack()
        {
            bool engine = App.ViewModel.Car.contact.input3 || App.ViewModel.Car.contact.realIgnition;
            bool az = App.ViewModel.Car.az;
            if (!engine || az)
            {
                if (track_data != null)
                {
                    track_data = null;
                    SetData();
                }
                return;
            }

            if (track_load)
                return;

            try
            {
                String key = App.ViewModel.Car.api_key;
                long end = App.ViewModel.Car.time;
                long begin = end - 86400000;
                JObject res = await Helper.GetApi("tracks", "skey", key, "begin", begin, "end", end);
                JArray list = res.GetValue("tracks").ToObject<JArray>();
                JObject track = list.Last.ToObject<JObject>();
                track_data = track.GetValue("track").ToString().Replace('|', '_');
                SetData();
                CallJs("showPoints");
            }
            catch (Exception)
            {
            }
            track_load = false;
        }

        private void SaveFilesToIsoStore()
        {
            //These files must match what is included in the application package,
            //or BinaryStream.Dispose below will throw an exception.
            string[] files = {
                "leaflet.min.js",
                "leaflet.css",
                "html/map.html",
                "images/layers-2x.png",
                "images/layers.png",
                "images/marker-icon-2x.png",
                "images/marker-icon.png",
                "images/marker-shadow.png",
                "images/arrow.png",
                "images/marker.png",
                "images/cur_arrow.png",
                "images/cur_marker.png",
                "images/person.png",
            };

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            foreach (string f in files)
            {
                StreamResourceInfo sr = Application.GetResourceStream(new Uri("map/"+ f, UriKind.Relative));
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

            try
            {
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
            catch (Exception)
            {

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

        void CallJsParts(String data, String method)
        {
            String res = "";
            byte[] bytes = Encoding.Unicode.GetBytes(data);
            for (int i = 0; i < bytes.Length; i += 2)
            {
                if (res.Length > 2040)
                {
                    Map.Navigate(new Uri("javascript:setPart('" + res + "')", UriKind.Absolute));
                    res = "";
                }
                if ((bytes[i + 1] == 0) && ((bytes[i] >= 0x20) && (bytes[i] != ':')))
                {
                    res += (char)bytes[i];
                    continue;
                }
                res += "\\u" + bytes[i + 1].ToString("X2") + bytes[i].ToString("X2");
            }
            Map.Navigate(new Uri("javascript:" + method + "('" + res + "')", UriKind.Absolute));
        }
    }

}