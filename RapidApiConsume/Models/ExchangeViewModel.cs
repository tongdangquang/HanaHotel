namespace RapidApiConsume.Models
{


    public class Rootobject
    {
        public bool status { get; set; }
        public string message { get; set; }
        public long timestamp { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string base_currency { get; set; }
        public ExhangeViewModel[] exchange_rates { get; set; }
        public string base_currency_date { get; set; }
    }

    public class ExhangeViewModel
    {
        public string exchange_rate_buy { get; set; }
        public string currency { get; set; }
    }

}
