using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ugona_net
{
    class Action
    {

        static ObservableCollection<Action> msActions;

        Action(string icon_, String name_){
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

        string name
        {
            get;
            set;
        }

        string icon
        {
            get;
            set;
        }

        static public ObservableCollection<Action> MsActions
        {
            get
            {
                if (msActions == null)
                {
                    msActions = new ObservableCollection<Action>();
                    msActions.Add(new Action("icon_phone", "call"));
                    msActions.Add(new Action("icon_parking", "search"));
                    msActions.Add(new Action("icon_valet_on", "valet_on"));
                    msActions.Add(new Action("icon_valet_off", "valet_off"));
                    msActions.Add(new Action("icon_motor_on", "motor_on"));
                    msActions.Add(new Action("icon_motor_off", "motor_off"));
                    msActions.Add(new Action("icon_heater", "rele"));
                    msActions.Add(new Action("icon_heater", "heater_on"));
                    msActions.Add(new Action("icon_heater_air", "heater_air"));
                    msActions.Add(new Action("icon_air", "air"));
                    msActions.Add(new Action("icon_heater", "heater_off"));
                    msActions.Add(new Action("rele1_on", "rele1_on"));
                    msActions.Add(new Action("rele1_off", "rele1_off"));
                    msActions.Add(new Action("rele1_impulse", "rele1i"));
                    msActions.Add(new Action("rele2_on", "rele2_on"));
                    msActions.Add(new Action("rele2_off", "rele2_off"));
                    msActions.Add(new Action("rele2_impulse", "rele2i"));
                    msActions.Add(new Action("icon_status", "status_title"));
                    msActions.Add(new Action("icon_block", "block"));
                    msActions.Add(new Action("sound_off", "sound_off"));
                    msActions.Add(new Action("sound", "sound_on"));
                    msActions.Add(new Action("icon_turbo_on", "turbo_on"));
                    msActions.Add(new Action("icon_turbo_off", "turbo_off"));
                    msActions.Add(new Action("icon_internet_on", "internet_on"));
                    msActions.Add(new Action("icon_internet_off", "internet_off"));
                    msActions.Add(new Action("icon_status", "map_req"));
                    msActions.Add(new Action("balance", "balance"));
                    msActions.Add(new Action("icon_reset", "reset"));
                }
                return msActions;
            }
        }
    }
}
