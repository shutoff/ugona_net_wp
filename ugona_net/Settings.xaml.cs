using Microsoft.Phone.Controls;
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
        static int[] values;
        static int[] old_values;

        PivotItem az_item;
        bool az_visible;

        public Settings()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            Auth.ItemsSource = SettingsItem.AuthItems;
            Commands.ItemsSource = SettingsItem.CommandsItems;
            values = new int[22];
            old_values = new int[22];
            az_item = Pivot.Items[3] as PivotItem;
            Pivot.Items.RemoveAt(3);
            SetupAz();
            App.ViewModel.PropertyChanged += CarPropertyChanged;
        }

        private void CarPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "commands.autostart")
                SetupAz();
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
            App.ViewModel.Flush();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadSettings();
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
                    values[i] = v.ToObject<Int32>();
                    old_values[i] = values[i];
                }
                res = await Helper.GetApi("version",
                        "skey", App.ViewModel.Car.api_key);
                String version = res.GetValue("version").ToString();

                Loading.Visibility = Visibility.Collapsed;
                SetupLabel.Text = Helper.GetString("set");

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
                items.Add(new SettingsItem("version", version));

                DeviceSettings.ItemsSource = items;

                items = new List<SettingsItem>();

                items.Add(new ListItem("start_time", 20));
                items.Add(new SeekBarBitItem("voltage_limit", 18, 60, 93, "v", 0.13));
                items.Add(new CheckBitItem("soft_start", 19, 1));
                AzSettings.ItemsSource = items;
            }
            catch (Exception ex)
            {
                Loading.Visibility = Visibility.Collapsed;
                SetupLabel.Text = Helper.GetString("refresh");
                DeviceError.Visibility = Visibility.Visible;
                DeviceError.Text = ex.Message;
                AzError.Visibility = Visibility.Visible;
                AzError.Text = ex.Message;
            }
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

            virtual public Object SelectedItem
            {
                get
                {
                    IEnumerable<String> i = Items;
                    return items[values[word]];
                }

                set
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] == value)
                        {
                            values[word] = i;
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
                    return values[word] != old_values[word];
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
                    double v = (double) info.GetValue(App.ViewModel.Car);
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
                    int res = values[word];
                    if (res < min)
                        res = min;
                    if (res > max)
                        res = max;
                    return res - min;
                }
                set
                {
                    values[word] = value + min;
                    NotifyPropertyChanged("Units");
                    NotifyPropertyChanged("TitleColor");
                }
            }

            public override bool updated
            {
                get
                {
                    return values[word] != old_values[word];
                }
            }

            int word;
        }

        class SeekBarListItem : SeekBarWordItem
        {
            public SeekBarListItem(String title, int word_)
                :base(title, word_, 0, 0, "")
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
                    if ((values[word] & mask) == 0)
                        return 0;
                    return base.Value;
                }
                set
                {
                    values[word] &= ~mask;
                    if (value > 0)
                    {
                        values[word] |= mask;
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
                    if (((values[word] & mask) == 0) && ((old_values[word] & mask) == 0))
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
                    return (values[word] & mask) != 0;
                }

                set
                {
                    values[word] &= ~mask;
                    if (value)
                        values[word] |= mask;
                    NotifyPropertyChanged("IsChecked");
                    NotifyPropertyChanged("TitleColor");
                }
            }

            public override bool updated
            {
                get
                {
                    return (values[word] & mask) != (old_values[word] & mask);
                }
            }

            int word;
            int mask;
        }
    }
}