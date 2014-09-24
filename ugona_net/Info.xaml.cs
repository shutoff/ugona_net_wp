using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using Microsoft.Phone.Tasks;

namespace ugona_net
{
    public partial class Info : PhoneApplicationPage
    {
        public Info()
        {
            InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            String parameter = string.Empty;
            NavigationContext.QueryString.TryGetValue("parameter", out parameter);
            LoadMessage(parameter);
        }

        String url;

        async void LoadMessage(String id)
        {
            Progress.Visibility = Visibility.Visible;
            Content.Visibility = Visibility.Collapsed;
            try
            {
                JObject res = await Helper.GetApi("message", "id", id);
                Title.Text = res.GetValue("title").ToString();
                Message.Text = res.GetValue("message").ToString();
                JToken u = res.GetValue("url");
                if (u != null)
                {
                    url = u.ToString();
                    More.Visibility = Visibility.Visible;
                }
                else
                {
                    More.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Title.Text = Helper.GetString("error");
                Message.Text = ex.Message;
            }
            Progress.Visibility = Visibility.Collapsed;
            Content.Visibility = Visibility.Visible;
        }

        private void MoreClick(object sender, RoutedEventArgs e)
        {
            if (url != null)
            {
                WebBrowserTask webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = new Uri(url, UriKind.Absolute);
                webBrowserTask.Show();
            }
        }
    }
}