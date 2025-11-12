using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Pass_Api_Maker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassesController : ControllerBase
    {
        private static List<PassModel> passes = new List<PassModel>();
        private static object lockObj = new object();

        [HttpPost]
        public IActionResult SubmitPass([FromBody] PassModel pass)
        {
            if (pass == null)
                return BadRequest(new { message = "Pass data is required" });

            if (string.IsNullOrWhiteSpace(pass.LaborID))
                return BadRequest(new { message = "LaborID is required" });

            if (string.IsNullOrWhiteSpace(pass.FullName))
                return BadRequest(new { message = "FullName is required" });

            lock (lockObj)
            {
                var existing = passes.FirstOrDefault(p => p.LaborID == pass.LaborID && p.Status == "Pending");
                if (existing != null)
                    return BadRequest(new { message = $"A pending pass already exists for LaborID: {pass.LaborID}" });

                pass.Status = "Pending";
                pass.SubmittedAt = DateTime.Now;
                pass.ApprovedBy = pass.ApprovedBy ?? "";
                pass.RejectionReason = pass.RejectionReason ?? "";
                pass.DigitalSignature = null;

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

        [HttpGet]
        public IActionResult GetAllPasses()
        {
            lock (lockObj)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 📋 Fetching all passes. Total: {passes.Count}");
                return Ok(passes.ToList());
            }
        }

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

        [HttpGet("{laborId}")]
        public IActionResult GetPassByLaborId(string laborId)
        {
            lock (lockObj)
            {
                var pass = passes.FirstOrDefault(p => p.LaborID == laborId);

                if (pass == null)
                    return NotFound(new { message = "Pass not found" });

                return Ok(pass);
            }
        }

        [HttpPost("update-status")]
        public IActionResult UpdatePassStatus([FromBody] StatusUpdateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LaborID))
                return BadRequest(new { message = "LaborID is required" });

            lock (lockObj)
            {
                var pass = passes.FirstOrDefault(p => p.LaborID == request.LaborID);
                if (pass == null)
                    return NotFound(new { message = $"Pass not found for LaborID: {request.LaborID}" });

                pass.Status = request.Status;
                pass.ApprovedBy = request.ApprovedBy ?? "";
                if (DateTime.TryParse(request.ApprovedAt, out DateTime approvedDate))
                    pass.ApprovedAt = approvedDate;
                else
                    pass.ApprovedAt = DateTime.Now;

                pass.RejectionReason = request.Reason ?? "";
                pass.DigitalSignature = request.DigitalSignature;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔄 Pass status updated - LaborID: {pass.LaborID}, Status: {pass.Status}, By: {pass.ApprovedBy}");

                if (pass.DigitalSignature != null)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ✅ Digital signature received: {pass.DigitalSignature.SignatureId}");
                }
            }

            return Ok(new
            {
                message = $"Pass {request.Status.ToLower()} successfully",
                laborID = request.LaborID,
                status = request.Status,
                approvedBy = request.ApprovedBy,
                signatureId = request.DigitalSignature?.SignatureId
            });
        }

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

    public class PassModel
    {
        [JsonPropertyName("laborID")]
        public string LaborID { get; set; } = "";

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = "";

        [JsonPropertyName("dob")]
        public string? DOB { get; set; }

        [JsonPropertyName("contractorName")]
        public string? ContractorName { get; set; }

        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [JsonPropertyName("gateAccess")]
        public string? GateAccess { get; set; }

        [JsonPropertyName("entryDate")]
        public string? EntryDate { get; set; }

        [JsonPropertyName("exitDate")]
        public string? ExitDate { get; set; }

        [JsonPropertyName("entryTime")]
        public string? EntryTime { get; set; }

        [JsonPropertyName("checkoutTime")]
        public string? CheckoutTime { get; set; }

        [JsonPropertyName("labourImageBase64")]
        public string? LabourImageBase64 { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Pending";

        [JsonPropertyName("submittedAt")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

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

        [JsonPropertyName("digitalSignature")]
        public DigitalSignatureData? DigitalSignature { get; set; }
    }

    public class StatusUpdateRequest
    {
        [JsonPropertyName("laborID")]
        public string LaborID { get; set; } = "";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("approvedBy")]
        public string? ApprovedBy { get; set; }

        [JsonPropertyName("approvedAt")]
        public string? ApprovedAt { get; set; }

        [JsonPropertyName("digitalSignature")]
        public DigitalSignatureData? DigitalSignature { get; set; }
    }

    public class DigitalSignatureData
    {
        [JsonPropertyName("signatureId")]
        public string? SignatureId { get; set; }

        [JsonPropertyName("signerName")]
        public string? SignerName { get; set; }

        [JsonPropertyName("signerTitle")]
        public string? SignerTitle { get; set; }

        [JsonPropertyName("signerOrganization")]
        public string? SignerOrganization { get; set; }

        [JsonPropertyName("signedDate")]
        public DateTime SignedDate { get; set; }

        [JsonPropertyName("documentHash")]
        public string? DocumentHash { get; set; }

        [JsonPropertyName("signatureValue")]
        public string? SignatureValue { get; set; }

        [JsonPropertyName("publicKey")]
        public string? PublicKey { get; set; }
    }
}