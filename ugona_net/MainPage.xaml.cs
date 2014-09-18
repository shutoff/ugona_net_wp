﻿using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace ugona_net
{
    public partial class MainPage : PhoneApplicationPage
    {
        DispatcherTimer refreshTimer;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContext = App.ViewModel;

            App.ViewModel.PropertyChanged += OnPropertyChanged;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void OnAddressClick(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["MapEvent"] = null;
            PhoneApplicationService.Current.State["MapTracks"] = null;
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
        }

        private void OnDateClick(object sender, RoutedEventArgs e)
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Helper.Init(LayoutRoot);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Error")
            {
                if (App.ViewModel.Error == "Auth error")
                    ShowAuth();
            }
            if (e.PropertyName == "Date")
            {
                if (Pivot.SelectedItem != null)
                {
                    PivotItem item = (PivotItem)Pivot.SelectedItem;
                    if (item.Name == "EventsPage")
                        LoadEvents();
                    if (item.Name == "TracksPage")
                        LoadTracks();
                }
            }
        }

        private void DateChanged(object sender, DateTimeValueChangedEventArgs e)
        {
            if (e.NewDateTime == null)
                return;
            App.ViewModel.Date = (DateTime)e.NewDateTime;
        }

        bool show_auth;

        protected void ShowAuth()
        {
            show_auth = true;
            Helper.RemoveSetting(Names.KEY);
            NavigationService.Navigate(new Uri("/AuthPage.xaml", UriKind.Relative));
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                String key = Helper.GetSetting(Names.KEY, "demo");
                if (key == "demo")
                {
                    ShowAuth();
                    return;
                }
            }

            if (e.NavigationMode == NavigationMode.Back)
            {
                if (show_auth && (Helper.GetSetting(Names.KEY) == null))
                {
                    Application.Current.Terminate();
                    return;
                }
                show_auth = false;
            }

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            App.ViewModel.refresh();
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

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}

        List<ApplicationBarIconButton> pageButtons = null;

        private void Pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (pageButtons == null)
                pageButtons = new List<ApplicationBarIconButton>();
            foreach (ApplicationBarIconButton button in pageButtons)
            {
                ApplicationBar.Buttons.Remove(button);
            }
            pageButtons.Clear();
            if (e.Item.Name == "EventsPage")
            {
                addButton(((Event.Filter & 1) != 0) ? "appbar.user" : "appbar.user.off", "actions", ChangeFilter);
                addButton(((Event.Filter & 2) != 0) ? "appbar.contacts" : "appbar.contacts.off", "contacts", ChangeFilter);
                addButton(((Event.Filter & 4) != 0) ? "appbar.system" : "appbar.system.off", "system", ChangeFilter);
                LoadEvents();
            }
            if (e.Item.Name == "TracksPage")
                LoadTracks();
            if (e.Item.Name == "StatPage")
                LoadStat();
        }

        private void ChangeFilter(object sender, EventArgs e)
        {
            int n = 0;
            foreach (ApplicationBarIconButton button in pageButtons)
            {
                if (button == sender)
                {
                    int mask = 1 << n;
                    bool off = true;
                    if ((Event.Filter & mask) != 0)
                    {
                        Event.Filter &= ~mask;
                    }
                    else
                    {
                        Event.Filter |= mask;
                        off = false;
                    }
                    string icon = "";
                    switch (n)
                    {
                        case 0:
                            icon = "appbar.user";
                            break;
                        case 1:
                            icon = "appbar.contacts";
                            break;
                        case 2:
                            icon = "appbar.system";
                            break;
                    }
                    if (off)
                        icon += ".off";
                    button.IconUri = new Uri("/Assets/Icons/" + icon + ".png", UriKind.Relative);
                    FilterEvents();
                    return;
                }
                n++;
            }
        }

        private void addButton(string icon, string text, EventHandler handler)
        {
            ApplicationBarIconButton button = new ApplicationBarIconButton(new Uri("/Assets/Icons/" + icon + ".png", UriKind.Relative));
            button.Text = Helper.GetString(text);
            button.Click += handler;
            ApplicationBar.Buttons.Insert(ApplicationBar.Buttons.Count, button);
            pageButtons.Insert(pageButtons.Count, button);
        }

        List<Event> events;

        async private void LoadEvents()
        {
            Events.Visibility = Visibility.Collapsed;
            EventsProgress.Visibility = Visibility.Visible;
            EventsText.Visibility = Visibility.Collapsed;
            DateTime current = App.ViewModel.Date;
            DateTime begin = new DateTime(current.Year, current.Month, current.Day);
            DateTime end = begin.AddDays(1);
            try
            {
                JObject res = await Helper.GetApi("events",
                    "skey", Helper.GetSetting(Names.KEY),
                    "begin", DateUtils.ToJavaTime(begin),
                    "end", DateUtils.ToJavaTime(end),
                    "first", IsCurrentDate() ? "true" : "",
                    "pointer", "",
                    "auth", Helper.GetSetting(Names.AUTH));
                JArray ev = res.GetValue("events").ToObject<JArray>();
                events = new List<Event>();
                foreach (JToken el in ev)
                {
                    Event e = new Event();
                    Helper.SetData(e, el.ToObject<JObject>(), "", null);
                    events.Add(e);
                }
                FilterEvents();
            }
            catch (Exception ex)
            {
                EventsText.Text = Helper.GetString(ex.Message);
                EventsText.Visibility = Visibility.Visible;
            }
            EventsProgress.Visibility = Visibility.Collapsed;
        }

        private void FilterEvents()
        {
            ObservableCollection<Event> filtered = new ObservableCollection<Event>();
            bool first = IsCurrentDate();
            foreach (Event e in events)
            {
                e.IsCurrent = false;
                if (first)
                {
                    first = false;
                    filtered.Add(e);
                    continue;
                }
                int mask = e.mask;
                if ((mask != 0) && ((mask & Event.Filter) == 0))
                    continue;
                filtered.Add(e);
            }
            if (filtered.Count == 0)
            {
                EventsText.Text = Helper.GetString("No events");
                EventsText.Visibility = Visibility.Visible;
                Events.Visibility = Visibility.Collapsed;
            }
            Events.ItemsSource = filtered;
            Events.Visibility = Visibility.Visible;
            EventsText.Visibility = Visibility.Collapsed;
        }

        private bool IsCurrentDate()
        {
            DateTime current = App.ViewModel.Date;
            DateTime now = DateTime.Now;
            return (current.Day == now.Day) && (current.Month == now.Month) && (current.Year == now.Year);
        }

        private void EventClick(object sender, SelectionChangedEventArgs e)
        {
            Event eNew = Events.SelectedItem as Event;
            if (eNew == null)
                return;
            Events.SelectedIndex = -1;
            foreach (Event ev in Events.ItemsSource)
            {
                if (ev.IsCurrent)
                {
                    if (ev == eNew)
                    {
                        if ((ev.gps.lat != null) && (ev.gps.lng != null))
                        {
                            PhoneApplicationService.Current.State["MapEvent"] = ev;
                            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
                        }
                        return;
                    }
                    ev.IsCurrent = false;
                    break;
                }
            }
            eNew.IsCurrent = true;
        }

        private static char[] separator = { '|' };

        async private void LoadTracks()
        {
            TracksProgress.Visibility = Visibility.Visible;
            Tracks.Visibility = Visibility.Collapsed;
            TracksText.Visibility = Visibility.Collapsed;
            TracksInfo.Text = "";
            DateTime current = App.ViewModel.Date;
            DateTime begin = new DateTime(current.Year, current.Month, current.Day);
            DateTime end = begin.AddDays(1);
            try
            {
                JObject res = await Helper.GetApi("tracks",
                    "skey", Helper.GetSetting(Names.KEY),
                    "begin", DateUtils.ToJavaTime(begin),
                    "end", DateUtils.ToJavaTime(end));
                JArray tracks_data = res.GetValue("tracks").ToObject<JArray>();
                ObservableCollection<Track> tracks = new ObservableCollection<Track>();

                double mileage = 0;
                long time = 0;
                int speed = 0;

                foreach (JToken t in tracks_data)
                {
                    Track track = new Track();
                    Helper.SetData(track, t.ToObject<JObject>(), "", null);
                    String[] points = track.track.Split(separator);
                    if (points.Length < 2)
                        continue;
                    track.start = await AddressHelper.get(points[0]);
                    track.finish = await AddressHelper.get(points[points.Length - 1]);
                    mileage += track.day_mileage;
                    time += (track.end - track.begin);
                    if (track.day_max_speed > speed)
                        speed = track.day_max_speed;
                    tracks.Add(track);
                }

                if (tracks.Count > 0)
                {
                    double avg_speed = mileage * 3600000.0 / time;
                    TracksInfo.Text = "|" + String.Format(Helper.GetString("status"), mileage, Track.timeFormat(time), avg_speed, speed);

                    Tracks.ItemsSource = tracks;
                    Tracks.Visibility = Visibility.Visible;
                }
                else
                {
                    TracksText.Text = Helper.GetString("no_data");
                    TracksText.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                TracksText.Text = ex.Message;
                TracksText.Visibility = Visibility.Visible;
            }
            TracksProgress.Visibility = Visibility.Collapsed;
        }

        private void TrackClick(object sender, SelectionChangedEventArgs e)
        {
            Track tNew = Tracks.SelectedItem as Track;
            if (tNew == null)
                return;
            Tracks.SelectedIndex = -1;
            foreach (Track t in Tracks.ItemsSource)
            {
                if (t.IsCurrent)
                {
                    if (t == tNew)
                    {
                        ObservableCollection<Track> tracks = new ObservableCollection<Track>();
                        tracks.Add(t);
                        PhoneApplicationService.Current.State["MapEvent"] = null;
                        PhoneApplicationService.Current.State["MapTracks"] = tracks;
                        NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
                        return;
                    }
                    t.IsCurrent = false;
                    break;
                }
            }
            tNew.IsCurrent = true;
        }

        private void OnTracksClick(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.State["MapEvent"] = null;
            PhoneApplicationService.Current.State["MapTracks"] = Tracks.ItemsSource;
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
        }

        List<Stat> stat;


        async private void LoadStat()
        {
            StatProgress.Visibility = Visibility.Visible;
            Stat.Visibility = Visibility.Collapsed;
            StatText.Visibility = Visibility.Collapsed;
            StatInfo.Text = "";
            try
            {
                JObject res = await Helper.GetApi("stat",
                    "skey", Helper.GetSetting(Names.KEY),
                    "tz", TimeZoneInfo.Local.StandardName);
                stat = new List<Stat>();
                foreach (var x in res)
                {
                    int year = Int32.Parse(x.Key);
                    JObject year_data = x.Value.ToObject<JObject>();
                    foreach (var y in year_data)
                    {
                        int month = Int32.Parse(y.Key);
                        JArray month_data = y.Value.ToObject<JArray>();
                        foreach (JToken d in month_data)
                        {
                            Stat s = new Stat();
                            Helper.SetData(s, d.ToObject<JObject>());
                            s.y = year;
                            s.m = month;
                            stat.Add(s);
                        }
                    }
                }
                stat.Sort();
                double total_s = 0;
                long total_t = 0;
                int max_v = 0;
                ObservableCollection<Stat> items = new ObservableCollection<Stat>();
                Stat item = new Stat();
                foreach (Stat s in stat)
                {
                    total_s += s.s;
                    total_t += s.t;
                    if (s.v > max_v)
                        max_v = s.v;
                    if ((item.y != s.y) || (item.m != s.m))
                    {
                        if (item.t > 0)
                            items.Add(item);
                        item = new Stat();
                        item.level = 2;
                        item.y = s.y;
                        item.m = s.m;
                    }
                    item.s += s.s;
                    item.t += s.t;
                    if (s.v > item.v)
                        item.v = s.v;
                }
                if (item.t > 0)
                    items.Add(item);

                if (total_t > 0)
                {
                    Stat.ItemsSource = items;
                    Stat.Visibility = Visibility.Visible;
                    StatInfo.Text = String.Format(Helper.GetString("status"), total_s / 1000, Track.timeFormat(total_t * 1000), total_s * 3.6 / total_t, max_v);
                }
                else
                {
                    StatText.Text = Helper.GetString("no_data");
                    StatText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                StatText.Text = ex.Message;
                StatText.Visibility = Visibility.Visible;
            }
            StatProgress.Visibility = Visibility.Collapsed;
        }

        private void StatClick(object sender, SelectionChangedEventArgs e)
        {
            Stat sNew = Stat.SelectedItem as Stat;
            if (sNew == null)
                return;
            Stat.SelectedIndex = -1;
            ObservableCollection<Stat> items = Stat.ItemsSource as ObservableCollection<Stat>;
            for (int i = 0; i < items.Count; i++)
            {
                Stat s = items[i];
                if (s != sNew)
                    continue;

                if ((i < items.Count - 1) && (items[i + 1].level < s.level))
                {
                    for (i++; i < items.Count; )
                    {
                        if (items[i].level >= s.level)
                            break;
                        items.RemoveAt(i);
                    }
                    break;
                }
                if (s.level == 2)
                {
                    DateTime start = new DateTime(s.y, s.m + 1, 1);
                    DayOfWeek first = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
                    while (start.DayOfWeek != first)
                    {
                        start = start.AddDays(-1);
                    }
                    for (; ; )
                    {
                        DateTime end = start.AddDays(7);
                        Stat n = new Stat();
                        n.level = 1;
                        n.y = end.Year;
                        n.m = end.Month - 1;
                        n.d = end.Day;
                        int p;
                        for (p = 0; p < stat.Count; p++)
                        {
                            if (stat[p].CompareTo(n) > 0)
                                break;
                        }
                        n.y = start.Year;
                        n.m = start.Month - 1;
                        n.d = start.Day;
                        for (; p < stat.Count; p++)
                        {
                            Stat day_s = stat[p];
                            if (day_s.CompareTo(n) > 0)
                                break;
                            n.s += day_s.s;
                            n.t += day_s.t;
                            if (day_s.v > n.v)
                                n.v = day_s.v;
                        }
                        if (n.t > 0)
                            items.Insert(i + 1, n);
                        if ((end.Month != s.m + 1) || (end.Year != s.y))
                            break;
                        start = end;
                    }
                }
                if (s.level == 1)
                {
                    Stat m_item = s;
                    for (int mi = i - 1; mi >= 0; mi--)
                    {
                        m_item = items[mi];
                        if (m_item.level == 2)
                            break;
                    }
                    DateTime dt = new DateTime(s.y, s.m + 1, s.d);
                    dt = dt.AddDays(7);
                    Stat end = new Stat();
                    end.y = dt.Year;
                    end.m = dt.Month - 1;
                    end.d = dt.Day;
                    int p;
                    for (p = 0; p < stat.Count; p++)
                    {
                        if (stat[p].CompareTo(end) > 0)
                            break;
                    }
                    for (; p < stat.Count; p++)
                    {
                        Stat ds = stat[p];
                        if (ds.CompareTo(s) > 0)
                            break;
                        if (m_item.m != ds.m)
                            continue;
                        if (m_item.y != ds.y)
                            continue;
                        items.Insert(++i, ds);
                    }
                }
            }
        }
    }
}