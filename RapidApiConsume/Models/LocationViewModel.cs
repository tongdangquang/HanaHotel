namespace RapidApiConsume.Models
{
    public class LocationViewModel
    {

        public class Rootobject
        {
            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public string dest_id { get; set; }
            public string search_type { get; set; }
            public string label { get; set; }
            public string country { get; set; }
            public string city_name { get; set; }
            public string image_url { get; set; }
            public string cc1 { get; set; }
            public string type { get; set; }
            public string roundtrip { get; set; }
            public string lc { get; set; }
            public string city_ufi { get; set; }
            public int Hotels { get; set; }
            public string region { get; set; }

        }

    }
}
