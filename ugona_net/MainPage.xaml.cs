using Microsoft.Phone.Controls;
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
            App.ViewModel.Date = (DateTime) e.NewDateTime;
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

            if (e.NavigationMode == NavigationMode.Back){
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
            foreach (Event ev in Events.ItemsSource){
                if (ev.IsCurrent)
                {
                    ev.IsCurrent = false;
                    if (ev == eNew)
                        return;
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

                foreach (JToken t in tracks_data){
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
                        
                double avg_speed = mileage * 3600000.0 / time;
                TracksInfo.Text = String.Format(Helper.GetString("status"), mileage, Track.timeFormat(time), avg_speed, speed);

                Tracks.ItemsSource = tracks;
                Tracks.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
            }
            TracksProgress.Visibility = Visibility.Collapsed;
        }

        private void TrackClick(object sender, SelectionChangedEventArgs e)
        {
            Track tNew = Tracks.SelectedItem as Track;
            if (tNew == null)
                return;
            foreach (Track t in Tracks.ItemsSource)
            {
                if (t.IsCurrent)
                {
                    t.IsCurrent = false;
                    if (t == tNew)
                        return;
                    break;
                }
            }
            tNew.IsCurrent = true;
        }
    }
}