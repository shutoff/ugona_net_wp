using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Newtonsoft.Json.Linq;

namespace ugona_net
{
    public partial class Command : PhoneApplicationPage
    {
        public Command()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CommandTitle.Text = Helper.GetString(command);
        }

        private void TextChanged(object sender, RoutedEventArgs e)
        {
            bool enabled = CCode.Text.Length == 6;
            Send.IsEnabled = enabled;
            if (sms_ccode)
                SendSms.IsEnabled = enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            command = PhoneApplicationService.Current.State["Command"].ToString();
            if (command == "valet_on")
            {
                net_cmd = 1793;
                sms = "VALET";
                sms_ccode = true;
            }
            if (command == "valet_off")
            {
                net_cmd = 1794;
                sms = "INIT";
                sms_ccode = true;
            }
            if (command == "motor_on")
            {
                net_cmd = 768;
                sms = "MOTOR ON";
            }
            if (command == "motor_off")
            {
                net_cmd = 769;
                sms = "MOTOR OFF";
            }
            if (command == "heater_on")
            {
                net_cmd = 258;
                sms = "HEAT 2";
            }
            if (command == "heater_air")
            {
                net_cmd = 514;
                sms = "HEAT 1";
            }
            if (command == "air")
            {
                net_cmd = 256;
                sms = "HEAT 3";
            }
            if (command == "heater_off")
            {
                net_cmd = 257;
                sms = "HEAT 0";
            }
            if (command == "rele1_on")
            {
                net_cmd = 256;
                sms = "REL1 LOCK";
            }
            if (command == "rele1_off")
            {
                net_cmd = 257;
                sms = "REL1 UNLOCK";
            }
            if (command == "rele1i")
            {
                net_cmd = 258;
                sms = "REL1 IMPULS";
            }
            if (command == "rele2_on")
            {
                net_cmd = 512;
                sms = "REL2 LOCK";
            }
            if (command == "rele2_off")
            {
                net_cmd = 513;
                sms = "REL2 UNLOCK";
            }
            if (command == "rele2i")
            {
                net_cmd = 514;
                sms = "REL2 IMPULS";
            }
            if (sms_ccode)
                SendSms.IsEnabled = false;
        }

        async private void SendClick(object sender, RoutedEventArgs e)
        {
            Send.IsEnabled = false;
            Progress.Visibility = Visibility.Visible;
            Status.Text = Helper.GetString("send_command");
            try
            {
                String skey = App.ViewModel.Car.api_key;
                String auth = App.ViewModel.Car.auth;
                JObject obj = await Helper.GetApi("command", "skey", skey, "cmd", net_cmd, "auth", auth);
                String text = Helper.GetString("wait_msg");
                text = text.Replace("$1", App.ViewModel.Car.timer + "");
                Status.Text = text;
            }
            catch (Exception ex)
            {
                Status.Text = Helper.GetString("send_sms_on_fail");
            }
            Progress.Visibility = Visibility.Collapsed;
            Send.IsEnabled = true;
        }

        private void SendSmsClick(object sender, RoutedEventArgs e)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.To = App.ViewModel.Car.phone;
            if (sms_ccode)
            {
                smsComposeTask.Body = CCode.Text + " " + sms;
            }
            else
            {
                smsComposeTask.Body = sms;
            }
            smsComposeTask.Show();
            NavigationService.GoBack();
        }

        String command;
        String sms;
        int net_cmd;
        bool sms_ccode;
    }
}