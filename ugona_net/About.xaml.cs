using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Reflection;

namespace ugona_net
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            Version.Text = Helper.GetString("version") + ": " + nameHelper.Version;
        }

        private void ServiceClick(object sender, EventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://www.ugona.net/", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void ForumClick(object sender, EventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://forum.ugona.net/topic47012.html", UriKind.Absolute);
            webBrowserTask.Show();
        }
    }
}