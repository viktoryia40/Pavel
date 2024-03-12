using MySql.Data.MySqlClient;
using StockPrice.DatabaseClasses;
using StockPrice.ResponseClasses;
using StockPrice.Settings;

namespace StockPrice.SiteMethods.USA_Sites
{
    public sealed class Lowes
    {

        public static void Parsing(DatabaseTotalResults request, List<MainPriceResponse> MainPriceResponsesList)
        {
            string search = request.Request;

            var cs = @$"Server={AppSettings.Current.Database.Host};Port={AppSettings.Current.Database.Port};User={AppSettings.Current.Database.Login};Database={AppSettings.Current.Database.Database};Password={AppSettings.Current.Database.Password}";
            using var con = new MySqlConnection(cs);

            var mpr = new MainPriceResponse() { SearchUrl = @$"https://partsexpert.ca/?s={search}&post_type=product&type_aws=true", Source = "PartsExpert" };
            var prices = new List<Prices>();


        }

    }
}
