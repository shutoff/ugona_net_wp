using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ugona_net
{
    class Track : INotifyPropertyChanged
    {
        public String Time
        {
            get
            {
                DateTime start = DateUtils.ToDateTime(begin);
                DateTime finish = DateUtils.ToDateTime(end);
                return start.ToShortTimeString() + "-" + finish.ToShortTimeString();
            }
        }

        bool is_current;

        public bool IsCurrent
        {
            get
            {
                return is_current;
            }
            set
            {
                if (value == is_current)
                    return;
                is_current = value;
                NotifyPropertyChanged("InfoVisible");
            }
        }

        public Visibility InfoVisible
        {
            get
            {
                return IsCurrent ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public String Distance
        {
            get
            {
                return String.Format("{0:n2}", mileage) + " " + Helper.GetString("km");
            }
        }

        private string[] sep = { ", " };

        public Brush Blue
        {
            get
            {
                return Colors.BlueBrush;
            }
        }

        public Brush Color
        {
            get
            {
                return (Brush)App.Current.Resources["PhoneForegroundBrush"];
            }
        }

        public String Info
        {
            get
            {
                return String.Format(Helper.GetString("short_status"),
                    timeFormat(end - begin), avg_speed, max_speed);
            }
        }

        static public String timeFormat(long ms)
        {
            long minutes = (ms + 30000) / 60000;
            if (minutes < 60)
                return String.Format(Helper.GetString("m_format"), minutes);
            long hours = minutes / 60;
            minutes -= hours * 60;
            return String.Format(Helper.GetString("hm_format"), hours, minutes);
        }

        public String Trace
        {
            get
            {
                String[] s_parts = start.Split(sep, StringSplitOptions.None);
                String[] f_parts = finish.Split(sep, StringSplitOptions.None);
                int s = s_parts.Length - 1;
                int f = f_parts.Length - 1;
                while ((s > 0) && (f > 0))
                {
                    if (s_parts[s] != f_parts[f])
                        break;
                    s--;
                    f--;
                }
                String s_res = null;
                for (int i = 0; i <= s; i++)
                {
                    if (s_res == null)
                    {
                        s_res = s_parts[i];
                        continue;
                    }
                    s_res += ", " + s_parts[i];
                }
                String f_res = null;
                for (int i = 0; i <= f; i++)
                {
                    if (f_res == null)
                    {
                        f_res = f_parts[i];
                        continue;
                    }
                    f_res += ", " + f_parts[i];
                }
                return s_res + " - " + f_res;
            }
        }

        public String track
        {
            get;
            set;
        }

        public double mileage
        {
            get;
            set;
        }

        public int max_speed
        {
            get;
            set;
        }

        public long begin
        {
            get;
            set;
        }

        public long end
        {
            get;
            set;
        }

        public double avg_speed
        {
            get;
            set;
        }

        public double day_mileage
        {
            get;
            set;
        }

        public int day_max_speed
        {
            get;
            set;
        }

        public String start
        {
            get;
            set;
        }

        public String finish
        {
            get;
            set;
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
