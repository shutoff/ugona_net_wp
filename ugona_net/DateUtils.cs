using System;

namespace ugona_net
{
    class DateUtils
    {    
        /// <summary>
        /// The start of the Windows epoch
        /// </summary>
        public static readonly DateTime windowsEpoch = new DateTime(1601, 1, 1, 0, 0, 0, 0);
        /// <summary>
        /// The start of the Java epoch
        /// </summary>
        public static readonly DateTime javaEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// The difference between the Windows epoch and the Java epoch
        /// in milliseconds.
        /// </summary>
        public static readonly long epochDiff; /* = 1164447360000L; */

        static DateUtils()
        {
            epochDiff = (javaEpoch.ToFileTimeUtc() - windowsEpoch.ToFileTimeUtc())
                            / TimeSpan.TicksPerMillisecond;
        }

        public static long ToJavaTime(DateTime dateTime)
        {
            return (dateTime.ToFileTime() / TimeSpan.TicksPerMillisecond) - epochDiff;
        }

        public static DateTime ToDateTime(long javaTime)
        {
            return DateTime.FromFileTime((javaTime + epochDiff) * TimeSpan.TicksPerMillisecond);
        }

        public static long ToJavaTimeUtc(DateTime dateTime)
        {
            return (dateTime.ToFileTimeUtc() / TimeSpan.TicksPerMillisecond) - epochDiff;
        }

        public static DateTime ToDateTimeUtc(long javaTime)
        {
            return DateTime.FromFileTimeUtc((javaTime + epochDiff) * TimeSpan.TicksPerMillisecond);
        }

        static public String formatTime(long time)
        {
            if (time == 0)
                return "";
            DateTime dt = ToDateTime(time);
            String res = dt.ToString("t");
            String date = dt.ToString("d");
            if (date != DateTime.Now.ToString("d"))
                res += " " + date;
            return res;
        }

        static public long Now
        {
            get
            {
                return ToJavaTime(DateTime.Now);
            }
        }
    }
}
