using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ugona_net
{
    public partial class Settings : PhoneApplicationPage
    {
        static State state;

        PivotItem az_item;
        PivotItem heater_item;
        bool az_visible;
        bool heater_visible;
        bool show_map;

        public Settings()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            Auth.ItemsSource = SettingsItem.AuthItems;
            Commands.ItemsSource = SettingsItem.CommandsItems;
            state = new State();
            az_item = Pivot.Items[3] as PivotItem;
            Pivot.Items.RemoveAt(3);
            heater_item = Pivot.Items[3] as PivotItem;
            Pivot.Items.RemoveAt(3);
            SetupAz();
            SetupHeater();
            App.ViewModel.PropertyChanged += CarPropertyChanged;
        }

        private void CarPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "commands.autostart")
                SetupAz();
            if (args.PropertyName == "commands.rele")
                SetupHeater();
        }

        void SetupAz()
        {
            if (App.ViewModel.Car.commands.autostart == az_visible)
                return;
            az_visible = App.ViewModel.Car.commands.autostart;
            if (az_visible)
            {
                Pivot.Items.Insert(3, az_item);
            }
            else
            {
                Pivot.Items.RemoveAt(3);
            }
        }

        void SetupHeater()
        {
            if (App.ViewModel.Car.commands.rele == heater_visible)
                return;
            heater_visible = App.ViewModel.Car.commands.rele;
            int pos = 3;
            if (App.ViewModel.Car.commands.autostart)
                pos++;
            if (heater_visible)
            {
                Pivot.Items.Insert(pos, heater_item);
            }
            else
            {
                Pivot.Items.RemoveAt(pos);
            }
        }

        private void ItemClick(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;
            SettingsItem iNew = list.SelectedItem as SettingsItem;
            if (iNew == null)
                return;
            list.SelectedIndex = -1;
            iNew.click(this);
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SettingsItem item = button.CommandParameter as SettingsItem;
            item.editClick();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (!show_map)
            {
                PhoneApplicationService.Current.State["Settings"] = null;
                PhoneApplicationService.Current.State["SettingsOld"] = null;
            }
            show_map = true;
            App.ViewModel.Flush();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (PhoneApplicationService.Current.State.ContainsKey("Settings"))
            {
                state = PhoneApplicationService.Current.State["Settings"] as State;
                RestoreState();
            }
            else
            {
                LoadSettings();
            }
            PivotItem item = Pivot.SelectedItem as PivotItem;
            if (item == null)
                return;
            OnItemSelected(item);
        }

        private void Pivot_LoadedPivotItem(object sender, PivotItemEventArgs e)
        {
            OnItemSelected(e.Item);
        }

        private void OnItemSelected(PivotItem item)
        {
            if ((item.Name == "AuthPage") || (item.Name == "ControlPage"))
            {
                Setup.Visibility = Visibility.Collapsed;
            }
            else
            {
                Setup.Visibility = Visibility.Visible;
            }
        }

        async void LoadSettings()
        {
            try
            {
                DeviceError.Visibility = Visibility.Collapsed;
                AzError.Visibility = Visibility.Collapsed;
                HeaterError.Visibility = Visibility.Collapsed;
                Loading.Visibility = Visibility.Visible;
                SetupLabel.Text = Helper.GetString("loading");
                JObject res = await Helper.GetApi("settings",
                                        "skey", App.ViewModel.Car.api_key,
                                        "auth", App.ViewModel.Car.auth);
                for (int i = 0; i < 22; i++)
                {
                    JToken v = res.GetValue("v" + i);
                    if (v == null)
                        continue;
                    state.values[i] = v.ToObject<Int32>();
                    state.old_values[i] = state.values[i];
                }
                res = await Helper.GetApi("version",
                        "skey", App.ViewModel.Car.api_key);
                state.version = res.GetValue("version").ToString();
                RestoreState();
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Collapsed;
                SetupLabel.Text = Helper.GetString("refresh");
                DeviceError.Visibility = Visibility.Visible;
                DeviceError.Text = ex.Message;
                AzError.Visibility = Visibility.Visible;
                AzError.Text = ex.Message;
                HeaterError.Visibility = Visibility.Visible;
                HeaterError.Text = ex.Message;
            }
        }

        void RestoreState()
        {
            Loading.Visibility = Visibility.Collapsed;
            SetupLabel.Text = Helper.GetString("setup");

            List<SettingsItem> items = new List<SettingsItem>();

            items.Add(new SeekBarWordItem("timer", 21, 1, 30, "minutes"));
            items.Add(new CheckBitItem("guard_sound", 0, 7));
            items.Add(new CheckBitItem("no_sound", 12, 3));
            items.Add(new CheckBitItem("guard_partial", 2, 2));
            items.Add(new CheckBitItem("comfort_enable", 9, 5));
            items.Add(new SeekBarWordItem("guard_time", 4, 15, 60, "sec"));
            items.Add(new SeekBarWordItem("robbery_time", 5, 1, 30, "sec", 20));
            items.Add(new SeekBarWordItem("door_time", 10, 10, 30, "sec"));
            items.Add(new SeekBarWordItem("tilt_level", 1, 15, 45, "unit"));
            items.Add(new SeekBarListItem("shock_sens", 14));
            items.Add(new CheckBitItem("tilt_low", 2, 1));
            items.Add(new SeekBarPrefItem("temp_correct", "TempShift", -10, 10, "ºC", 1));
            items.Add(new SeekBarPrefItem("voltage_shift", "VoltageShift", -20, 20, "V", 0.05));
            items.Add(new SettingsItem("version", state.version));

            DeviceSettings.ItemsSource = items;

            items = new List<SettingsItem>();
            items.Add(new ListItem("start_time", 20));
            items.Add(new SeekBarBitItem("voltage_limit", 18, 60, 93, "v", 0.13));
            items.Add(new CheckBitItem("soft_start", 19, 1));
            items.Add(new TimerAddItem());
            AzSettings.ItemsSource = items;

            items = new List<SettingsItem>();
            HeaterSettings.ItemsSource = items;

            items = new List<SettingsItem>();
            items.Add(new ZoneAddItem(AddZone));
            ZoneSettings.ItemsSource = items;
        }

        void AddZone(PhoneApplicationPage page)
        {
            Zone zone = new Zone();
            double lat = (double)App.ViewModel.Car.gps.latitude;
            double lng = (double)App.ViewModel.Car.gps.longitude;
            zone.lat1 = lat - 0.01;
            zone.lat2 = lat + 0.01;
            zone.lng1 = lng - 0.01;
            zone.lng2 = lng + 0.01;
            zone.name = "Zone";
            EditZone(zone);
        }

        void EditZone(Zone zone)
        {
            show_map = true;
            PhoneApplicationService.Current.State["Settings"] = state;
            PhoneApplicationService.Current.State["MapEvent"] = null;
            PhoneApplicationService.Current.State["MapZone"] = zone;
            PhoneApplicationService.Current.State["MapTracks"] = null;
            NavigationService.Navigate(new Uri("/MapPage.xaml", UriKind.Relative));
        }

        new class State
        {
            public State()
            {
                values = new int[22];
                old_values = new int[22];
            }

            public int[] values;
            public int[] old_values;
            public String version;
        }

        class ListItem : SettingsItem
        {
            public ListItem(String title, int word_)
                : base(title)
            {
                word = word_;
            }

            override public Visibility IsList
            {
                get
                {
                    return Visibility.Visible;
                }
            }

            override public IEnumerable<String> Items
            {
                get
                {
                    if (items == null)
                    {
                        items = new List<String>();
                        for (int i = 0; ; i++)
                        {
                            String s = title_ + "_" + i;
                            String v = Helper.GetString(s);
                            if ((v == "") || (v == s))
                                break;
                            items.Add(v);
                        }
                    }
                    return items;
                }
            }

            new virtual public Object SelectedItem
            {
                get
                {
                    IEnumerable<String> i = Items;
                    return items[state.values[word]];
                }

                set
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] == value)
                        {
                            state.values[word] = i;
                            break;
                        }
                    }
                    NotifyPropertyChanged("TitleColor");
                }
            }

            public override bool updated
            {
                get
                {
                    return state.values[word] != state.old_values[word];
                }
            }

            List<String> items;
            int word;
        }

        class SeekBarPrefItem : SeekBarItem
        {
            public SeekBarPrefItem(String title, String name_, int min_, int max_, String units_, double k)
                : base(title, min_, max_, units_, k)
            {
                name = name_;
            }

            public override int Value
            {
                get
                {
                    Type type = App.ViewModel.Car.GetType();
                    PropertyInfo info = type.GetProperty(name);
                    double v = (double)info.GetValue(App.ViewModel.Car);
                    return (int)Math.Round(v / k) - min;
                }
                set
                {
                    double v = value;
                    v = (v + min) * k;
                    Type type = App.ViewModel.Car.GetType();
                    PropertyInfo info = type.GetProperty(name);
                    info.SetValue(App.ViewModel.Car, v);
                    NotifyPropertyChanged("Units");
                }
            }

            String name;
        }

        class SeekBarWordItem : SeekBarItem
        {
            public SeekBarWordItem(String title, int word_, int min_, int max_, String units_)
                : base(title, min_, max_, units_)
            {
                word = word_;
            }

            public SeekBarWordItem(String title, int word_, int min_, int max_, String units_, double k_)
                : base(title, min_, max_, units_, k_)
            {
                word = word_;
            }

            public override int Value
            {
                get
                {
                    int res = state.values[word];
                    if (res < min)
                        res = min;
                    if (res > max)
                        res = max;
                    return res - min;
                }
                set
                {
                    state.values[word] = value + min;
                    NotifyPropertyChanged("Units");
                    NotifyPropertyChanged("TitleColor");
                }
            }

            public override bool updated
            {
                get
                {
                    return state.values[word] != state.old_values[word];
                }
            }

            int word;
        }

        class SeekBarListItem : SeekBarWordItem
        {
            public SeekBarListItem(String title, int word_)
                : base(title, word_, 0, 0, "")
            {
                for (int i = 0; ; i++)
                {
                    String s = title_ + "_" + i;
                    String v = Helper.GetString(s);
                    if ((v == "") || (s == v))
                        break;
                    max = i;
                }
            }

            public override String Units
            {
                get
                {
                    return Helper.GetString(title_ + "_" + Value);
                }
            }
        }

        class SeekBarBitItem : SeekBarWordItem
        {
            public SeekBarBitItem(String title, int word_, int min_, int max_, String units_, double k)
                : base(title, word_, min_, max_, units_, k)
            {
                mask = 1 << 7;
                word = 12;
            }

            public override int Value
            {
                get
                {
                    if ((state.values[word] & mask) == 0)
                        return 0;
                    return base.Value;
                }
                set
                {
                    state.values[word] &= ~mask;
                    if (value > 0)
                    {
                        state.values[word] |= mask;
                        base.Value = value;
                    }
                    else
                    {
                        NotifyPropertyChanged("Units");
                    }
                }
            }

            public override String Units
            {
                get
                {
                    if (Value <= 0)
                        return Helper.GetString("no_start");
                    return base.Units;
                }
            }

            public override bool updated
            {
                get
                {
                    if (((state.values[word] & mask) == 0) && ((state.old_values[word] & mask) == 0))
                        return false;
                    return base.updated;
                }
            }

            int mask;
            int word;
        }

        class CheckBitItem : CommandItem
        {
            public CheckBitItem(String title, int word_, int bit)
                : base(title)
            {
                word = word_;
                mask = 1 << bit;
            }

            override public bool IsChecked
            {
                get
                {
                    return (state.values[word] & mask) != 0;
                }

                set
                {
                    state.values[word] &= ~mask;
                    if (value)
                        state.values[word] |= mask;
                    NotifyPropertyChanged("IsChecked");
                    NotifyPropertyChanged("TitleColor");
                }
            }

            public override bool updated
            {
                get
                {
                    return (state.values[word] & mask) != (state.old_values[word] & mask);
                }
            }

            int word;
            int mask;
        }

        class TimerAddItem : SettingsItem
        {
            public TimerAddItem()
                : base("add_timer")
            {

            }

            public override Visibility IsAdd
            {
                get
                {
                    return Visibility.Visible;
                }
            }
        }

        class ZoneAddItem : SettingsItem
        {
            public ZoneAddItem(SettingsItem.OnClick onclick)
                : base("add_zone")
            {
                on_click = onclick;
            }

            public override Visibility IsAdd
            {
                get
                {
                    return Visibility.Visible;
                }
            }


        }
    }
}