using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Windows.Navigation;

namespace ugona_net
{
    public partial class PhotoView : PhoneApplicationPage, INotifyPropertyChanged
    {
        Photo photo;

        public PhotoView()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            photo = PhoneApplicationService.Current.State["Photo"] as Photo;
            NotifyPropertyChanged("ImageUrl");
        }

        public String ImageUrl
        {
            get
            {
                if (photo == null)
                    return null;
                return photo.ImageUrl;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}