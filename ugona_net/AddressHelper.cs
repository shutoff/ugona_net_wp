using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using SQLite;

namespace ugona_net
{
    

    class AddressHelper
    {
        static private double D2R = 0.017453; // Константа для преобразования градусов в радианы
        static private double a = 6378137.0; // Основные полуоси
        static private double e2 = 0.006739496742337; // Квадрат эксцентричности эллипсоида

        static private String OSM_URL = "https://nominatim.openstreetmap.org/reverse?lat=$1&lon=$2&osm_type=N&format=json&address_details=0&accept-language=$3";

        public static double? distance(double? lat1, double? lon1, double? lat2, double? lon2){
            if ((lat1 == null) || (lon1 == null) || (lat2 == null) || (lon2 == null))
                return null;

            if ((lat1 == lat2) && (lon1 == lon2))
                return 0;

            double fdLambda = ((double)lon1 - (double)lon2) * D2R;
            double fdPhi = ((double)lat1 - (double)lat2) * D2R;
            double fPhimean = (((double)lat1 + (double)lat2) / 2.0) * D2R;

            double fTemp = 1 - e2 * (Math.Pow(Math.Sin(fPhimean), 2));
            double fRho = (a * (1 - e2)) / Math.Pow(fTemp, 1.5);
            double fNu = a / (Math.Sqrt(1 - e2 * (Math.Sin(fPhimean) * Math.Sin(fPhimean))));

            double fz = Math.Sqrt(Math.Pow(Math.Sin(fdPhi / 2.0), 2) +
                    Math.Cos((double)lat2 * D2R) * Math.Cos((double)lat1 * D2R) * Math.Pow(Math.Sin(fdLambda / 2.0), 2));
            fz = 2 * Math.Asin(fz);

            double fAlpha = Math.Cos((double)lat1 * D2R) * Math.Sin(fdLambda) * 1 / Math.Sin(fz);
            fAlpha = Math.Asin(fAlpha);

            double fR = (fRho * fNu) / ((fRho * Math.Pow(Math.Sin(fAlpha), 2)) + (fNu * Math.Pow(Math.Cos(fAlpha), 2)));

            return fz * fR;
        }

        public class Address
        {
            [Indexed]
            public double Lat { get; set; }
            [Indexed]
            public double Lng { get; set; }
            public String address { get; set; }
            public String param { get; set; }
        }

        static SQLiteAsyncConnection db_;

        static SQLiteAsyncConnection db
        {
            get
            {
                if (db_ == null)
                {
                    Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    String DBPath = Path.Combine(localFolder.Path, "address");
                    db_ = new SQLiteAsyncConnection(DBPath);
                    db_.CreateTableAsync<Address>();
                }
                return db_;
            }
        }

        static private char[] separator = { ',' };

        async public static Task<String> get(String str)
        {
            String[] parts = str.Split(separator);
            return await get(Double.Parse(parts[0]), Double.Parse(parts[1]));
        }

        async public static Task<String> get(double? lat, double? lon)
        {
            if ((lat == null) || (lon == null))
                return null;
            lat = Math.Round((double)lat * 100000.0) / 100000.0;
            lon = Math.Round((double)lon * 100000.0) / 100000.0;

            var query = await db.QueryAsync<Address>("select * from Address where (Lat BETWEEN ? AND ?) AND (Lng BETWEEN ? AND ?)", lat - 0.001, lat + 0.001, lon - 0.001, lon + 0.001);
            String result = null;
            double best = 80;
            foreach (Address row in query){
                double dist = (double)distance(row.Lat, row.Lng, lat, lon);
                if (dist < best)
                {
                    best = dist;
                    result = row.address;
                }
            }
            if (result != null)
                return result;

            JObject res = await Helper.GetJson(OSM_URL, String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", lat), String.Format(CultureInfo.InvariantCulture, "{0:0.#####}", lon), Helper.GetString("ResourceLanguage"));
            string[] separators = new string[] {", "};
            String[] parts = res.GetValue("display_name").ToString().Split(separators, StringSplitOptions.None);
            JToken house_number = null;
            JToken address = res.GetValue("address");
            if (address != null)
                house_number = address.ToObject<JObject>().GetValue("house_number");
            if (house_number != null)
            {
                String house = house_number.ToString();
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (parts[i] == house)
                    {
                        parts[i] = parts[i + 1];
                        parts[i + 1] = house;
                        break;
                    }
                }
            }
            for (int i = 0; i < parts.Length - 2; i++)
            {
                if (parts[i] == "Unnamed road")
                    continue;
                if (result == null)
                {
                    result = parts[i];
                    continue;
                }
                result += ", ";
                result += parts[i];
            }
            Address addr = new Address();
            addr.address = result;
            addr.Lat = (double)lat;
            addr.Lng = (double)lon;
            db.InsertAsync(addr);
            return result;
        }
    }

}
