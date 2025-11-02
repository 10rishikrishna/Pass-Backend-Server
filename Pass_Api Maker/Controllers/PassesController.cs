using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Pass_Api_Maker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassesController : ControllerBase
    {
        // In-memory storage (replace with database in production)
        private static List<PassModel> passes = new List<PassModel>();
        private static object lockObj = new object();

        /// <summary>
        /// Submit a new pass for approval
        /// </summary>
        [HttpPost]
        public IActionResult SubmitPass([FromBody] PassModel pass)
        {
            if (pass == null)
                return BadRequest(new { message = "Pass data is required" });

            if (string.IsNullOrEmpty(pass.LaborID))
                return BadRequest(new { message = "LaborID is required" });

            lock (lockObj)
            {
                // Check for duplicates
                var existing = passes.FirstOrDefault(p => p.LaborID == pass.LaborID && p.Status == "Pending");
                if (existing != null)
                    return BadRequest(new { message = $"A pending pass already exists for LaborID: {pass.LaborID}" });

                pass.Status = "Pending";
                pass.SubmittedAt = DateTime.Now;

                // Initialize nullable fields if null
                pass.ApprovedBy ??= "";
                pass.RejectionReason ??= "";

                passes.Add(pass);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ New pass submitted - LaborID: {pass.LaborID}, Name: {pass.FullName}");
            }

            return Ok(new
            {
                message = "Pass submitted successfully",
                laborID = pass.LaborID,
                status = pass.Status
            });
        }

        /// <summary>
        /// Get all passes (for Authenticator)
        /// </summary>
        [HttpGet]
        public IActionResult GetAllPasses()
        {
            lock (lockObj)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📋 Fetching all passes. Total: {passes.Count}");
                return Ok(passes.ToList());
            }
        }

        /// <summary>
        /// Get approved passes only (for Form9 to poll)
        /// </summary>
        [HttpGet("approved")]
        public IActionResult GetApprovedPasses()
        {
            lock (lockObj)
            {
                var approvedPasses = passes.Where(p => p.Status == "Approved").ToList();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ Fetching approved passes. Count: {approvedPasses.Count}");
                return Ok(approvedPasses);
            }
        }

        /// <summary>
        /// Update pass status (Approve/Reject)
        /// </summary>
        [HttpPost("update-status")]
        public IActionResult UpdatePassStatus([FromBody] StatusUpdateRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.LaborID))
                return BadRequest(new { message = "LaborID is required" });

            lock (lockObj)
            {
                var pass = passes.FirstOrDefault(p => p.LaborID == request.LaborID);
                if (pass == null)
                    return NotFound(new { message = $"Pass not found for LaborID: {request.LaborID}" });

                // Update status
                pass.Status = request.Status;
                pass.ApprovedBy = request.ApprovedBy ?? "";
                if (DateTime.TryParse(request.ApprovedAt, out DateTime approvedDate))
                    pass.ApprovedAt = approvedDate;
                pass.RejectionReason = request.Reason ?? "";

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔄 Pass status updated - LaborID: {pass.LaborID}, Status: {pass.Status}, By: {pass.ApprovedBy}");
            }

            return Ok(new
            {
                message = $"Pass {request.Status.ToLower()} successfully",
                laborID = request.LaborID,
                status = request.Status,
                approvedBy = request.ApprovedBy
            });
        }

        /// <summary>
        /// Mark pass as downloaded
        /// </summary>
        [HttpPost("mark-downloaded/{laborId}")]
        public IActionResult MarkAsDownloaded(string laborId)
        {
            lock (lockObj)
            {
                var pass = passes.FirstOrDefault(p => p.LaborID == laborId);
                if (pass == null)
                    return NotFound(new { message = $"Pass not found for LaborID: {laborId}" });

                pass.IsDownloaded = true;
                pass.DownloadedAt = DateTime.Now;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📥 Pass marked as downloaded - LaborID: {laborId}");
            }

            return Ok(new { message = "Pass marked as downloaded", laborID = laborId });
        }

        /// <summary>
        /// Clear all passes (for testing)
        /// </summary>
        [HttpDelete("clear")]
        public IActionResult ClearAllPasses()
        {
            lock (lockObj)
            {
                int count = passes.Count;
                passes.Clear();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🗑️ Cleared all passes. Removed: {count}");
                return Ok(new { message = $"Cleared {count} pass(es)" });
            }
        }
    }

    // Models
    public class PassModel
    {
        [JsonPropertyName("laborID")]
        public string LaborID { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("dob")]
        public string DOB { get; set; }

        [JsonPropertyName("contractorName")]
        public string ContractorName { get; set; }

        [JsonPropertyName("area")]
        public string Area { get; set; }

        [JsonPropertyName("gateAccess")]
        public string GateAccess { get; set; }

        [JsonPropertyName("entryDate")]
        public string EntryDate { get; set; }

        [JsonPropertyName("exitDate")]
        public string ExitDate { get; set; }

        [JsonPropertyName("entryTime")]
        public string EntryTime { get; set; }

        [JsonPropertyName("checkoutTime")]
        public string CheckoutTime { get; set; }

        [JsonPropertyName("labourImageBase64")]
        public string LabourImageBase64 { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // FIX: Make these nullable with ? to accept null values
        [JsonPropertyName("approvedBy")]
        public string? ApprovedBy { get; set; }

        [JsonPropertyName("approvedAt")]
        public DateTime? ApprovedAt { get; set; }

        [JsonPropertyName("rejectionReason")]
        public string? RejectionReason { get; set; }

        [JsonPropertyName("isDownloaded")]
        public bool IsDownloaded { get; set; } = false;

        [JsonPropertyName("downloadedAt")]
        public DateTime? DownloadedAt { get; set; }
    }

    public class StatusUpdateRequest
    {
        [JsonPropertyName("laborID")]
        public string LaborID { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("approvedBy")]
        public string? ApprovedBy { get; set; }

        [JsonPropertyName("approvedAt")]
        public string? ApprovedAt { get; set; }
    }
}