using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ugona_net
{

    public class Event
    {

        public long id
        {
            get;
            set;
        }

        public long time
        {
            get;
            set;
        }

        public int type
        {
            get;
            set;
        }

        public String Name
        {
            get
            {
                if (defs.ContainsKey(type))
                    return Helper.GetString(defs[type].name);
                return "Event #:" + id;
            }
        }

        public String Image
        {
            get
            {
                String s = "e_system";
                if (defs.ContainsKey(type))
                    s = defs[type].pict;
                return "/Assets/Events/" + s + ".png";
            }
        }

        public String Time
        {
            get
            {
                DateTime dt = DateUtils.ToDateTime(time);
                return dt.ToShortTimeString();
            }
        }

        struct EventDef
        {
            public string name;
            public string pict;
            public int level;
        }

        static Dictionary<int, EventDef> events_def;

        Dictionary<int, EventDef> defs
        {
            get
            {
                if (events_def == null)
                {
                    events_def = new Dictionary<int, EventDef>();
                    add(1, "light_shock", "e_light_shock", 0);
                    add(2, "ext_zone", "e_exit_zone", 0);
                    add(3, "heavy_shock", "e_heavy_shock", 0);
                    add(4, "inner_zone", "e_inner_zone", 0);
                    add(5, "trunk_open", "e_boot_open", 2);
                    add(6, "hood_open", "e_hood_open", 2);
                    add(7, "door_open", "e_doors_open", 2);
                    add(8, "tilt", "e_tilt", 0);
                    add(9, "ignition_on", "e_ignition_on", 2);
                    add(10, "access_on", "e_access_on", 2);
                    add(11, "input1_on", "e_input1_on", 2);
                    add(12, "input2_on", "e_input2_on", 2);
                    add(13, "input3_on", "e_input3_on", 2);
                    add(14, "input4_on", "e_input4_on", 2);
                    add(15, "trunk_close", "e_boot_close", 2);
                    add(16, "hood_close", "e_hood_close", 2);
                    add(17, "door_close", "e_doors_close", 2);
                    add(18, "ignition_off", "e_ignition_off", 2);
                    add(19, "access_off", "e_access_off", 2);
                    add(20, "input1_off", "e_input1_off", 2);
                    add(21, "input2_off", "e_input2_off", 2);
                    add(22, "input3_off", "e_input3_off", 2);
                    add(23, "input4_off", "e_input4_off", 2);
                    add(24, "guard_on", "e_guard_on", 1);
                    add(25, "guard_off", "e_guard_off", 1);
                    add(26, "reset", "e_reset", 3);
                    add(27, "main_power_on", "e_main_power_off", 0);
                    add(28, "main_power_off", "e_main_power_off", 0);
                    add(29, "reserve_power_on", "e_reserve_power_off", 0);
                    add(30, "reserve_power_off", "e_reserve_power_off", 0);
                    add(31, "gsm_recover", "e_gsm_recover", 3);
                    add(32, "gsm_fail", "e_gsm_fail", 3);
                    add(33, "gsm_new", "e_gsm_recover", 3);
                    add(34, "gps_recover", "e_gps_recover", 3);
                    add(35, "gps_fail", "e_gps_fail", 3);
                    add(37, "trace_start", "e_trace_start", 3);
                    add(38, "trace_stop", "e_trace_stop", 3);
                    add(39, "trace_point", "e_trace_start", 3);
                    add(41, "timer_event", "e_timer", 3);
                    add(42, "user_call", "e_user_call", 1);
                    add(43, "rogue", "e_rogue", 0);
                    add(44, "rogue_off", "e_rogue", 0);
                    add(45, "motor_start_azd", "e_motor_start", 1);
                    add(46, "motor_start", "e_motor_start", 1);
                    add(47, "motor_stop", "e_motor_stop", 1);
                    add(48, "motor_start_error", "e_motor_error", 1);
                    add(49, "alarm_boot", "e_alarm_boot", 0);
                    add(50, "alarm_hood", "e_alarm_hood", 0);
                    add(51, "alarm_door", "e_alarm_door", 0);
                    add(52, "ignition_lock", "e_ignition_on", 0);
                    add(53, "alarm_accessories", "e_alarm_accessories", 0);
                    add(54, "alarm_input1", "e_alarm_input1", 0);
                    add(55, "alarm_input2", "e_alarm_input2", 0);
                    add(56, "alarm_input3", "e_alarm_input3", 0);
                    add(57, "alarm_input4", "e_alarm_input4", 0);
                    add(58, "sms_request", "e_user_sms", 1);
                    add(59, "reset_modem", "e_reset_modem", 3);
                    add(60, "gprs_on", "e_gprs_on", 3);
                    add(61, "gprs_off", "e_gprs_off", 3);
                    add(65, "reset", "e_reset", 3);
                    add(66, "gsm_register_fail", "e_gsm_fail", 3);
                    add(68, "net_error", "e_system", 3);
                    add(71, "sms_err", "e_user_sms", 3);
                    add(72, "net_error", "e_system", 3);
                    add(74, "error_read", "e_system", 3);
                    add(75, "net_error", "e_system", 3);
                    add(76, "reset_modem", "e_reset_modem", 3);
                    add(77, "reset_modem", "e_reset_modem", 3);
                    add(78, "reset_modem", "e_reset_modem", 3);
                    add(79, "reset_modem", "e_reset_modem", 3);
                    add(80, "reset_modem", "e_reset_modem", 3);
                    add(85, "sos", "e_sos", 0);
                    add(86, "zone_in", "e_zone_in", 1);
                    add(87, "zone_out", "e_zone_out", 1);
                    add(88, "incomming_sms", "e_user_sms", 1);
                    add(89, "request_photo", "e_request_photo", 1);
                    add(90, "till_start", "e_till_start", 3);
                    add(91, "end_move", "e_system", 3);
                    add(94, "temp_change", "e_system", 3);
                    add(98, "data_transfer", "e_system", 3);
                    add(100, "reset", "e_reset", 3);
                    add(101, "reset", "e_reset", 3);
                    add(105, "reset_modem", "e_reset_modem", 3);
                    add(106, "reset_modem", "e_reset_modem", 3);
                    add(107, "reset_modem", "e_reset_modem", 3);
                    add(108, "reset_modem", "e_reset_modem", 3);
                    add(110, "valet_off", "e_valet_off", 1);
                    add(111, "lock_off1", "e_lockclose1", 1);
                    add(112, "lock_off2", "e_lockclose2", 1);
                    add(113, "lock_off3", "e_lockclose3", 1);
                    add(114, "lock_off4", "e_lockclose4", 1);
                    add(115, "lock_off5", "e_lockclose5", 1);
                    add(-116, "lan_change", "e_system", 0);
                    add(120, "valet_on", "e_valet_on", 1);
                    add(121, "lock_on1", "e_lockopen1", 1);
                    add(122, "lock_on2", "e_lockopen2", 1);
                    add(123, "lock_on3", "e_lockopen3", 1);
                    add(124, "lock_on4", "e_lockopen4", 1);
                    add(125, "lock_on5", "e_lockopen5", 1);
                    add(127, "brk_data", "e_system", 3);
                    add(128, "input5_on", "e_input5_on", 2);
                    add(129, "input5_off", "e_input5_off", 2);
                    add(130, "voice", "e_voice", 3);
                    add(131, "download_events", "e_settings", 1);
                    add(132, "can_on", "e_can", 1);
                    add(133, "can_off", "e_can", 1);
                    add(134, "input6_on", "e_input6_on", 2);
                    add(135, "input6_off", "e_input6_off", 2);
                    add(136, "low_battery", "e_system", 0);
                    add(137, "download_settings", "e_settings", 3);
                    add(138, "guard2_on", "e_guard_on", 1);
                    add(139, "guard2_off", "e_guard_off", 1);
                    add(140, "lan_change", "e_system", 0);
                    add(141, "command", "e_system", 1);
                    add(142, "brake", "e_brake", 2);
                    add(145, "brake_on", "e_brake", 2);
                    add(146, "brake_off", "e_brake", 2);
                    add(293, "sos", "e_sos", 0);
                    add(10101, "e0101", "e_guard_on", 1);
                    add(10102, "e0102", "e_guard_on", 1);
                    add(10103, "e0103", "e_guard_on", 1);
                    add(10104, "e0104", "e_guard_on", 1);
                    add(10105, "e0105", "e_guard_on", 1);
                    add(10106, "e0106", "e_guard_on", 1);
                    add(10107, "e0107", "e_guard_on", 1);
                    add(10201, "e0201", "e_guard_off", 1);
                    add(10202, "e0202", "e_guard_off", 1);
                    add(10203, "e0203", "e_guard_off", 1);
                    add(10204, "e0204", "e_guard_off", 1);
                    add(10205, "e0205", "e_guard_off", 1);
                    add(10206, "e0206", "e_guard_off", 1);
                    add(10301, "e0301", "e_alarm_accessories", 0);
                    add(10302, "e0302", "e_alarm_accessories", 0);
                    add(10303, "e0303", "e_alarm_accessories", 0);
                    add(10304, "e0304", "e_light_shock", 0);
                    add(10305, "e0305", "e_heavy_shock", 0);
                    add(10306, "e0306", "e_brake", 0);
                    add(10307, "e0307", "e_alarm_accessories", 0);
                    add(10308, "e0308", "e_alarm_accessories", 0);
                    add(10309, "e0309", "e_alarm_accessories", 0);
                    add(10310, "e0310", "e_alarm_accessories", 0);
                    add(10311, "e0311", "e_alarm_boot", 0);
                    add(10312, "e0312", "e_alarm_door", 0);
                    add(10313, "e0313", "e_alarm_door", 0);
                    add(10314, "e0314", "e_alarm_door", 0);
                    add(10315, "e0315", "e_alarm_door", 0);
                    add(10316, "e0316", "e_alarm_hood", 0);
                    add(10401, "e0401", "e_motor_start", 1);
                    add(10402, "e0402", "e_motor_start", 1);
                    add(10403, "e0403", "e_motor_start", 1);
                    add(10404, "e0404", "e_motor_start", 1);
                    add(10405, "e0405", "e_motor_start", 1);
                    add(10406, "e0406", "e_motor_start", 1);
                    add(10501, "e0501", "e_motor_stop", 1);
                    add(10502, "e0502", "e_motor_stop", 1);
                    add(10503, "e0503", "e_motor_stop", 1);
                    add(10504, "e0504", "e_motor_stop", 1);
                    add(10505, "e0505", "e_motor_stop", 1);
                    add(10506, "e0506", "e_motor_stop", 1);
                    add(10601, "e0601", "e_rogue", 1);
                    add(10602, "e0602", "e_rogue", 1);
                    add(10603, "e0603", "e_rogue", 1);
                    add(10701, "e0701", "e_valet_on", 1);
                    add(10801, "e0801", "e_system", 3);
                    add(10802, "e0802", "e_system", 3);
                    add(10803, "e0803", "e_system", 3);
                    add(10804, "e0804", "e_system", 3);
                    add(10805, "e0805", "e_system", 3);
                    add(10806, "e0806", "e_system", 3);
                    add(10807, "e0807", "e_system", 3);
                    add(10808, "e0808", "e_system", 3);
                    add(10901, "e0901", "e_system", 2);
                    add(11001, "e1001", "e_sos", 0);
                    add(11101, "e1101", "e_gsm_fail", 3);
                    add(11102, "e1102", "e_gsm_recover", 3);
                    add(11103, "e1103", "e_user_call", 2);
                    add(11201, "e1201", "e_sos", 2);
                    add(11301, "e1301", "e_motor_error", 2);
                    add(11302, "e1302", "e_motor_error", 2);
                    add(11303, "e1303", "e_motor_error", 2);
                    add(11304, "e1304", "e_motor_error", 2);
                    add(11401, "e1401", "e_trace_start", 2);
                    add(11402, "e1402", "e_trace_start", 2);
                    add(11403, "e1403", "e_trace_start", 2);
                    add(11501, "e1501", "e_trace_stop", 2);
                    add(11502, "e1502", "e_trace_stop", 2);
                    add(11503, "e1503", "e_trace_stop", 2);
                    add(11601, "e1601", "e_main_power_off", 2);
                    add(11701, "e1701", "e_hood_open", 2);
                    add(11702, "e1702", "e_hood_open", 2);
                    add(11703, "e1703", "e_hood_open", 2);
                    add(13700, "e3700", "e_tilt", 3);
                }
                return events_def;
            }
        }

        static void add(int id, string name, string pict, int level) {
            EventDef d = new EventDef();
            d.name = name;
            d.pict = pict;
            d.level = level;
            events_def.Add(id, d);
        }
    }
}
