using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace ugona_net
{
    class Stat : IComparable<Stat>
    {
        public Brush Blue
        {
            get
            {
                return Colors.BlueBrush;
            }
        }

        public String Time
        {
            get
            {
                if (level == 2)
                    return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m + 1) + " " + y;
                DateTime date = new DateTime(y, m + 1, d);
                if (level == 1)
                {
                    DateTime end = date.AddDays(6);
                    return date.ToShortDateString() + " - " + end.ToShortDateString();
                }
                return date.ToShortDateString();
            }
        }

        public String Distance
        {
            get
            {
                return String.Format("{0:n2}", s / 1000) + " " + Helper.GetString("km");
            }           
        }

        public String Info
        {
            get
            {
                return String.Format(Helper.GetString("short_status"), Track.timeFormat(t * 1000), s * 3.6 / t, v);
            }
        }

        public Thickness Margin
        {
            get
            {
                return new Thickness(40 - level * 20, 0, 0, 0);
            }
        }

        public int y
        {
            get;
            set;
        }

        public int m
        {
            get;
            set;
        }

        public int d
        {
            get;
            set;
        }

        public double s
        {
            get;
            set;
        }

        public long t
        {
            get;
            set;
        }

        public int v
        {
            get;
            set;
        }

        public int level
        {
            get;
            set;
        }

        public int CompareTo(Stat compareStat)
        {
            if (compareStat == null)
                return 1;
            if (y > compareStat.y)
                return -1;
            if (y < compareStat.y)
                return 1;
            if (m > compareStat.m)
                return -1;
            if (m < compareStat.m)
                return 1;
            if (d > compareStat.d)
                return -1;
            if (d < compareStat.d)
                return 1;
            return 0;
        }
    }
}
