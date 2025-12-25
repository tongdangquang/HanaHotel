namespace HanaHotel.WebUI.DTOs.DashboardDTO.DashboardSocialPartialDTO
{
    public class InstagramCountModel
    {

        public class Rootobject
        {
            public Data data { get; set; }
        }

        public class Data
        {
            public Edge_Followed_By edge_followed_by { get; set; }
        }

        public class Edge_Followed_By
        {
            public int count { get; set; }
        }

    }
}
