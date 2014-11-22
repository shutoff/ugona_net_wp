using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace ugona_net
{
    class DoAction
    {

        static ObservableCollection<DoAction> msActions;

        DoAction(string icon_, String name_){
            icon = icon_;
            name = name_;
        }

        public String Name
        {
            get
            {
                return Helper.GetString(name);
            }
        }

        public String Icon
        {
            get
            {
                return "/Assets/Icons/" + icon + ".png";
            }
        }

        public string name
        {
            get;
            set;
        }

        string icon
        {
            get;
            set;
        }

        static public ObservableCollection<DoAction> MsActions
        {
            get
            {
                if (msActions == null)
                {
                    msActions = new ObservableCollection<DoAction>();
                    msActions.Add(new DoAction("icon_phone", "call"));
                    msActions.Add(new DoAction("icon_valet_on", "valet_on"));
                    msActions.Add(new DoAction("icon_valet_off", "valet_off"));
                    msActions.Add(new DoAction("icon_motor_on", "motor_on"));
                    msActions.Add(new DoAction("icon_motor_off", "motor_off"));
                    /*
                    msActions.Add(new DoAction("icon_heater", "heater_on"));
                    msActions.Add(new DoAction("icon_heater_air", "heater_air"));
                    msActions.Add(new DoAction("icon_air", "air"));
                    msActions.Add(new DoAction("icon_heater", "heater_off"));
                     */
                    msActions.Add(new DoAction("rele1_on", "rele1_on"));
                    msActions.Add(new DoAction("rele1_off", "rele1_off"));
                    msActions.Add(new DoAction("rele1_impulse", "rele1i"));
                    msActions.Add(new DoAction("rele2_on", "rele2_on"));
                    msActions.Add(new DoAction("rele2_off", "rele2_off"));
                    msActions.Add(new DoAction("rele2_impulse", "rele2i"));
                    msActions.Add(new DoAction("icon_status", "status_title"));
                    msActions.Add(new DoAction("icon_block", "block"));
                    msActions.Add(new DoAction("icon_turbo_on", "turbo_on"));
                    msActions.Add(new DoAction("icon_turbo_off", "turbo_off"));
                    msActions.Add(new DoAction("icon_internet_on", "internet_on"));
                    msActions.Add(new DoAction("icon_internet_off", "internet_off"));
                    msActions.Add(new DoAction("icon_status", "map_req"));
                    msActions.Add(new DoAction("balance", "balance"));
                    msActions.Add(new DoAction("icon_reset", "reset"));
                }
                return msActions;
            }
        }

        public static void Run(String cmd, NavigationService navigationService)
        {
            if (cmd == "call")
            {
                PhoneCallTask phoneCallTask = new PhoneCallTask();
                phoneCallTask.PhoneNumber = App.ViewModel.Car.phone;
                phoneCallTask.Show();
                return;
            }
            String sms = null;
            if (cmd == "turbo_on")
            {
                sms = "TURBO ON";
            }
            if (cmd == "turbo_off")
            {
                sms = "TURBO OFF";
            }
            if (cmd == "internet_on")
            {
                sms = "INTERNET ALL";
            }
            if (cmd == "internet_off")
            {
                sms = "INTERNET OFF";
            }
            if (cmd == "map_req")
            {
                sms = "MAP";
            }
            if (cmd == "balance")
            {
                sms = "BALANCE?";
            }
            if (cmd == "reset")
            {
                sms = "RESET";
            }
            if (cmd == "status_title")
            {
                sms = "STATUS?";
            }
            if (cmd == "block")
            {
                sms = "BLOCK MTR";
            }
            if (sms != null)
            {
                SmsComposeTask smsComposeTask = new SmsComposeTask();
                smsComposeTask.To = App.ViewModel.Car.phone;
                smsComposeTask.Body = sms;
                smsComposeTask.Show();
                return;
            }
            PhoneApplicationService.Current.State["Command"] = cmd;
            navigationService.Navigate(new Uri("/Command.xaml", UriKind.Relative));
        }
    }
}
