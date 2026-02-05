using FixItUp.Data;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TasksController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] TaskEntity task)
        {
            if (task == null)
                return BadRequest("Task data is required");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(task.Title))
                return BadRequest("Title is required");
            
            if (string.IsNullOrWhiteSpace(task.Category))
                return BadRequest("Category is required");
            
            if (string.IsNullOrWhiteSpace(task.Description))
                return BadRequest("Description is required");
            
            if (string.IsNullOrWhiteSpace(task.Location))
                return BadRequest("Location is required");
            
            if (task.CustomerId <= 0)
                return BadRequest("Valid CustomerId is required");

            // Set default values for fields that might be null
            task.Status = "Posted";
            task.CreatedAt = DateTime.UtcNow;
            task.BeforeImageURL = task.BeforeImageURL ?? string.Empty;
            task.AfterImageURL = task.AfterImageURL ?? string.Empty;

            try
            {
                _db.Tasks.Add(task);
                _db.SaveChanges();
                return Ok(task);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error saving task: {ex.Message}");
            }
        }

        [HttpGet("nearby")]
        public IActionResult Nearby(int? workerId = null)
        {
            var query = _db.Tasks.Where(t => t.Status == "Posted");

            if (workerId.HasValue)
            {
                // Get worker's location (state)
                var worker = _db.Users.Find(workerId.Value);
                if (worker != null && !string.IsNullOrEmpty(worker.State))
                {
                    // Filter tasks by same state
                    query = query.Where(t => t.State == worker.State);
                }

                // Get worker's skill names
                var skillNames = _db.WorkerSkills
                    .Where(ws => ws.UserId == workerId)
                    .Select(ws => ws.Category.Name)
                    .ToList();

                // Filter by matching skills
                if (skillNames.Any())
                {
                    query = query.Where(t => skillNames.Contains(t.Category));
                }
            }

            return Ok(query.ToList());
        }

        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, string status, string afterImageUrl)
        {
            var task = _db.Tasks.Find(id);
            if (task == null) return NotFound();

            if (status == "Completed" && string.IsNullOrEmpty(afterImageUrl))
                return BadRequest("Completion image required");

            task.Status = status;
            task.AfterImageURL = afterImageUrl;

            _db.SaveChanges();
            return Ok();
        }

        [HttpGet("customer/{customerId}")]
        public IActionResult GetCustomerTasks(int customerId)
        {
            var tasks = _db.Tasks
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Category,
                    t.Location,
                    t.MinBudget,
                    t.MaxBudget,
                    t.IsUrgent,
                    t.Status,
                    t.CreatedAt,
                    t.AssignedWorkerId,
                    BidsCount = _db.Bids.Count(b => b.TaskId == t.Id),
                    Bids = _db.Bids
                        .Where(b => b.TaskId == t.Id)
                        .OrderBy(b => b.Amount)
                        .Select(b => new
                        {
                            b.Id,
                            b.Amount,
                            b.EstimatedHours,
                            b.WorkerId,
                            b.CreatedAt,
                            Worker = _db.Users
                                .Where(u => u.Id == b.WorkerId)
                                .Select(u => new
                                {
                                    u.Id,
                                    u.FullName,
                                    u.TrustScore,
                                    u.IsTopRated,
                                    u.IsVerifiedPro,
                                    JobsCompleted = _db.Tasks.Count(t2 => t2.AssignedWorkerId == u.Id && t2.Status == "Completed")
                                })
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .ToList();

            return Ok(tasks);
        }

        [HttpGet("all")]
        public IActionResult GetAllTasks()
        {
            var tasks = _db.Tasks
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Category,
                    t.Location,
                    t.State,
                    t.City,
                    t.MinBudget,
                    t.MaxBudget,
                    t.Status,
                    t.CreatedAt,
                    t.CustomerId,
                    t.AssignedWorkerId,
                    CustomerName = _db.Users
                        .Where(u => u.Id == t.CustomerId)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    WorkerName = t.AssignedWorkerId.HasValue
                        ? _db.Users
                            .Where(u => u.Id == t.AssignedWorkerId)
                            .Select(u => u.FullName)
                            .FirstOrDefault()
                        : null,
                    BidsCount = _db.Bids.Count(b => b.TaskId == t.Id)
                })
                .ToList();

            return Ok(tasks);
        }

        [HttpGet("worker/{workerId}/my-jobs")]
        public IActionResult GetWorkerJobs(int workerId)
        {
            var tasks = _db.Tasks
                .Where(t => t.AssignedWorkerId == workerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Category,
                    t.Location,
                    t.State,
                    t.City,
                    t.Status,
                    t.CreatedAt,
                    t.CustomerId,
                    Customer = _db.Users
                        .Where(u => u.Id == t.CustomerId)
                        .Select(u => new
                        {
                            u.Id,
                            u.FullName,
                            u.Email
                        })
                        .FirstOrDefault(),
                    AcceptedBid = _db.Bids
                        .Where(b => b.TaskId == t.Id && b.WorkerId == workerId)
                        .Select(b => new
                        {
                            b.Id,
                            b.Amount,
                            b.EstimatedHours
                        })
                        .FirstOrDefault()
                })
                .ToList();

            return Ok(tasks);
        }

        [HttpPut("{taskId}/status")]
        public IActionResult UpdateStatus(int taskId, [FromBody] UpdateStatusRequest request)
        {
            var task = _db.Tasks.Find(taskId);
            if (task == null) return NotFound("Task not found");

            var oldStatus = task.Status;
            task.Status = request.Status;

            // Worker starts work
            if (request.Status == "InProgress" && oldStatus == "Accepted")
            {
                // Worker can start the work
            }

            // Worker completes work - needs customer verification
            if (request.Status == "WorkerCompleted" && oldStatus == "InProgress")
            {
                // Status changes to WorkerCompleted, waiting for customer verification
                // TODO: Send notification to customer
            }

            // Customer verifies and closes the work
            if (request.Status == "Completed" && oldStatus == "WorkerCompleted")
            {
                task.CompletedAt = DateTime.UtcNow;

                Console.WriteLine($"Task {taskId} is being closed by customer");

                if (task.AssignedWorkerId.HasValue)
                {
                    // Check if payment doesn't already exist
                    var existingPayment = _db.Payments
                        .FirstOrDefault(p => p.TaskId == taskId);
                    
                    Console.WriteLine($"Existing payment for task {taskId}: {(existingPayment != null ? "Found" : "Not found")}");
                    
                    if (existingPayment == null)
                    {
                        // Get the accepted bid amount - try multiple ways
                        decimal bidAmount = 0;
                        
                        Console.WriteLine($"AcceptedBidId: {(task.AcceptedBidId.HasValue ? task.AcceptedBidId.Value.ToString() : "null")}");
                        
                        // First try using AcceptedBidId
                        if (task.AcceptedBidId.HasValue)
                        {
                            var acceptedBid = _db.Bids.Find(task.AcceptedBidId.Value);
                            if (acceptedBid != null)
                            {
                                bidAmount = acceptedBid.Amount;
                                Console.WriteLine($"Found accepted bid amount: {bidAmount}");
                            }
                        }
                        
                        // If no AcceptedBidId, find the bid by worker
                        if (bidAmount == 0)
                        {
                            Console.WriteLine($"Looking for bid by TaskId {taskId} and WorkerId {task.AssignedWorkerId.Value}");
                            var workerBid = _db.Bids
                                .FirstOrDefault(b => b.TaskId == taskId && b.WorkerId == task.AssignedWorkerId.Value);
                            if (workerBid != null)
                            {
                                bidAmount = workerBid.Amount;
                                Console.WriteLine($"Found worker bid amount: {bidAmount}");
                            }
                            else
                            {
                                Console.WriteLine("No bid found for this worker");
                            }
                        }

                        // Create payment if amount is found
                        if (bidAmount > 0)
                        {
                            var payment = new Payment
                            {
                                TaskId = taskId,
                                CustomerId = task.CustomerId,
                                WorkerId = task.AssignedWorkerId.Value,
                                Amount = bidAmount,
                                Status = "Pending",
                                CreatedAt = DateTime.UtcNow
                            };
                            _db.Payments.Add(payment);
                            Console.WriteLine($"Payment created: TaskId={taskId}, Amount={bidAmount}, Status=Pending");
                        }
                        else
                        {
                            Console.WriteLine($"ERROR: Could not find bid amount for task {taskId}");
                        }
                    }
                }
            }

            _db.SaveChanges();
            return Ok(new { message = "Task status updated successfully", task });
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }
}
