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
using System.Windows.Media;

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

        public String Error
        {
            get;
            private set;
        }

        public String EventTime
        {
            get
            {
                return Helper.formatTime(Car.time);
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

        public Brush MainVoltageColor
        {
            get
            {
                if ((Car.voltage.main != null) && (Car.voltage.main <= 12.2))
                    return ErrorBrush;
                return (Brush) App.Current.Resources["PhoneForegroundBrush"];
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

        static private Brush error_brush;

        public Brush ErrorBrush
        {
            get
            {
                if (error_brush == null)
                {
                    error_brush = new SolidColorBrush(Color.FromArgb(255, 255, 64, 64));
                }
                return error_brush;
            }
        }

        public String Balance
        {
            get
            {
                if (Car.balance.value == null)
                    return "";
                return String.Format("{0:n2}", Car.balance.value);
            }
        }

        String[] levels;

        public String CarLevel0
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[0] == null)
                    return null;
                return "/Assets/Car/" + levels[0] + ".png";
            }
        }

        public String CarLevel1
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[1] == null)
                    return null;
                return "/Assets/Car/" + levels[1] + ".png";
            }
        }

        public String CarLevel2
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[2] == null)
                    return null;
                return "/Assets/Car/" + levels[2] + ".png";
            }
        }

        public String CarLevel3
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[3] == null)
                    return null;
                return "/Assets/Car/" + levels[3] + ".png";
            }
        }

        public String CarLevel4
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[4] == null)
                    return null;
                return "/Assets/Car/" + levels[4] + ".png";
            }
        }

        public String CarLevel5
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[5] == null)
                    return null;
                return "/Assets/Car/" + levels[5] + ".png";
            }
        }

        public String CarLevel6
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[6] == null)
                    return null;
                return "/Assets/Car/" + levels[6] + ".png";
            }
        }

        public String CarLevel7
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[7] == null)
                    return null;
                return "/Assets/Car/" + levels[7] + ".png";
            }
        }

        public String CarLevel8
        {
            get
            {
                if (levels == null)
                    return null;
                if (levels[8] == null)
                    return null;
                return "/Assets/Car/" + levels[8] + ".png";
            }
        }

        public String Address
        {
            get
            {
                String res = Helper.formatTime(Car.last_stand);
                if (res.Length > 0)
                    res += " ";
                res += "|";
                if ((Car.gps.latitude != null) && (Car.gps.longitude != null))
                {
                    res += " " + String.Format("{0:n5}", Car.gps.latitude);
                    res += " " + String.Format("{0:n5}", Car.gps.longitude);
                }
                res += "|\n";
                if (address != null)
                {
                    string[] separators = new string[] { ", " };
                    String[] parts = address.Split(separators, StringSplitOptions.None);
                    int start = 1;
                    if (parts.Length > 0)
                        res += parts[0];
                    res += "|\n";
                    for (int i = start; i < parts.Length; i++)
                    {
                        if (i > start)
                            res += ", ";
                        res += parts[i];
                    }
                }
                return res;
            }
        }

        public class VoltageData
        {
            public double? main
            {
                get;
                set;
            }

            public double? reserved
            {
                get;
                set;
            }
        }

        public class BalanceData
        {
            public double? value
            {
                get;
                set;
            }

        }

        public class ContactData
        {
            public bool guard
            {
                get;
                set;
            }

            public bool guardMode0
            {
                get;
                set;
            }

            public bool guardMode1
            {
                get;
                set;
            }

            public bool accessory
            {
                get;
                set;
            }

            public bool door_front_left
            {
                get;
                set;
            }

            public bool door_front_right
            {
                get;
                set;
            }


            public bool door_back_left
            {
                get;
                set;
            }

            public bool door_back_right
            {
                get;
                set;
            }

            public bool input1
            {
                get;
                set;
            }

            public bool input2
            {
                get;
                set;
            }

            public bool input3
            {
                get;
                set;
            }

            public bool input4
            {
                get;
                set;
            }

            public bool door
            {
                get;
                set;
            }
            public bool hood
            {
                get;
                set;
            }

            public bool trunk
            {
                get;
                set;
            }

            public bool realIgnition
            {
                get;
                set;
            }
        }

        public class GpsData
        {
            public double? latitude
            {
                get;
                set;
            }

            public double? longitude
            {
                get;
                set;
            }

            public double? speed
            {
                get;
                set;
            }

            public int? course
            {
                get;
                set;
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

            public long guard_time
            {
                get;
                set;
            }

            public long card
            {
                get;
                set;
            }

            public bool az
            {
                get;
                set;
            }

            public long az_start
            {
                get;
                set;
            }

            public long az_stop
            {
                get;
                set;
            }

            public long last_stand
            {
                get;
                set;
            }

            public VoltageData voltage
            {
                get
                {
                    if (voltage_data == null)
                    {
                        voltage_data = new VoltageData();
                    }
                    return voltage_data;
                }
            }

            public BalanceData balance
            {
                get
                {
                    if (balance_data == null)
                    {
                        balance_data = new BalanceData();
                    }
                    return balance_data;
                }
            }

            public ContactData contact
            {
                get
                {
                    if (contact_data == null)
                    {
                        contact_data = new ContactData();
                    }
                    return contact_data;
                }
            }

            public GpsData gps
            {
                get{
                    if (gps_data == null)
                    {
                        gps_data = new GpsData();
                    }
                    return gps_data;
                }
            }

            VoltageData voltage_data;
            BalanceData balance_data;
            ContactData contact_data;
            GpsData gps_data;

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
            {
                NotifyPropertyChanged("MainVoltage");
                NotifyPropertyChanged("MainVoltageColor");
            }
            if (args.PropertyName == "voltage.reserved")
                NotifyPropertyChanged("ReservedVoltage");
            if (args.PropertyName == "balance.value")
                NotifyPropertyChanged("Balance");
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
            UpdateAddress();
            UpdateLevels();
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

        public void SetData(Object to, JObject obj, String prefix, Delegate[] delegates)
        {
            PropertyInfo[] props = to.GetType().GetProperties();
            if (delegates == null)
            {
                MulticastDelegate eventDelagate =
                          (MulticastDelegate)to.GetType().GetField("PropertyChanged",
                           System.Reflection.BindingFlags.Instance |
                           System.Reflection.BindingFlags.NonPublic).GetValue(to);
                delegates = eventDelagate.GetInvocationList();
            }
            foreach (PropertyInfo p in props)
            {
                JToken v = obj[p.Name];
                if (v == null)
                    continue;
                Type type = p.PropertyType;
                if (type.IsClass)
                {
                    String name = p.Name;
                    if (prefix != null)
                        name = prefix + name;
                    SetData(p.GetValue(to), v.ToObject<JObject>(), name + ".", delegates);
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
                else if (type == typeof(int))
                {
                    int val = v.ToObject<int>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (int)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(bool))
                {
                    bool val = v.ToObject<bool>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (bool)old))
                        continue;
                    p.SetValue(to, val);
                }
                else if (type == typeof(double))
                {
                    double val = v.ToObject<double>();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == (double)old))
                        continue;
                    p.SetValue(to, val);
                }
                else
                {
                    String val = v.ToString();
                    Object old = p.GetValue(to);
                    if ((old != null) && (val == old.ToString()))
                        continue;
                    p.SetValue(to, val);
                }
                String pname = p.Name;
                if (prefix != null)
                    pname = prefix + pname;
                foreach (Delegate dlg in delegates)
                {
                    dlg.Method.Invoke(dlg.Target, new object[] { to, new PropertyChangedEventArgs(pname) });
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
                long time = Car.time;
                JObject res = await Helper.GetApi("", "skey", key, "time", Car.time);           
                SetData(Car, res, null, null);
                Error = "";
                NotifyPropertyChanged("Error");
                if (time == Car.time)
                    return;
                UpdateLevels();
                UpdateAddress();
                Helper.PutSettings(Names.CAR_DATA, JsonConvert.SerializeObject(Car));
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                NotifyPropertyChanged("Error");
            }
        }

        double? latitude;
        double? longitude;

        String address;

        async private void UpdateAddress()
        {
            if ((Car.gps.latitude == null) || (Car.gps.longitude == null))
            {
                SetAddress(null);
                return;
            }
            double? d = AddressHelper.distance(Car.gps.longitude, Car.gps.longitude, latitude, longitude);
            if ((d != null) && (d < 50))
                return;
            if (d > 300)
                SetAddress(null);
            String res = await AddressHelper.get(Car.gps.latitude, Car.gps.longitude);
            SetAddress(res);
        }

        private void SetAddress(String addr)
        {
            if (addr == address)
                return;
            address = addr;
            NotifyPropertyChanged("Address");
        }

        private void UpdateLevels()
        {
            if (levels == null)
                levels = new String[9];
            bool doors4 = false;
            DateTime last = DateTime.Now.AddDays(-3);
            DateTime time = Helper.JavaTimeToDateTime(Car.time);
            if (time.CompareTo(last) < 0)
            {
                setLayer(0, doors4 ? "car_black4" : "car_black");
                setLayer(1);
                return;
            }
            bool guard = Car.contact.guard;
            bool guard0 = Car.contact.guardMode0;
            bool guard1 = Car.contact.guardMode1;
            bool card = false;
            if (guard)
            {
                long guard_t = Car.guard_time;
                long card_t = Car.card;
                if ((guard_t > 0) && (card_t > 0) && (card_t < guard_t))
                    card = true;
            }

            bool white = !guard || (guard0 && guard1) || card;
            setModeCar(!white, Car.contact.accessory, doors4);

            if (doors4)
            {
                bool fl = Car.contact.door_front_left;
                setModeOpen(1, "door_fl", !white, fl, fl && guard, false);
                bool fr = Car.contact.door_front_right;
                setModeOpen(6, "door_fr", !white, fr, fr && guard, false);
                bool bl = Car.contact.door_back_left;
                setModeOpen(7, "door_bl", !white, bl, bl && guard, false);
                bool br = Car.contact.door_back_right;
                setModeOpen(8, "door_br", !white, br, br && guard, false);

            }
            else
            {
                bool doors_open = Car.contact.input1;
                bool doors_alarm = Car.contact.door;
                if (white && doors_alarm)
                {
                    doors_alarm = false;
                    doors_open = true;
                }
                setModeOpen(1, "doors", !white, doors_open, doors_alarm, false);
                setLayer(6);
                setLayer(7);
                setLayer(8);
            }

            bool hood_open = Car.contact.input4;
            bool hood_alarm = Car.contact.hood;
            if (white && hood_alarm)
            {
                hood_alarm = false;
                hood_open = true;
            }
            setModeOpen(2, "hood", !white, hood_open, hood_alarm, doors4);

            bool trunk_open = Car.contact.input2;
            bool trunk_alarm = Car.contact.trunk;
            if (white && trunk_alarm)
            {
                trunk_alarm = false;
                trunk_open = true;
            }
            setModeOpen(3, "trunk", !white, trunk_open, trunk_alarm, doors4);

            bool az = Car.az;
            if (az)
            {
                setLayer(4, "engine1", !white);
            }
            else
            {
                String ignition_id = null;
                if (!az && (Car.contact.input3 || Car.contact.realIgnition))
                    ignition_id = guard ? "ignition_red" : (white ? "ignition_blue" : "ignition");
                setLayer(4, ignition_id);
            }

            String state = null;
            if (guard)
            {
                state = white ? "lock_blue" : "lock_white";
                if (card)
                    state = "lock_red";
            }
            if (guard0 && !guard1)
                state = "valet";
            if (!guard0 && guard1)
                state = "block";
            setLayer(5, state);
        }

        private void setLayer(int n, String value)
        {
            if (levels[n] == value)
                return;
            levels[n] = value;
            NotifyPropertyChanged("CarLevel" + n);
        }

        private void setLayer(int n, String name, bool white)
        {
            if (!white)
                name += "_blue";
            setLayer(n, name);
        }

        private void setLayer(int n)
        {
            setLayer(n, null);
        }

        private void setModeCar(bool guard, bool alarm, bool doors4)
        {
            String pos = guard ? "car_blue" : "car_white";
            if (alarm)
                pos = "car_red";
            if (doors4)
                pos += "4";
            setLayer(0, pos);
        }

        private void setModeOpen(int pos, String group, bool guard, bool open, bool alarm, bool doors4)
        {
            if (alarm)
            {
                group += "_red";
            }
            else if (guard)
            {
                group += "_blue";
            }
            else
            {
                group += "_white";
            }
            if (open || alarm)
                group += "_open";
            if (doors4)
                group += "4";
            setLayer(pos, group);
        }
    }

}
