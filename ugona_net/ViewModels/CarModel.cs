using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Reflection;
using Microsoft.Phone.Info;
using System.Collections.Generic;

namespace ugona_net
{
    public class CarModel : INotifyPropertyChanged
    {

        public CarModel()
        {
            currentDate = DateTime.Now;
            LoadData();
        }

        public String Error
        {
            get;
            private set;
        }
        
        public bool Refresh
        {
            get;
            private set;
        }

        public String EventTime
        {
            get
            {
                return DateUtils.formatDateTime(Car.time);
            }
        }

        public Brush EventColor
        {
            get
            {
                long delta = DateUtils.Now - Car.time;
                if ((delta / 60000) < Car.timer + 1)
                    return Colors.BlueBrush;
                return (Brush)App.Current.Resources["PhoneForegroundBrush"];
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
                    return Colors.ErrorBrush;
                return (Brush) App.Current.Resources["PhoneForegroundBrush"];
            }
        }

        public Visibility MainVoltageVisibilty
        {
            get
            {
                return (Car.voltage.main == null) ? Visibility.Collapsed : Visibility.Visible;
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

        public Brush ReservedVoltageColor
        {
            get
            {
                if (!Car.contact.reservePowerNormal)
                    return Colors.ErrorBrush;
                return (Brush)App.Current.Resources["PhoneForegroundBrush"];
            }
        }

        public Visibility ReservedVoltageVisibilty
        {
            get
            {
                return (Car.voltage.reserved == null) ? Visibility.Collapsed : Visibility.Visible;
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

        public Brush BalanceColor
        {
            get
            {
                if ((Car.balance.value != null) && (Car.balance.value < 50))
                    return Colors.ErrorBrush;
                return (Brush)App.Current.Resources["PhoneForegroundBrush"];
            }
        }

        public Visibility BalanceVisibilty
        {
            get
            {
                return (Car.balance.value == null) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public String Temperature
        {
            get
            {
                if (Car.temperature.Count == 0)
                    return "";
                return String.Format("{0} ºC", Car.temperature[0].value + Car.TempShift);
            }
        }

        public Visibility TemperatureVisibilty
        {
            get
            {
                return (Car.temperature.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
            }
        }


        public String GsmLevel
        {
            get
            {
                if (Car.gsm.db == null)
                    return "";
                return String.Format("{0:d} dBm", Car.gsm.db);
            }
        }

        public String GsmLevelImage
        {
            get
            {
                if (Car.gsm.db == null)
                    return null;
                if (Car.gsm.db > -51)
                    return "/Assets/Icons/gsm_level5.png";
                if (Car.gsm.db > -65)
                    return "/Assets/Icons/gsm_level4.png";
                if (Car.gsm.db > -77)
                    return "/Assets/Icons/gsm_level3.png";
                if (Car.gsm.db > -91)
                    return "/Assets/Icons/gsm_level2.png";
                if (Car.gsm.db > -105)
                    return "/Assets/Icons/gsm_level1.png";
                return "/Assets/Icons/gsm_level0.png";
            }
        }

        public Visibility GsmLevelVisibility
        {
            get
            {
                return (Car.gsm.db == null) ? Visibility.Collapsed : Visibility.Visible;
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

        static Regex number_match = new Regex("^[0-9]+ ?");

        public String GoogleText
        {
            get
            {
                return GetPrefix("Google") + "Google";
            }
        }

        public String YandexText
        {
            get
            {
                return GetPrefix("Yandex") + "Yandex";
            }
        }

        public String BingText
        {
            get
            {
                return GetPrefix("Bing") + "Bing";
            }
        }

        public String OsmText
        {
            get
            {
                return GetPrefix("OSM") + "OSM";
            }
        }

        public String MapType
        {
            get
            {
                String res = Helper.GetSetting("MapType");
                if (res == null)
                    res = "OSM";
                return res;
            }

            set
            {
                Helper.PutSetting("MapType", value);
                NotifyPropertyChanged("GoogleText");
                NotifyPropertyChanged("YandexText");
                NotifyPropertyChanged("BingText");
                NotifyPropertyChanged("OsmText");
            }
        }

        public bool Traffic
        {
            get
            {
                return Helper.GetSetting<bool>("Traffic", false);
            }

            set
            {
                Helper.PutSetting("Traffic", value);
                NotifyPropertyChanged("TrafficText");
            }
        }

        public String TrafficText
        {
            get
            {
                String res = Helper.GetString("Show traffic");
                if (Traffic)
                    return "\u221A\xA0" + res;
                return res;
            }
        }

        public String PositionText
        {
            get
            {
                return Helper.GetString("My position");
            }
        }

        public String CenterText
        {
            get
            {
                return Helper.GetString("Center");
            }
        }

        String GetPrefix(String type)
        {
            if (MapType == type) 
                return "\u221A\xA0";
            return "";
        }

        DateTime currentDate;

        public DateTime Date
        {
            get
            {
                if (currentDate == null)
                    currentDate = DateTime.Now;
                return currentDate;
            }

            set
            {
                if ((currentDate.Day == value.Day) && (currentDate.Month == value.Month) && (currentDate.Year == value.Year))
                    return;
                currentDate = value;
                NotifyPropertyChanged("Date");
            }
        }

        public int? Course
        {
            get {
                return Car.gps.course;
            }
        }

        public double? Latitude
        {
            get
            {
                return latitude;
            }
        }

        public double? Longitude
        {
            get
            {
                return longitude;
            }
        }

        public String Address
        {
            get
            {
                String res = "";
                if (Car.last_stand > 0)
                    res = DateUtils.formatDateTime(Car.last_stand);
                if ((Car.last_stand < 0) && (Car.gps.speed != null))
                    res = String.Format("{0:n0} ", Car.gps.speed) + Helper.GetString("km/h");
                if (res.Length > 0)
                    res += " ";
                res += "|";
                if ((Car.gps.latitude != null) && (Car.gps.longitude != null))
                {
                    res += " " + String.Format("{0:n4}", Car.gps.latitude);
                    res += " " + String.Format("{0:n4}", Car.gps.longitude);
                }
                res += "|\n";
                if (address != null)
                {
                    string[] separators = new string[] { ", " };
                    String[] parts = address.Split(separators, StringSplitOptions.None);
                    int start = 2;
                    res += parts[0];
                    if (parts.Length > 1)
                        res += ", " + parts[1];
                    if ((parts.Length > 2) && number_match.IsMatch(parts[2]))
                    {
                        res += ", " + parts[2].Replace(" ", "\xA0");
                        start++;
                    }
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

            public bool reservePowerNormal
            {
                get;
                set;
            }

            public bool relay1
            {
                get;
                set;
            }

            public bool relay2
            {
                get;
                set;
            }

            public bool relay3
            {
                get;
                set;
            }

            public bool relay4
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

        public class GsmData
        {
            public int cc
            {
                get;
                set;
            }

            public int nc
            {
                get;
                set;
            }

            public int lac
            {
                get;
                set;
            }

            public int cid
            {
                get;
                set;
            }

            public int? db
            {
                get;
                set;
            }
        }

        public class Commands
        {
            public Commands()
            {
                valet_cmd = true;
            }

            public bool call
            {
                get;
                set;
            }

            public bool search
            {
                get;
                set;
            }

            public bool valet_cmd 
            {
                get;
                set;
            }

            public bool autostart
            {
                get;
                set;
            }

            public bool rele 
            {
                get;
                set;
            }

            public bool silent_mode
            {
                get;
                set;
            }

            public bool rele1
            {
                get;
                set;
            }

            public bool rele1i
            {
                get;
                set;
            }

            public bool rele2
            {
                get;
                set;
            }

            public bool rele2i
            {
                get;
                set;
            }

            public String rele1_name
            {
                get;
                set;
            }

            public String rele1i_name
            {
                get;
                set;
            }

            public String rele2_name
            {
                get;
                set;
            }

            public String rele2i_name
            {
                get;
                set;
            }
        }

        public class TemperatureData
        {
            public int sensor
            {
                get;
                set;
            }

            public int value
            {
                get;
                set;
            }
        }

        public class TemperaturesData : List<TemperatureData>
        {
            public void FromJson(JObject obj)
            {
                Clear();
                foreach (KeyValuePair<string, JToken> item in obj){
                    try
                    {
                        TemperatureData data = new TemperatureData();
                        data.sensor = Int16.Parse(item.Key);
                        data.value = item.Value.ToObject<Int16>();
                        Add(data);
                    }
                    catch (Exception)
                    {
                    }
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

            public long timer
            {
                get;
                set;
            }

            public long settings
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

            public GsmData gsm
            {
                get
                {
                    if (gsm_data == null)
                    {
                        gsm_data = new GsmData();
                    }
                    return gsm_data;
                }
            }

            public Commands commands
            {
                get
                {
                    if (commands_ == null)
                        commands_ = new Commands();
                    return commands_;
                }
            }

            public int[] settings_values
            {
                get
                {
                    if (settings_ == null)
                        settings_ = new int[22];
                    return settings_;
                }
            }

            public String api_key
            {
                get;
                set;
            }

            public String auth
            {
                get;
                set;
            }

            public String phone
            {
                get;
                set;
            }

            public bool silent_mode
            {
                get
                {
                    return false;
                }
            }

            public double TempShift
            {
                get;
                set;
            }

            public double VoltageShift
            {
                get;
                set;
            }

            public TemperaturesData temperature
            {
                get
                {
                    if (temperature_data == null)
                        temperature_data = new TemperaturesData();
                    return temperature_data;
                }
            }

            TemperaturesData temperature_data;
            VoltageData voltage_data;
            BalanceData balance_data;
            ContactData contact_data;
            GpsData gps_data;
            GsmData gsm_data;
            Commands commands_;
            int[] settings_;

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

        public CarData Car
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
            {
                NotifyPropertyChanged("EventTime");
                NotifyPropertyChanged("EventColor");
            }
            if (args.PropertyName == "timer")
                NotifyPropertyChanged("EventColor");
            if (args.PropertyName == "voltage.main")
            {
                NotifyPropertyChanged("MainVoltage");
                NotifyPropertyChanged("MainVoltageColor");
                NotifyPropertyChanged("MainVoltageVisibilty");
            }
            if (args.PropertyName == "voltage.reserved")
            {
                NotifyPropertyChanged("ReservedVoltage");
                NotifyPropertyChanged("ReservedVoltageVisibilty");
            }
            if (args.PropertyName == "contact.reservePowerNormal")
                NotifyPropertyChanged("ReservedVoltageColor");
            if (args.PropertyName == "balance.value")
            {
                NotifyPropertyChanged("Balance");
                NotifyPropertyChanged("BalanceColor");
                NotifyPropertyChanged("BalanceVisibilty");
            }
            if (args.PropertyName == "gsm.db")
            {
                NotifyPropertyChanged("GsmLevel");
                NotifyPropertyChanged("GsmLevelImage");
                NotifyPropertyChanged("GsmLevelVisibility");
            }
            if (args.PropertyName == "temperature")
            {
                NotifyPropertyChanged("Temperature");
                NotifyPropertyChanged("TemperatureVisibilty");
            }
        }

        private void LoadData()
        {
            String data = Helper.GetSetting(Names.CAR_DATA);
            if (data != null)
            {
                try
                {
                    car_data = JsonConvert.DeserializeObject<CarData>(data);
                }
                catch (Exception)
                {
                    car_data = new CarData();
                }
                car_data.PropertyChanged += CarPropertyChanged;
            }
            UpdateAddress();
            UpdateLevels();
        }

        public void ClearData()
        {
            Helper.GetSetting(Names.CAR_DATA, null);
            car_data = new CarData();
            car_data.PropertyChanged += CarPropertyChanged;
            latitude = null;
            longitude = null;
            NotifyPropertyChanged("Address");
            Helper.PutSetting(Names.CAR_DATA, JsonConvert.SerializeObject(Car));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        async public void refresh()
        {
            if (Refresh)
                return;
            Refresh = true;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.Text = Helper.GetString("Refresh data");
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;
            try
            {
                await doRefresh();
            }
            catch (Exception)
            {
            }
            try
            {
                if (SystemTray.ProgressIndicator != null)
                {
                    SystemTray.ProgressIndicator.IsIndeterminate = false;
                    SystemTray.ProgressIndicator.IsVisible = false;
                }
            }
            catch (Exception)
            {
            }
            Refresh = false;
        }

        public void Flush()
        {
            Helper.PutSetting(Names.CAR_DATA, JsonConvert.SerializeObject(Car));
            Helper.Flush();
        }

        async public Task<bool> doRefresh()
        {
            try
            {
                long time = Car.time;
                JObject res = await Helper.GetApi("", "skey", Car.api_key, "time", Car.time);           
                Helper.SetData(Car, res);
                Error = "";
                NotifyPropertyChanged("Error");
                if (time == Car.time)
                    return false;
                UpdateLevels();
                UpdateAddress();
                Helper.PutSetting(Names.CAR_DATA, JsonConvert.SerializeObject(Car));
                return true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                NotifyPropertyChanged("Error");
            }
            return false;
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
            double? d = AddressHelper.distance(Car.gps.latitude, Car.gps.longitude, latitude, longitude);
            if ((d != null) && (d < 50))
                return;
            if (d > 300)
                SetAddress(null);
            try
            {
                String res = await AddressHelper.get(Car.gps.latitude, Car.gps.longitude);
                SetAddress(res);
                latitude = Car.gps.latitude;
                longitude = Car.gps.longitude;
            }
            catch (Exception)
            {

            }
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
            DateTime time = DateUtils.ToDateTime(Car.time);
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

        public long RegisterTime
        {
            get
            {
                return Helper.GetSetting<long>(Names.CHANNEL_TIME, 0);
            }

            set
            {
                Helper.PutSetting(Names.CHANNEL_TIME, value);
                RegisterGCM();
            }
        }

        public String ChannelUri
        {
            get
            {
                return Helper.GetSetting(Names.CHANNEL);
            }

            set
            {
                if (value == ChannelUri)
                    return;
                Helper.PutSetting(Names.CHANNEL, value);
                RegisterTime = 0;

            }
        }

        static String gcm_version;

        public async void RegisterGCM()
        {
            if (ChannelUri == null)
                return;
            if ((Car.api_key == null) || (Car.api_key == "demo"))
                return;
            long now = DateUtils.ToJavaTime(DateTime.Now);

            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            String version = nameHelper.Version.ToString();
            if (gcm_version == null)
                gcm_version = Helper.GetSetting(Names.GCM_VERSION, "");

            if ((now < RegisterTime + 86400000) && (version == gcm_version))
                return;

            JObject data = new JObject();
            data.Add("reg", ChannelUri);
            data.Add("tz", TimeZoneInfo.Local.StandardName);
            data.Add("version", version);
            data.Add("uid", DeviceId());
            data.Add("lang", Helper.GetString("ResourceLanguage"));
            data.Add("cars", Car.api_key + ";;" + Car.phone + ";" + Car.auth);
            data.Add("os", Environment.OSVersion.ToString());
            data.Add("model", DeviceStatus.DeviceManufacturer + " " + DeviceStatus.DeviceName);
            data.Add("type", 1);
            try
            {
                await Helper.PostData("https://car-online.ugona.net/reg", data.ToString());
                Helper.PutSetting(Names.GCM_VERSION, version);
                App.ViewModel.RegisterTime = now;
                Helper.Flush();
            }
            catch (Exception)
            {
            }
        }

        String DeviceId()
        {
            byte[] myDeviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
            return Convert.ToBase64String(myDeviceID);
        }
        
    }

}
