using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ugona_net
{
    public partial class AuthPage : PhoneApplicationPage
    {

        public AuthPage()
        {
            InitializeComponent();
        }

        private void TextChanged(object sender, RoutedEventArgs e)
        {
            SignIn.IsEnabled = (Login.Text.Length > 0) && (Password.Password.Length > 0);
            Error.Text = "";
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
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.Text = Helper.GetString("Authorization");
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                JObject obj = await Helper.GetApi("key", "login", login, "password", password);
                Helper.PutSettings(Names.KEY, obj["key"].ToString());
                Helper.PutSettings(Names.AUTH, obj["auth"].ToString());
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