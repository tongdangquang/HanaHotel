namespace RapidApiConsume.Models
{
    public class HotelViewModel
    {
        public class Rootobject
        {
            public Data data { get; set; }
        }

        public class Data
        {
            public Hotel[] Hotels { get; set; }
        }

        public class Hotel
        {
            public Property1 property { get; set; }
        }

        public class Property1
        {
            public string checkoutDate { get; set; }
            public string reviewScoreWord { get; set; }
            public string[] photoUrls { get; set; }
            public float reviewScore { get; set; }
            public string checkinDate { get; set; }
            public string name { get; set; }
            public string wishlistName { get; set; }
        }

    }
}
