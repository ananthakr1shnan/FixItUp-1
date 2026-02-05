using FixItUp.Data;
using FixItUp.DTOs;
using FixItUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixItUp.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PaymentsController(AppDbContext db)
        {
            _db = db;
        }

        // Customer: Get pending payments
        [HttpGet("customer/{customerId}/pending")]
        public IActionResult GetPendingPayments(int customerId)
        {
            try
            {
                Console.WriteLine($"Getting pending payments for customer: {customerId}");
                
                // Get tasks that are WorkerCompleted (worker done, customer needs to verify and pay)
                var pendingTasks = _db.Tasks
                    .Include(t => t.AcceptedBid)
                    .Where(t => t.CustomerId == customerId && t.Status == "WorkerCompleted")
                    .Select(t => new
                    {
                        Id = 0, // No payment ID yet
                        TaskId = t.Id,
                        TaskTitle = t.Title,
                        WorkerName = _db.Users.Where(u => u.Id == t.AssignedWorkerId).Select(u => u.FullName).FirstOrDefault(),
                        Amount = t.AcceptedBid != null ? t.AcceptedBid.Amount : 0,
                        CompletedDate = t.CompletedAt,
                        Status = "WorkerCompleted"
                    })
                    .ToList();
                
                Console.WriteLine($"Found {pendingTasks.Count} WorkerCompleted tasks");

                // Also get payments that are pending (already created but not released)
                var pendingPayments = _db.Payments
                    .Include(p => p.Task)
                    .Include(p => p.Worker)
                    .Where(p => p.CustomerId == customerId && p.Status == "Pending")
                    .Select(p => new
                    {
                        p.Id,
                        TaskId = p.TaskId,
                        TaskTitle = p.Task.Title,
                        WorkerName = p.Worker.FullName,
                        Amount = p.Amount,
                        CompletedDate = p.Task.CompletedAt,
                        Status = p.Status
                    })
                    .ToList();
                
                Console.WriteLine($"Found {pendingPayments.Count} pending payments");

                // Combine both lists
                var allPending = pendingTasks.Concat(pendingPayments).ToList();
                
                Console.WriteLine($"Total pending items: {allPending.Count}");

                return Ok(allPending);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetPendingPayments: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // Customer: Get transaction history
        [HttpGet("customer/{customerId}/history")]
        public IActionResult GetTransactionHistory(int customerId)
        {
            try
            {
                Console.WriteLine($"Getting transaction history for customer: {customerId}");
                
                var history = _db.Payments
                    .Include(p => p.Task)
                    .Include(p => p.Worker)
                    .Where(p => p.CustomerId == customerId && p.Status == "Completed")
                    .OrderByDescending(p => p.CompletedAt)
                    .Select(p => new
                    {
                        p.Id,
                        TaskTitle = p.Task.Title,
                        WorkerName = p.Worker.FullName,
                        Amount = p.Amount,
                        Date = p.CompletedAt,
                        TransactionId = p.TransactionId,
                        Status = p.Status
                    })
                    .ToList();

                Console.WriteLine($"Found {history.Count} completed transactions");
                return Ok(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetTransactionHistory: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // Customer: Get payment summary (total spent this month)
        [HttpGet("customer/{customerId}/summary")]
        public IActionResult GetCustomerSummary(int customerId)
        {
            var thisMonth = DateTime.UtcNow.Month;
            var thisYear = DateTime.UtcNow.Year;

            var totalSpentThisMonth = _db.Payments
                .Where(p => p.CustomerId == customerId 
                    && p.Status == "Completed" 
                    && p.CompletedAt.HasValue
                    && p.CompletedAt.Value.Month == thisMonth 
                    && p.CompletedAt.Value.Year == thisYear)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            var servicesCount = _db.Payments
                .Where(p => p.CustomerId == customerId 
                    && p.Status == "Completed" 
                    && p.CompletedAt.HasValue
                    && p.CompletedAt.Value.Month == thisMonth 
                    && p.CompletedAt.Value.Year == thisYear)
                .Count();

            return Ok(new
            {
                TotalSpent = totalSpentThisMonth,
                ServicesCount = servicesCount,
                TotalPaid = totalSpentThisMonth
            });
        }

        // Customer: Release payment to worker
        [HttpPost("release/{paymentId}")]
        public IActionResult ReleasePayment(int paymentId)
        {
            var payment = _db.Payments
                .Include(p => p.Worker)
                .FirstOrDefault(p => p.Id == paymentId);

            if (payment == null) return NotFound();

            payment.Status = "Completed";
            payment.CompletedAt = DateTime.UtcNow;
            payment.TransactionId = $"TXN{DateTime.UtcNow.Ticks}";

            // Update worker balance
            payment.Worker.AvailableBalance += payment.Amount;

            _db.SaveChanges();
            return Ok(new { message = "Payment released successfully" });
        }

        // Worker: Get earnings summary
        [HttpGet("worker/{workerId}/summary")]
        public IActionResult GetWorkerEarnings(int workerId)
        {
            try
            {
                Console.WriteLine($"Getting earnings summary for worker: {workerId}");
                
                var worker = _db.Users.Find(workerId);
                if (worker == null)
                {
                    Console.WriteLine($"Worker {workerId} not found");
                    return NotFound();
                }

                var totalEarned = _db.Payments
                    .Where(p => p.WorkerId == workerId && p.Status == "Completed")
                    .Sum(p => (decimal?)p.Amount) ?? 0;

                var pendingClearance = _db.Payments
                    .Where(p => p.WorkerId == workerId && p.Status == "Pending")
                    .Sum(p => (decimal?)p.Amount) ?? 0;

                Console.WriteLine($"Worker {workerId} - Available: {worker.AvailableBalance}, Pending: {pendingClearance}, Total: {totalEarned}");

                return Ok(new PaymentSummaryDTO
                {
                    AvailableBalance = worker.AvailableBalance,
                    PendingClearance = pendingClearance,
                    TotalEarned = totalEarned
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetWorkerEarnings: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // Worker: Get earnings history
        [HttpGet("worker/{workerId}/history")]
        public IActionResult GetWorkerHistory(int workerId)
        {
            try
            {
                Console.WriteLine($"Getting payment history for worker: {workerId}");
                
                var history = _db.Payments
                    .Include(p => p.Task)
                    .Include(p => p.Customer)
                    .Where(p => p.WorkerId == workerId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        p.Id,
                        TaskTitle = p.Task.Title,
                        CustomerName = p.Customer.FullName,
                        Amount = p.Amount,
                        Status = p.Status,
                        Date = p.Status == "Completed" ? p.CompletedAt : p.CreatedAt
                    })
                    .ToList();

                Console.WriteLine($"Found {history.Count} payment records for worker {workerId}");
                return Ok(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetWorkerHistory: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

        // Admin: Get all finance records with filters
        [HttpGet("admin/finance")]
        public IActionResult GetAdminFinance([FromQuery] string? filterBy, [FromQuery] string? date)
        {
            var query = _db.Payments
                .Include(p => p.Task)
                .Include(p => p.Customer)
                .Include(p => p.Worker)
                .AsQueryable();

            // Apply date filters
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(date))
            {
                DateTime filterDate = DateTime.Parse(date);
                
                if (filterBy == "day")
                {
                    query = query.Where(p => p.CompletedAt.HasValue && 
                        p.CompletedAt.Value.Date == filterDate.Date);
                }
                else if (filterBy == "month")
                {
                    query = query.Where(p => p.CompletedAt.HasValue &&
                        p.CompletedAt.Value.Month == filterDate.Month &&
                        p.CompletedAt.Value.Year == filterDate.Year);
                }
            }

            var payments = query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    TaskTitle = p.Task.Title,
                    CustomerName = p.Customer.FullName,
                    WorkerName = p.Worker.FullName,
                    Amount = p.Amount,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    CompletedAt = p.CompletedAt,
                    TransactionId = p.TransactionId
                })
                .ToList();

            var totalRevenue = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            var pendingAmount = payments.Where(p => p.Status == "Pending").Sum(p => p.Amount);

            return Ok(new
            {
                Payments = payments,
                Summary = new
                {
                    TotalRevenue = totalRevenue,
                    PendingAmount = pendingAmount,
                    TotalTransactions = payments.Count,
                    CompletedTransactions = payments.Count(p => p.Status == "Completed")
                }
            });
        }

        // Create payment when task is completed
        [HttpPost("create")]
        public IActionResult CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var task = _db.Tasks
                .Include(t => t.AcceptedBid)
                .FirstOrDefault(t => t.Id == request.TaskId);

            if (task == null) return NotFound("Task not found");

            var payment = new Payment
            {
                TaskId = request.TaskId,
                CustomerId = task.CustomerId,
                WorkerId = task.AssignedWorkerId ?? 0,
                Amount = task.AcceptedBid?.Amount ?? 0,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            _db.SaveChanges();

            return Ok(new { message = "Payment created successfully", paymentId = payment.Id });
        }
    }

    public class CreatePaymentRequest
    {
        public int TaskId { get; set; }
    }
}
