using Microsoft.Phone.Controls;
using System;
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
                    Helper.RemoveSettings(Names.KEY);
                    NavigationService.Navigate(new Uri("/AuthPage.xaml", UriKind.Relative));
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
    }
}