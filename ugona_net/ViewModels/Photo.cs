using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ugona_net
{
    class Photo
    {
        public String Time
        {
            get
            {
                DateTime t = DateUtils.ToDateTime(eventTime);
                return t.ToLongTimeString();
            }
        }

        public String Camera
        {
            get
            {
                return "#" + cameraNumber;
            }
        }

        public String ImageUrl
        {
            get
            {
                String url = "https://car-online.ugona.net/photo?id=" + id + "&skey=" + App.ViewModel.Car.api_key;
                return url;
            }
        }

        public long id
        {
            get;
            set;
        }

        public int cameraNumber
        {
            get;
            set;
        }

        public long eventTime
        {
            get;
            set;
        }
    }
}
