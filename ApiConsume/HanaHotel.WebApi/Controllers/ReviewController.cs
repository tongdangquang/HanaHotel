using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
			_reviewService = reviewService;
        }

        [HttpGet]
        public IActionResult GetReviews()
        {
            var reviews = _reviewService.TGetList();
            return Ok(reviews);
        }

        [HttpGet("{id}")]
        public IActionResult GetReview(int id)
        {
            var review = _reviewService.TGetByID(id);
            return Ok(review);
        }

        [HttpPut]
        public IActionResult UpdateReview(Review review)
        {
			_reviewService.TUpdate(review);
            return Ok();
        }

        [HttpPost]
        public IActionResult AddReview(Review review)
        {
			_reviewService.TInsert(review);
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteReview(int id)
        {
            var review = _reviewService.TGetByID(id);
            if (review == null)
            {
                return NotFound();
            }

			_reviewService.TDelete(review);
            return NoContent();
        }
    }
}
