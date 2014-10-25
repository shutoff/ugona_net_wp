using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace ugona_net
{
    class SettingsItem : INotifyPropertyChanged
    {
        SettingsItem(String title, String info, OnClick click)
        {
            title_ = title;
            info_ = info;
            on_click = click;
        }

        public SettingsItem(String title, String info)
        {
            title_ = title;
            info_ = info;
        }

        protected SettingsItem(String title)
        {
            title_ = title;
        }

        protected String title_;
        protected String info_;

        public String Title
        {
            get
            {
                return Helper.GetString(title_);
            }
        }

        public Brush TitleColor
        {
            get
            {
                if (updated)
                    return Colors.UpdatedBrush;
                return (Brush)App.Current.Resources["PhoneForegroundBrush"];
            }
        }

        public virtual bool updated
        {
            get
            {
                return false;
            }
        }

        public String Info
        {
            get
            {
                if (info_ == null)
                    return null;
                return Helper.GetString(info_);
            }
        }

        virtual public String Units
        {
            get
            {
                return "";
            }
        }

        virtual public Visibility IsInfo
        {
            get
            {
                return (Info == null) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        virtual public Visibility IsList
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        virtual public Visibility IsCheckbox
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        virtual public IEnumerable<String> Items
        {
            get
            {
                return null;
            }
        }

        virtual public Object SelectedItem
        {
            get
            {
                return null;
            }

            set
            {

            }
        }

        public Visibility IsEdit
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        virtual public bool IsChecked
        {
            get
            {
                return false;
            }

            set
            {

            }
        }

        virtual public Visibility IsSlider
        {
            get
            {
                return Visibility.Collapsed;
            }
        }

        virtual public int Value
        {
            get
            {
                return 0;
            }

            set
            {

            }
        }

        virtual public int MinValue
        {
            get
            {
                return 0;
            }
        }

        virtual public int MaxValue
        {
            get
            {
                return 0;
            }
        }

        virtual public int TitleSize
        {
            get
            {
                return 30;
            }
        }

        public SettingsItem Item
        {
            get
            {
                return this;
            }
        }

        protected delegate void OnClick(PhoneApplicationPage page);
        protected OnClick on_click;

        public void click(PhoneApplicationPage page)
        {
            if (on_click != null)
                on_click(page);
        }

        public virtual void editClick()
        {

        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        static List<SettingsItem> auth_items;
        static List<SettingsItem> commands_items;

        public static List<SettingsItem> AuthItems
        {
            get
            {
                if (auth_items == null)
                {
                    auth_items = new List<SettingsItem>();
                    auth_items.Add(new SettingsItem("auth", "auth_summary", delegate(PhoneApplicationPage page)
                    {
                        page.NavigationService.Navigate(new Uri("/AuthPage.xaml?parameter=nodemo", UriKind.Relative));
                    }));
                }
                return auth_items;
            }
        }

        public static List<SettingsItem> CommandsItems
        {
            get
            {
                if (commands_items == null)
                {
                    commands_items = new List<SettingsItem>();
                    commands_items.Add(new CommandItem("call"));
                    commands_items.Add(new CommandItem("search"));
                    commands_items.Add(new CommandItem("valet_cmd"));
                    commands_items.Add(new CommandItem("autostart"));
                    commands_items.Add(new CommandItem("rele"));
                    commands_items.Add(new CommandItem("silent_mode"));
                    commands_items.Add(new CommandEditItem("rele1"));
                    commands_items.Add(new CommandEditItem("rele1i"));
                    commands_items.Add(new CommandEditItem("rele2"));
                    commands_items.Add(new CommandEditItem("rele2i"));
                }
                return commands_items;
            }
        }


    }

    class CommandItem : SettingsItem
    {
        public CommandItem(String title)
            : base(title)
        {
            on_click = onclick;
        }

        override public Visibility IsCheckbox
        {
            get
            {
                return Visibility.Visible;
            }
        }

        override public int TitleSize
        {
            get
            {
                return 24;
            }
        }

        override public bool IsChecked
        {
            get
            {
                Object o = App.ViewModel.Car.commands;
                Type type = o.GetType();
                PropertyInfo pInfo = type.GetProperty(title_);
                if (pInfo == null)
                    return false;
                return (bool)pInfo.GetValue(o, null);
            }

            set
            {
                Object o = App.ViewModel.Car.commands;
                Type type = o.GetType();
                PropertyInfo pInfo = type.GetProperty(title_);
                if (pInfo == null)
                    return;
                pInfo.SetValue(o, value);
                NotifyPropertyChanged("IsChecked");
                App.ViewModel.NotifyPropertyChanged("commands." + title_);
            }
        }

        void onclick(PhoneApplicationPage page)
        {
            IsChecked = !IsChecked;
        }
    }

    class CommandEditItem : CommandItem
    {
        public CommandEditItem(String title)
            :base(title)
        {

        }

        new public Visibility IsEdit
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public override void editClick()
        {
            String info = Info;
            InputPrompt input = new InputPrompt();
            input.Title = Helper.GetString(title_);
            input.Message = Helper.GetString("command_name");
            input.Completed += edit_Completed;
            if (info != null)
                input.Value = info;
            input.Show();
        }

        private void edit_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok)
            {
                String res = e.Result;
                if (res == "")
                    res = null;
                Info = res;
            }
        }

        public new String Info
        {
            get
            {
                Object o = App.ViewModel.Car.commands;
                Type type = o.GetType();
                PropertyInfo pInfo = type.GetProperty(title_ + "_name");
                if (pInfo == null)
                    return null;
                String info = (String)pInfo.GetValue(o, null);
                if (info == Helper.GetString(title_))
                    return null;
                return info;
            }
            
            set{
                Object o = App.ViewModel.Car.commands;
                Type type = o.GetType();
                PropertyInfo pInfo = type.GetProperty(title_ + "_name");
                if (pInfo == null)
                    return;
                pInfo.SetValue(o, value);
                NotifyPropertyChanged("Info");
                NotifyPropertyChanged("IsInfo");
            }
        }

        public new Visibility IsInfo
        {
            get
            {
                return (Info == null) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        
    }

    class SeekBarItem : SettingsItem
    {
        public SeekBarItem(String title, int min_, int max_, String units_)
            : base(title)
        {
            units = units_;
            min = min_;
            max = max_;
            k = 1;
        }

        public SeekBarItem(String title, int min_, int max_, String units_, double k_)
            : base(title)
        {
            units = units_;
            min = min_;
            max = max_;
            k = k_;
        }

        public override Visibility IsSlider
        {
            get
            {
                return Visibility.Visible;
            }
        }

        public override int MaxValue
        {
            get
            {
                return max - min;
            }
        }

        public override String Units
        {
            get
            {
                return ((Value + min) * k) + " " + Helper.GetString(units);
            }
        }

        String units;
        protected int min;
        protected int max;
        protected double k;
    }

}
