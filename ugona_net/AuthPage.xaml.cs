using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using PhoneNumbers;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ugona_net
{
    public partial class AuthPage : PhoneApplicationPage
    {
        PhoneNumberUtil util;

        public AuthPage()
        {
            util = PhoneNumberUtil.GetInstance();
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Helper.Init(LayoutRoot);
        }

        private void TextChanged(object sender, RoutedEventArgs e)
        {
            SignIn.IsEnabled = (Login.Text.Length > 0) && (Password.Password.Length > 0) && IsValidNumber();
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
                Phone.Focus();
            }
        }

        private void OnPhoneKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!IsValidNumber())
                    return;
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

        private bool IsValidNumber()
        {
            if (Phone.Text == "")
                return true;
            try
            {
                PhoneNumber number = util.Parse(Phone.Text, CultureInfo.CurrentCulture.Name);
                if (util.IsValidNumber(number))
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
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

            try
            {
                String region = CultureInfo.CurrentCulture.Name;
                int pos = region.LastIndexOf('-');
                if (pos > 0)
                    region = region.Substring(pos + 1);
                PhoneNumber number = util.Parse(Phone.Text, region);
                Phone.Text = util.Format(number, PhoneNumberFormat.INTERNATIONAL);
            }
            catch (Exception)
            {
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
                Helper.PutSetting(Names.PHONE, Phone.Text);
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