using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ugona_net
{
    public partial class MainPage : PhoneApplicationPage
    {
        DispatcherTimer refreshTimer;

        // Constructor
        public MainPage()
        {
            show_auth = false;

            InitializeComponent();
            Loaded += OnLoaded;
            DataContext = App.ViewModel;
            
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

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (show_auth)
            {
                show_auth = false;
                if (Helper.GetSetting(Names.KEY) == null)
                {
                    Application.Current.Terminate();
                    return;
                }
            }
            else
            {
                String key = Helper.GetSetting(Names.KEY, "demo");
                if (key == "demo")
                {
                    show_auth = true;
                    Helper.RemoveSetting(Names.KEY);
                    NavigationService.Navigate(new Uri("/AuthPage.xaml", UriKind.Relative));
                    return;
                }
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

        bool show_auth;

        private void Pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item.Name == "EventsPage")
                LoadEvents();
        }

        async private void LoadEvents()
        {
            DateTime current = App.ViewModel.Date;
            DateTime begin = new DateTime(current.Year, current.Month, current.Day);
            DateTime end = begin.AddDays(1);
            try
            {
                JObject res = await Helper.GetApi("events",
                    "skey", Helper.GetSetting(Names.KEY),
                    "begin", DateUtils.ToJavaTime(begin),
                    "end", DateUtils.ToJavaTime(end));
                JArray events = res.GetValue("events").ToObject<JArray>();
                List<Event> event_array = new List<Event>(0);
                foreach (JToken el in events)
                {
                    Event e = new Event();
                    Helper.SetData(e, el.ToObject<JObject>(), "", null);
                    event_array.Add(e);
                }
                Events.ItemsSource = event_array;
                Events.Visibility = Visibility.Visible;
                EventsProgress.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }

        }
    }
}