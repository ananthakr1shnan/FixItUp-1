using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReviewsController(AppDbContext db)
        {
            _db = db;
        }

        // Create a review
        [HttpPost]
        public IActionResult CreateReview([FromBody] Review review)
        {
            try
            {
                // Validate that the task exists and is completed
                var task = _db.Tasks.Find(review.TaskId);
                if (task == null)
                    return NotFound("Task not found");

                if (task.Status != "Completed")
                    return BadRequest("Can only review completed tasks");

                // Verify the customer is the one who posted the task
                if (task.CustomerId != review.CustomerId)
                    return BadRequest("Only the customer who posted the task can review");

                // Verify the worker is the one who completed the task
                if (task.AssignedWorkerId != review.WorkerId)
                    return BadRequest("Invalid worker ID");

                // Check if review already exists for this task
                var existingReview = _db.Reviews.FirstOrDefault(r => r.TaskId == review.TaskId);
                if (existingReview != null)
                    return BadRequest("Review already submitted for this task");

                // Validate rating
                if (review.Rating < 1 || review.Rating > 5)
                    return BadRequest("Rating must be between 1 and 5");

                review.CreatedAt = DateTime.UtcNow;
                review.IsVerified = true;

                _db.Reviews.Add(review);
                _db.SaveChanges();

                // Update worker's trust score and rating
                UpdateWorkerStats(review.WorkerId);

                return Ok(new { message = "Review submitted successfully", reviewId = review.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get reviews for a specific worker
        [HttpGet("worker/{workerId}")]
        public IActionResult GetWorkerReviews(int workerId)
        {
            try
            {
                var reviews = _db.Reviews
                    .Include(r => r.Customer)
                    .Include(r => r.Task)
                    .Where(r => r.WorkerId == workerId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt,
                        r.IsVerified,
                        TaskTitle = r.Task.Title,
                        TaskCategory = r.Task.Category,
                        CustomerName = r.Customer.FullName,
                        CustomerId = r.Customer.Id
                    })
                    .ToList();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get reviews for a specific customer
        [HttpGet("customer/{customerId}")]
        public IActionResult GetCustomerReviews(int customerId)
        {
            try
            {
                var reviews = _db.Reviews
                    .Include(r => r.Worker)
                    .Include(r => r.Task)
                    .Where(r => r.CustomerId == customerId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt,
                        TaskTitle = r.Task.Title,
                        TaskCategory = r.Task.Category,
                        WorkerName = r.Worker.FullName,
                        WorkerId = r.Worker.Id
                    })
                    .ToList();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get worker rating summary
        [HttpGet("worker/{workerId}/summary")]
        public IActionResult GetWorkerRatingSummary(int workerId)
        {
            try
            {
                var reviews = _db.Reviews.Where(r => r.WorkerId == workerId).ToList();
                
                if (!reviews.Any())
                {
                    return Ok(new
                    {
                        workerId,
                        averageRating = 0.0,
                        totalReviews = 0,
                        ratingDistribution = new { five = 0, four = 0, three = 0, two = 0, one = 0 }
                    });
                }

                var summary = new
                {
                    workerId,
                    averageRating = Math.Round(reviews.Average(r => r.Rating), 2),
                    totalReviews = reviews.Count,
                    ratingDistribution = new
                    {
                        five = reviews.Count(r => r.Rating == 5),
                        four = reviews.Count(r => r.Rating == 4),
                        three = reviews.Count(r => r.Rating == 3),
                        two = reviews.Count(r => r.Rating == 2),
                        one = reviews.Count(r => r.Rating == 1)
                    }
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get review for a specific task
        [HttpGet("task/{taskId}")]
        public IActionResult GetTaskReview(int taskId)
        {
            try
            {
                var review = _db.Reviews
                    .Include(r => r.Customer)
                    .Include(r => r.Worker)
                    .FirstOrDefault(r => r.TaskId == taskId);

                if (review == null)
                    return NotFound("No review found for this task");

                var result = new
                {
                    review.Id,
                    review.Rating,
                    review.Comment,
                    review.CreatedAt,
                    review.IsVerified,
                    CustomerName = review.Customer.FullName,
                    WorkerName = review.Worker.FullName
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Update a review
        [HttpPut("{reviewId}")]
        public IActionResult UpdateReview(int reviewId, [FromBody] Review updatedReview)
        {
            try
            {
                var review = _db.Reviews.Find(reviewId);
                if (review == null)
                    return NotFound("Review not found");

                // Only allow customer to update their own review
                if (review.CustomerId != updatedReview.CustomerId)
                    return Unauthorized("Cannot update another user's review");

                // Validate rating
                if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
                    return BadRequest("Rating must be between 1 and 5");

                review.Rating = updatedReview.Rating;
                review.Comment = updatedReview.Comment;

                _db.SaveChanges();

                // Update worker stats with new rating
                UpdateWorkerStats(review.WorkerId);

                return Ok(new { message = "Review updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Delete a review
        [HttpDelete("{reviewId}")]
        public IActionResult DeleteReview(int reviewId, [FromQuery] int customerId)
        {
            try
            {
                var review = _db.Reviews.Find(reviewId);
                if (review == null)
                    return NotFound("Review not found");

                // Only allow customer to delete their own review
                if (review.CustomerId != customerId)
                    return Unauthorized("Cannot delete another user's review");

                _db.Reviews.Remove(review);
                _db.SaveChanges();

                // Update worker stats after deletion
                UpdateWorkerStats(review.WorkerId);

                return Ok(new { message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Helper method to update worker statistics based on reviews
        private void UpdateWorkerStats(int workerId)
        {
            var worker = _db.Users.Find(workerId);
            if (worker == null) return;

            var reviews = _db.Reviews.Where(r => r.WorkerId == workerId).ToList();
            
            if (reviews.Any())
            {
                var avgRating = reviews.Average(r => r.Rating);
                // Update trust score based on average rating (scale to 0-100)
                worker.TrustScore = (int)(avgRating * 20);
                
                // Update top rated status (4.5+ average)
                worker.IsTopRated = avgRating >= 4.5;
            }

            _db.SaveChanges();
        }
    }
}
