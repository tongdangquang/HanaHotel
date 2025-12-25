namespace RapidApiConsume.Models
{
    public class ApiMovieViewModel
    {
        public int movieId { get; set; }
        public string Genre { get; set; }
        public string Actor { get; set; }
        public string Director { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string Cover { get; set; }
        public string Trailer { get; set; }

    }

    public class Result
    {
        public int Page { get; set; }
        public int Pages { get; set; }
        public int Count { get; set; }
        public List<ApiMovieViewModel> Data { get; set; }
    }

    public class ApiResponse
    {
        public string Version { get; set; }
        public Result Result { get; set; }
    }
}
