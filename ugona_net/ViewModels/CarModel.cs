using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Globalization;

namespace ugona_net
{
    public class CarModel : INotifyPropertyChanged
    {

        public CarModel()
        {
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public String EventTime
        {
            get
            {
                if (Car.time == 0)
                    return "";
                DateTime dt = Helper.JavaTimeToDateTime(Car.time);
                return dt.ToString();
            }

        }

        public String MainVoltage
        {
            get
            {
                if (Car.voltage.main == null)
                    return "";
                return String.Format("{0:n2} V", Car.voltage.main);
            }
        }

        public String ReservedVoltage
        {
            get
            {
                if (Car.voltage.reserved == null)
                    return "";
                return String.Format("{0:n2} V", Car.voltage.reserved);
            }
        }

        public class Voltage : INotifyPropertyChanged
        {
            public float? main
            {
                get;
                set;
            }

            public float? reserved
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

        public class CarData : INotifyPropertyChanged
        {
            public long time
            {
                get;
                set;
            }

            public long first_time
            {
                get;
                set;
            }

            public Voltage voltage
            {
                get
                {
                    if (voltage_data == null)
                    {
                        voltage_data = new Voltage();
                        voltage_data.PropertyChanged += Voltage_Changed;
                    }
                    return voltage_data;
                }
            }

            private void Voltage_Changed(Object sender, PropertyChangedEventArgs args)
            {
                NotifyPropertyChanged("voltage." + args.PropertyName);
            }

            Voltage voltage_data;

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

        CarData car_data;

        CarData Car
        {
            get
            {
                if (car_data == null)
                {
                    car_data = new CarData();
                    car_data.PropertyChanged += CarPropertyChanged;
                }
                return car_data;
            }

        }

        private void CarPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "time")
                NotifyPropertyChanged("EventTime");
            if (args.PropertyName == "voltage.main")
                NotifyPropertyChanged("MainVoltage");
        }

        public void LoadData()
        {
            String key = Helper.GetSetting(Names.KEY);
            if (key == null)
                return;
            String data = Helper.GetSetting(Names.CAR_DATA);
            if (data != null)
                car_data = JsonConvert.DeserializeObject<CarData>(data);
            this.IsDataLoaded = true;
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


        public void SetData(Object to, JObject obj)
        {
            PropertyInfo[] props = to.GetType().GetProperties();
            MulticastDelegate eventDelagate =
                          (MulticastDelegate)to.GetType().GetField("PropertyChanged",
                           System.Reflection.BindingFlags.Instance |
                           System.Reflection.BindingFlags.NonPublic).GetValue(to);
            Delegate[] delegates = eventDelagate.GetInvocationList();
            foreach (PropertyInfo p in props)
            {
                JToken v = obj[p.Name];
                if (v == null)
                    continue;
                Type type = p.PropertyType;
                if (type.IsClass)
                {
                    SetData(p.GetValue(to), v.ToObject<JObject>());
                    continue;
                }
                if (!p.CanWrite)
                    continue;
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    type = Nullable.GetUnderlyingType(type);
                if (type == typeof(long))
                {
                    long val = v.ToObject<long>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (long)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(float))
                {
                    float val = v.ToObject<float>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (float)old))
                        continue;
                    p.SetValue(to, val);
                }
                else
                {
                    String val = v.ToString();
                    if (val == p.GetValue(to))
                        continue;
                    p.SetValue(to, val);
                }
                foreach (Delegate dlg in delegates)
                {
                    dlg.Method.Invoke(dlg.Target, new object[] { to, new PropertyChangedEventArgs(p.Name) });
                }
            }
        }

        async public void refresh()
        {
            try
            {
                String key = Helper.GetSetting(Names.KEY);
                if (key == null)
                    return;
                JObject res = await Helper.GetApi("", "skey", key, "time", Car.time);
                SetData(Car, res);
            }
            catch (Exception)
            {
            }
        }
    }

}
