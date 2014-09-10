using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Input;

namespace ugona_net
{
    public partial class AuthPage : PhoneApplicationPage
    {

        public AuthPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Helper.Init(LayoutRoot);
        }

        private void TextChanged(object sender, RoutedEventArgs e)
        {
            SignIn.IsEnabled = (Login.Text.Length > 0) && (Password.Password.Length > 0);
            Error.Text = "";
        }

        private void OnLoginKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Password.Focus();
            }
        }

        private void OnPasswordKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SignIn.Focus();
                DoAuth(Login.Text, Password.Password);
            }
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            DoAuth(Login.Text, Password.Password);
        }

        private void DemoClick(object sender, RoutedEventArgs e)
        {
            DoAuth("demo", "demo");
        }

        private async void DoAuth(String login, String password)
        {
            if (login == "")
            {
                Login.Focus();
                return;
            }
            if (password == "")
            {
                Password.Focus();
                return;
            }
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.Text = Helper.GetString("Authorization");
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                JObject obj = await Helper.GetApi("key", "login", login, "password", password);
                Helper.PutSetting(Names.KEY, obj["key"].ToString());
                Helper.PutSetting(Names.AUTH, obj["auth"].ToString());
                Helper.Flush();
                String key = Helper.GetSetting(Names.KEY);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Error.Text = Helper.GetString(ex.Message);
            }
            SystemTray.ProgressIndicator.IsIndeterminate = false;
            SystemTray.ProgressIndicator.IsVisible = false;
        }
    }
}