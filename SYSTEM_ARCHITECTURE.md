# 🛂 CIAL Entry Pass Management System - Complete Architecture

---

## 📋 Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Execution Flow (START → END)](#execution-flow-start--end)
4. [File Execution Details](#file-execution-details)
5. [API Endpoints](#api-endpoints)
6. [Request-Response Examples](#request-response-examples)
7. [Security & Deployment](#security--deployment)
8. [Troubleshooting](#troubleshooting)

---

# System Overview

## What is CIAL Entry Pass System?

A **3-repository ecosystem** for managing labour entry passes at Cochin International Airport Limited (CIAL).

### Three Main Components:

| Component | Type | Purpose |
|-----------|------|---------|
| **Pass-Backend-Server** | ASP.NET Core REST API | Central hub for authentication & pass management |
| **Entry-Pass-Generator-CIAL** | WinForms Desktop App | Labour registration & pass request submission |
| **Entry-Pass-Authenticator-CIAL** | .NET MAUI Desktop App | Security dashboard for approval/rejection |

---

# Architecture Diagram

```
╔═════════════════════════════════════════════════════════════════════════════╗
║                CIAL ENTRY PASS MANAGEMENT SYSTEM                          ║
╚═════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────────┐              ┌─────────────────────────────────┐
│  PASS GENERATOR (WinForms)      │              │  PASS AUTHENTICATOR (.NET MAUI) │
│  Entry-Pass-Generator-CIAL      │              │  Entry-Pass-Authenticator-CIAL  │
│                                 │              │                                 │
│  👤 User Role: Admin/Registrar  │              │  👤 User Role: Security Officer │
│                                 │              │                                 │
│  📌 Features:                   │              │  📌 Features:                   │
│  ├─ Photo Capture              │              │  ├─ Real-time Dashboard         │
│  ├─ Labour Registration         │              │  ├─ Digital Signatures (RSA)    │
│  ├─ Contractor Details          │              │  ├─ Approve/Reject Passes       │
│  ├─ Gate Access Permissions     │              │  ├─ Batch Operations            │
│  └─ Validity Period Settings    │              │  └─ CSV Export                  │
│                                 │              │                                 │
│  📤 API Calls:                  │              │  📥 API Calls:                  │
│  ├─ POST /api/passes            │              │  ├─ GET /api/passes             │
│  └─ POST /api/auth/login        │              │  └─ POST /api/passes/update-status
│                                 │              │                                 │
└────────────────────┬────────────┘              └────────────────┬────────────────┘
                     │                                            │
                     │          REST API Communication           │
                     │     (HTTP - localhost:5135)               │
                     └──────────────────┬──────────────────────────┘
                                        │
                ┌───────────────────────▼───────────────────────┐
                │  BACKEND API (ASP.NET Core)                   │
                │  Pass-Backend-Server                          │
                │                                               │
                │  🎯 Central Hub                               │
                │                                               │
                │  📦 Components:                               │
                │  ├─ AuthController                            │
                │  │  ├─ Login/Register                         │
                │  │  ├─ Password Management                    │
                │  │  └─ Token Generation                       │
                │  │                                            │
                │  ├─ PassesController                          │
                │  │  ├─ Submit Passes                          │
                │  │  ├─ Fetch Passes                           │
                │  │  ├─ Update Status (Approve/Reject)         │
                │  │  └─ Mark as Downloaded                     │
                │  │                                            │
                │  ├─ DigitalSignatureService                   │
                │  │  ├─ RSA Key Generation (2048-bit)          │
                │  │  ├─ Sign Pass Data (SHA-256)               │
                │  │  └─ Verify Signatures                      │
                │  │                                            │
                │  └─ PassGeneratorService                      │
                │     ├─ Generate Pass Images                   │
                │     └─ Save to Disk                           │
                │                                               │
                │  💾 Storage:                                  │
                │  └─ In-Memory List<PassModel>                 │
                │                                               │
                └───────────────────────────────────────────────┘
```

---

# Execution Flow (START → END)

## 🔄 Complete User Journey

### Step 1️⃣: BACKEND SERVER STARTS

```
▶ Program.cs Executes
   ↓
▶ Services Registered
   ├─ Controllers enabled
   ├─ CORS configured
   └─ Dependency Injection setup
   ↓
▶ Middleware Configured
   ├─ Static Files enabled
   ├─ CORS activated
   ├─ Authorization setup
   └─ Routes mapped
   ↓
▶ Server Starts
   └─ Listening on http://localhost:5135 ✅
   
   📝 Console Output:
   [14:32:00] API Server running on http://localhost:5135
```

---

### Step 2️⃣: LABOUR REGISTRATION (Generator App)

```
👤 USER LAUNCHES GENERATOR APP
   ↓
🔐 LOGIN PAGE
   ├─ Enter Username/Password
   └─ Click "Login"
   ↓
📡 API CALL: POST /api/auth/login
   │
   └─→ AuthController.Login()
       ├─ Validate username
       ├─ Hash & verify password (SHA-256)
       ├─ Generate auth token (32-byte random)
       └─ Return token ✅
   ↓
✅ LOGIN SUCCESSFUL
   └─ Dashboard displayed
   ↓
📋 LABOUR REGISTRATION FORM
   ├─ Photo: Capture from camera
   ├─ Labour Details:
   │  ├─ Labour ID (e.g., "LABOR123")
   │  ├─ Full Name (e.g., "John Doe")
   │  ├─ Date of Birth (e.g., "1990-01-15")
   │  ├─ Contractor Name (e.g., "ABC Construction")
   │  └─ Work Area (e.g., "Terminal 1")
   ├─ Gate Access:
   │  ├─ Permitted Gates (e.g., "Gate A, Gate B")
   │  └─ Area Access (e.g., "Terminal 1")
   └─ Validity:
      ├─ Entry Date & Time (e.g., "2025-05-22 08:00 AM")
      └─ Exit Date & Time (e.g., "2025-05-23 04:00 PM")
   ↓
🔘 CLICK "SUBMIT PASS"
   ↓
📡 API CALL: POST /api/passes
   │
   └─→ PassesController.SubmitPass()
       ├─ Validate all required fields
       ├─ Check for duplicate pending passes
       ├─ Set Status = "Pending"
       ├─ Add to in-memory List<PassModel>
       └─ Return success response ✅
   ↓
📝 Console Output:
[14:35:22] ✅ New pass submitted - LaborID: LABOR123, Name: John Doe
```

---

### Step 3️⃣: SECURITY OFFICER REVIEWS (Authenticator App)

```
👮 SECURITY OFFICER LAUNCHES AUTHENTICATOR APP
   ↓
🔄 CONTINUOUS POLLING (Every 30 seconds)
   │
   └─→ API CALL: GET /api/passes
       │
       └─→ PassesController.GetAllPasses()
           ├─ Fetch all passes from List<PassModel>
           ├─ Return complete list
           └─ Log: "📋 Fetching all passes. Total: 5"
   ↓
📊 DASHBOARD DISPLAYED
   ├─ Stats Cards:
   │  ├─ Total Entries: 5
   │  ├─ Pending: 1 (Amber)
   │  ├─ Approved: 3 (Green)
   │  └─ Rejected: 1 (Red)
   ├─ 3-Column Card Grid
   ├─ Search by Name/ID
   └─ Filter by Status
   ↓
🔍 OFFICER REVIEWS JOHN DOE'S PASS
   ├─ Click on pass card
   ├─ View all details:
   │  ├─ Photo preview
   │  ├─ Labour information
   │  ├─ Contractor details
   │  ├─ Gate access permissions
   │  └─ Validity period
   └─ Two action buttons available:
      ├─ APPROVE button
      └─ REJECT button
```

---

### Step 4️⃣: OFFICER APPROVES PASS

```
🔘 OFFICER CLICKS "APPROVE"
   ↓
🔐 GENERATE DIGITAL SIGNATURE
   │
   ├─ Step 1: Serialize Pass to JSON
   │  └─ Convert PassModel object to JSON string
   │
   ├─ Step 2: Create SHA-256 Hash
   │  └─ Hash JSON string (creates document fingerprint)
   │
   ├─ Step 3: Sign with RSA Private Key
   │  ├─ Use 2048-bit RSA key pair
   │  └─ Sign the hash with private key
   │
   ├─ Step 4: Generate Signature ID
   │  └─ Create unique ID (e.g., "SIG-A1B2C3D4E5F6")
   │
   └─ Step 5: Create DigitalSignatureData Object
      ├─ SignatureId: "SIG-A1B2C3D4E5F6"
      ├─ SignerName: "Mr. Sharma"
      ├─ SignerTitle: "Assistant GM - Security"
      ├─ SignerOrganization: "CIAL"
      ├─ SignedDate: 2025-05-22 14:37:00
      ├─ DocumentHash: "abc123def456..."
      ├─ SignatureValue: "qwerty1234567..."
      └─ PublicKey: "<RSAKeyValue>..."
   ↓
📡 API CALL: POST /api/passes/update-status
   │
   ├─ Request Body:
   │  {
   │    "laborID": "LABOR123",
   │    "status": "Approved",
   │    "approvedBy": "Mr. Sharma",
   │    "approvedAt": "2025-05-22T14:37:00",
   │    "reason": null,
   │    "digitalSignature": { ... }
   │  }
   │
   └─→ PassesController.UpdatePassStatus()
       ├─ Find pass by LaborID
       ├─ Update Status = "Approved"
       ├─ Set ApprovedBy = "Mr. Sharma"
       ├─ Store DigitalSignatureData
       └─ Return success response ✅
   ↓
📝 Console Output:
[14:37:22] 🔄 Pass status updated - LaborID: LABOR123, Status: Approved, By: Mr. Sharma
[14:37:22] ✅ Digital signature received: SIG-A1B2C3D4E5F6
```

---

### Step 5️⃣: GENERATE PHYSICAL PASS

```
🔨 ADMIN GENERATES PASS IMAGE
   │
   └─→ PassGeneratorService.GenerateApprovedPass()
       │
       ├─ Create 468x564 pixel bitmap
       │
       ├─ Draw Components (in order):
       │  ├─ Dotted border (decoration)
       │  ├─ CIAL logo (green square)
       │  ├─ "UNDER ESCORT" banner (red vertical)
       │  ├─ "AERODOME ENTRY PERMIT" header
       │  ├─ Labour photo (112x160)
       │  ├─ Left side details (Labour ID, Contractor)
       │  ├─ Right side details (DOB)
       │  ├─ Validity section (Entry/Exit dates)
       │  ├─ Access Gates section (Gates permitted)
       │  ├─ Officer signature image
       │  └─ Blue "TEMPORARY PASS" footer
       │
       └─ Save to: Documents/CIAL_Entry_Passes/
          Pass_LABOR123_20250522_143722.png ✅
   ↓
✅ PHYSICAL PASS GENERATED
   └─ Ready for printing & delivery
```

---

# File Execution Details

## 📁 File 1: Program.cs

**Location:** `Pass_Api Maker/Program.cs`  
**Execution Order:** 1️⃣ First (Always)  
**Language:** C#  
**Purpose:** ASP.NET Core application entry point

### File Structure:

```csharp
────────────────────────────────────────────────────────────────
LINE 1-11: SERVICE REGISTRATION
────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
│
├─ Line 2: AddControllers()
│  └─ Enables MVC/API controller discovery & routing
│
└─ Lines 3-11: AddCors()
   └─ Configures Cross-Origin Resource Sharing
      ├─ AllowAnyOrigin() - Accept requests from any domain
      ├─ AllowAnyMethod() - Accept GET, POST, PUT, DELETE, etc.
      └─ AllowAnyHeader() - Accept any HTTP headers


────────────────────────────────────────────────────────────────
LINE 12-16: MIDDLEWARE CONFIGURATION
────────────────────────────────────────────────────────────────

var app = builder.Build();
│
├─ Line 13: UseStaticFiles()
│  └─ Enables serving of static files (images, CSS, JS)
│
├─ Line 14: UseCors()
│  └─ Activates CORS middleware for this request
│
├─ Line 15: UseAuthorization()
│  └─ Checks authorization for protected endpoints
│
└─ Line 16: MapControllers()
   └─ Maps controller routes automatically

   ⚠️ ORDER MATTERS! 
   Each middleware processes request in this order:
   UseStaticFiles → UseCors → UseAuthorization → MapControllers


────────────────────────────────────────────────────────────────
LINE 17-18: CONSOLE LOGGING
────────────────────────────────────────────────────────────────

Console.WriteLine("API Server running on http://localhost:5135");
Console.WriteLine("Static files accessible at: http://localhost:5135/images/");

   📝 Output:
   [14:32:00] API Server running on http://localhost:5135
   [14:32:00] Static files accessible at: http://localhost:5135/images/


────────────────────────────────────────────────────────────────
LINE 20-175: HOME PAGE ROUTE
────────────────────────────────────────────────────────────────

app.MapGet("/", () => Results.Content(@"...HTML...", "text/html"));

   When user visits: http://localhost:5135/
   ├─ Returns: Beautiful HTML dashboard
   ├─ Shows: "Labour Pass Backend Server"
   ├─ Status: "All Systems Operational" ✅
   └─ Lists: Available API endpoints


────────────────────────────────────────────────────────────────
LINE 177: SERVER STARTUP (BLOCKING CALL)
────────────────────────────────────────────────────────────────

app.Run();

   🚀 STARTS SERVER - Runs indefinitely!
   ├─ Listens on http://localhost:5135
   ├─ Waits for incoming HTTP requests
   ├─ Routes requests to appropriate controllers
   ├─ Executes business logic
   ├─ Returns responses
   └─ Repeats until application stops

   ⚠️ app.Run() NEVER RETURNS
   Code after this line will NOT execute
   unless server crashes or is stopped
```

### Execution Summary:

```
Program Execution Timeline:
─────────────────────────────
Time: 0ms
│
├─ Create WebApplicationBuilder
├─ Register services (Controllers, CORS)
│
├─ Build application instance
├─ Configure middleware pipeline
├─ Register routes
│
├─ Print console messages
│
└─ Call app.Run()
   │
   └─ 🟢 SERVER RUNNING
      ├─ Listening on port 5135
      ├─ Ready for requests
      └─ ∞ Waits indefinitely
```

---

## 📁 File 2: AuthController.cs

**Location:** `Pass_Api Maker/Authcontroller.cs`  
**Execution Order:** 2️⃣ Called when user logs in  
**Language:** C#  
**Purpose:** User authentication & password management

### Route: `/api/auth`

### Endpoints & Methods:

#### 🔐 METHOD 1: Register New User

```
Endpoint: POST /api/auth/register

Request Body:
{
  "username": "admin",
  "password": "SecurePassword123!"
}

Execution Steps:
├─ Check if username already exists
│  ├─ If YES: Return 400 Bad Request ❌
│  │   { "message": "User already exists" }
│  │
│  └─ If NO: Continue
│
├─ Hash password using SHA-256
│  ├─ Input: "SecurePassword123!"
│  └─ Output: "abc123def456..." (128 chars)
│
├─ Create User object:
│  {
│    Username: "admin",
│    PasswordHash: "abc123def456...",
│    CreatedAt: 2025-05-22 14:32:00
│  }
│
├─ Store in Dictionary<string, User>
│  users["admin"] = newUser
│
└─ Return 200 OK ✅
   {
     "message": "User registered successfully",
     "username": "admin"
   }
```

#### 🔑 METHOD 2: Login User

```
Endpoint: POST /api/auth/login

Request Body:
{
  "username": "admin",
  "password": "SecurePassword123!"
}

Execution Steps:
├─ Check if username exists in dictionary
│  ├─ If NO: Return 401 Unauthorized ❌
│  │   { "message": "Invalid username or password" }
│  │
│  └─ If YES: Continue
│
├─ Verify password
│  ├─ Hash provided password (SHA-256)
│  ├─ Compare with stored hash
│  │
│  ├─ If NO MATCH: Return 401 ❌
│  │   { "message": "Invalid username or password" }
│  │
│  └─ If MATCH: Continue
│
├─ Generate authentication token
│  ├─ Create 32 random bytes
│  ├─ Encode as Base64 string
│  └─ Output: "XyZ9aB2cD4eF6..." (44 chars)
│
└─ Return 200 OK ✅
   {
     "message": "Login successful",
     "username": "admin",
     "token": "XyZ9aB2cD4eF6..."
   }
```

#### 🔒 METHOD 3: Generate Secure Password

```
Endpoint: POST /api/auth/generate-password

Request Body:
{
  "length": 16,
  "includeUppercase": true,
  "includeLowercase": true,
  "includeNumbers": true,
  "includeSpecialChars": true
}

Execution Steps:
├─ Build character set
│  ├─ If includeUppercase: Add "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
│  ├─ If includeLowercase: Add "abcdefghijklmnopqrstuvwxyz"
│  ├─ If includeNumbers: Add "0123456789"
│  └─ If includeSpecialChars: Add "!@#$%^&*()_+-=[]{}|;:,.<>?"
│
├─ Generate random password
│  ├─ Create 16 random bytes
│  ├─ For each position:
│  │  └─ Select random character from set
│  │
│  └─ Result: "Xy9!aB2$cD4#eF6@"
│
└─ Return 200 OK ✅
   {
     "password": "Xy9!aB2$cD4#eF6@"
   }
```

#### 🔄 METHOD 4: Change Password

```
Endpoint: POST /api/auth/change-password

Request Body:
{
  "username": "admin",
  "oldPassword": "OldPassword123!",
  "newPassword": "NewPassword456!"
}

Execution Steps:
├─ Find user by username
│  ├─ If NOT FOUND: Return 404 Not Found ❌
│  │
│  └─ If FOUND: Continue
│
├─ Verify old password
│  ├─ Hash provided old password (SHA-256)
│  ├─ Compare with stored hash
│  │
│  ├─ If NO MATCH: Return 401 Unauthorized ❌
│  │
│  └─ If MATCH: Continue
│
├─ Update to new password
│  ├─ Hash new password (SHA-256)
│  └─ Store new hash in user object
│
└─ Return 200 OK ✅
   {
     "message": "Password changed successfully"
   }
```

### Data Storage:

```
Static Dictionary: users

Structure:
┌──────────────────────────────────────────────────────┐
│ Key: "admin"                                         │
│ Value: {                                             │
│   Username: "admin"                                  │
│   PasswordHash: "abc123def456..." (SHA-256)          │
│   CreatedAt: 2025-05-22 14:32:00                    │
│ }                                                    │
└──────────────────────────────────────────────────────┘

⚠️ IN-MEMORY ONLY
   - Data cleared on application restart
   - No persistence to database
```

---

## 📁 File 3: PassesController.cs

**Location:** `Pass_Api Maker/Controllers/PassesController.cs`  
**Execution Order:** 3️⃣ Called when pass actions occur  
**Language:** C#  
**Purpose:** Pass lifecycle management (Create, Read, Update)

### Route: `/api/passes`

### 7️⃣ Endpoints Overview:

| # | Method | Endpoint | Purpose |
|---|--------|----------|---------|
| 1 | POST | `/api/passes` | Submit new pass request |
| 2 | GET | `/api/passes` | Fetch all passes |
| 3 | GET | `/api/passes/approved` | Fetch approved passes only |
| 4 | GET | `/api/passes/{laborId}` | Fetch specific pass |
| 5 | POST | `/api/passes/update-status` | Approve/Reject pass |
| 6 | POST | `/api/passes/mark-downloaded/{laborId}` | Mark as downloaded |
| 7 | DELETE | `/api/passes/clear` | Clear all passes (testing) |

### Detailed Endpoint Breakdown:

#### ✏️ ENDPOINT 1: Submit New Pass

```
POST /api/passes

Request Body:
{
  "laborID": "LABOR123",
  "fullName": "John Doe",
  "dob": "1990-01-15",
  "contractorName": "ABC Construction",
  "area": "Terminal 1",
  "gateAccess": "Gate A, Gate B",
  "entryDate": "2025-05-22",
  "entryTime": "08:00 AM",
  "exitDate": "2025-05-23",
  "checkoutTime": "04:00 PM",
  "labourImageBase64": "iVBORw0KGgoAAAANS..." (base64 encoded image)
}

Processing Flow:
├─ VALIDATION
│  ├─ Check: Pass object not null ✓
│  ├─ Check: LaborID not empty ✓
│  ├─ Check: FullName not empty ✓
│  │
│  └─ If validation fails:
│     └─ Return 400 Bad Request ❌
│
├─ CHECK FOR DUPLICATES
│  ├─ Search List<PassModel> for:
│  │  └─ LaborID == "LABOR123" AND Status == "Pending"
│  │
│  └─ If duplicate found:
│     └─ Return 400 Bad Request ❌
│        { "message": "A pending pass already exists..." }
│
├─ INITIALIZE PASS
│  ├─ Status = "Pending"
│  ├─ SubmittedAt = DateTime.Now
│  ├─ ApprovedBy = ""
│  ├─ RejectionReason = ""
│  └─ DigitalSignature = null
│
├─ STORE IN MEMORY
│  └─ passes.Add(newPass)
│
├─ LOGGING
│  └─ 📝 [14:35:22] ✅ New pass submitted - 
│              LaborID: LABOR123, Name: John Doe
│
└─ Return 200 OK ✅
   {
     "message": "Pass submitted successfully",
     "laborID": "LABOR123",
     "status": "Pending"
   }
```

#### 📋 ENDPOINT 2: Fetch All Passes

```
GET /api/passes

No Request Body

Processing Flow:
├─ THREAD-SAFE ACCESS
│  └─ lock(lockObj) { ... }
│
├─ FETCH ALL
│  └─ Convert List<PassModel> to array
│
├─ LOGGING
│  └─ 📝 [14:33:15] 📋 Fetching all passes. Total: 5
│
└─ Return 200 OK ✅
   [
     {
       "laborID": "LABOR123",
       "fullName": "John Doe",
       "status": "Pending",
       "submittedAt": "2025-05-22T14:35:22",
       ...
     },
     ...
   ]
```

#### ✅ ENDPOINT 3: Fetch Approved Passes

```
GET /api/passes/approved

No Request Body

Processing Flow:
├─ THREAD-SAFE ACCESS
│  └─ lock(lockObj) { ... }
│
├─ FILTER PASSES
│  └─ Where(p => p.Status == "Approved")
│
├─ LOGGING
│  └─ 📝 [14:33:15] ✅ Fetching approved passes. Count: 3
│
└─ Return 200 OK ✅
   [
     { "laborID": "LABOR001", "status": "Approved", ... },
     { "laborID": "LABOR002", "status": "Approved", ... },
     { "laborID": "LABOR003", "status": "Approved", ... }
   ]
```

#### 🔍 ENDPOINT 4: Get Specific Pass

```
GET /api/passes/{laborId}

URL Parameter:
  laborId = "LABOR123"

Processing Flow:
├─ THREAD-SAFE ACCESS
│  └─ lock(lockObj) { ... }
│
├─ SEARCH
│  └─ FirstOrDefault(p => p.LaborID == "LABOR123")
│
├─ IF NOT FOUND
│  └─ Return 404 Not Found ❌
│     { "message": "Pass not found" }
│
└─ IF FOUND
   └─ Return 200 OK ✅
      {
        "laborID": "LABOR123",
        "fullName": "John Doe",
        "status": "Pending",
        ...
      }
```

#### 🔄 ENDPOINT 5: Update Pass Status

```
POST /api/passes/update-status

Request Body:
{
  "laborID": "LABOR123",
  "status": "Approved",
  "approvedBy": "Mr. Sharma",
  "approvedAt": "2025-05-22T14:37:00",
  "reason": null,
  "digitalSignature": {
    "signatureId": "SIG-A1B2C3D4E5F6",
    "signerName": "Mr. Sharma",
    "signerTitle": "Assistant GM - Security",
    "signerOrganization": "CIAL",
    "signedDate": "2025-05-22T14:37:00",
    "documentHash": "abc123def456...",
    "signatureValue": "qwerty1234567...",
    "publicKey": "<RSAKeyValue>..."
  }
}

Processing Flow:
├─ VALIDATION
│  ├─ Check: Request not null ✓
│  ├─ Check: LaborID not empty ✓
│  │
│  └─ If validation fails:
│     └─ Return 400 Bad Request ❌
│
├─ THREAD-SAFE ACCESS
│  └─ lock(lockObj) { ... }
│
├─ FIND PASS
│  ├─ Search by LaborID
│  │
│  └─ If NOT FOUND:
│     └─ Return 404 Not Found ❌
│
├─ UPDATE STATUS
│  ├─ Status = "Approved" (or "Rejected", "Blacklisted")
│  ├─ ApprovedBy = "Mr. Sharma"
│  ├─ ApprovedAt = DateTime.Now
│  └─ RejectionReason = reason (if rejected)
│
├─ STORE SIGNATURE
│  └─ DigitalSignature = signature object
│
├─ LOGGING
│  ├─ 📝 [14:37:22] 🔄 Pass status updated - 
│  │         LaborID: LABOR123, Status: Approved, By: Mr. Sharma
│  │
│  └─ If signature present:
│     📝 [14:37:22] ✅ Digital signature received: SIG-A1B2C3D4E5F6
│
└─ Return 200 OK ✅
   {
     "message": "Pass approved successfully",
     "laborID": "LABOR123",
     "status": "Approved",
     "approvedBy": "Mr. Sharma",
     "signatureId": "SIG-A1B2C3D4E5F6"
   }
```

#### 📥 ENDPOINT 6: Mark as Downloaded

```
POST /api/passes/mark-downloaded/{laborId}

URL Parameter:
  laborId = "LABOR123"

Processing Flow:
├─ THREAD-SAFE ACCESS
│  └─ lock(lockObj) { ... }
│
├─ FIND PASS
│  └─ FirstOrDefault(p => p.LaborID == "LABOR123")
│
├─ UPDATE FLAGS
│  ├─ IsDownloaded = true
│  └─ DownloadedAt = DateTime.Now
│
├─ LOGGING
│  └─ 📝 [14:37:23] 📥 Pass marked as downloaded - 
│              LaborID: LABOR123
│
└─ Return 200 OK ✅
   {
     "message": "Pass marked as downloaded",
     "laborID": "LABOR123"
   }
```

#### 🗑️ ENDPOINT 7: Clear All Passes (Testing)

```
DELETE /api/passes/clear

No Request Body

Processing Flow:
├─ THREAD-SAFE ACCESS
│  └─ lock(lockObj) { ... }
│
├─ GET COUNT
│  └─ count = passes.Count (e.g., 5)
│
├─ CLEAR ALL
│  └─ passes.Clear()
│
├─ LOGGING
│  └─ 📝 [14:37:24] 🗑️ Cleared all passes. Removed: 5
│
└─ Return 200 OK ✅
   {
     "message": "Cleared 5 pass(es)"
   }
```

### Data Storage:

```
Static List: passes

Structure:
┌──────────────────────────────────────────┐
│ PassModel {                              │
│   laborID: "LABOR123"                    │
│   fullName: "John Doe"                   │
│   dob: "1990-01-15"                      │
│   contractorName: "ABC Construction"     │
│   area: "Terminal 1"                     │
│   gateAccess: "Gate A, Gate B"          │
│   entryDate: "2025-05-22"                │
│   entryTime: "08:00 AM"                  │
│   exitDate: "2025-05-23"                 │
│   checkoutTime: "04:00 PM"               │
│   labourImageBase64: "iVBORw0..."        │
│   status: "Approved"                     │
│   submittedAt: 2025-05-22 14:35:22      │
│   approvedBy: "Mr. Sharma"               │
│   approvedAt: 2025-05-22 14:37:22       │
│   rejectionReason: null                  │
│   isDownloaded: false                    │
│   downloadedAt: null                     │
│   digitalSignature: { ... }              │
│ }                                        │
└──────────────────────────────────────────┘

⚠️ IN-MEMORY ONLY
   - Stored in List<PassModel>
   - Thread-safe with lock(lockObj)
   - Cleared on application restart
```

---

## 📁 File 4: DigitalSignatureService.cs

**Location:** `Pass_Api Maker/DigitalSignatureService.cs`  
**Execution Order:** 4️⃣ Called when officer approves/rejects  
**Language:** C#  
**Purpose:** Cryptographic digital signing & verification

### Cryptography Used:

```
┌─────────────────────────────────────────┐
│ ASYMMETRIC ENCRYPTION                   │
│ ├─ Algorithm: RSA (Rivest-Shamir-Adleman)
│ ├─ Key Size: 2048-bit
│ ├─ Key Pair: Public + Private
│ │
│ ├─ Public Key: Can be shared
│ │  └─ Used to verify signatures
│ │
│ └─ Private Key: Must be kept secret
│    └─ Used to create signatures
│
├─ HASH ALGORITHM
│ ├─ Algorithm: SHA-256
│ ├─ Output Size: 256-bit / 32 bytes
│ └─ Purpose: Create document fingerprint
│
└─ SIGNATURE PROCESS
   ├─ Document → SHA-256 Hash
   ├─ Hash → RSA Sign (Private Key)
   └─ Result: Signature Value
```

### Signing Process Flow:

```
1️⃣ SERIALIZE PASS TO JSON
   ├─ Input: PassModel object
   │  {
   │    "laborID": "LABOR123",
   │    "fullName": "John Doe",
   │    "status": "Approved",
   │    ...
   │  }
   │
   └─ Output: JSON string (minified)

2️⃣ CREATE SHA-256 HASH
   ├─ Input: JSON string
   │
   ├─ Process:
   │  ├─ Convert string to UTF-8 bytes
   │  ├─ Apply SHA-256 algorithm
   │  └─ Encode result as Base64
   │
   └─ Output: "abc123def456..." (88 chars Base64)

3️⃣ SIGN WITH RSA PRIVATE KEY
   ├─ Input: Hash value
   │
   ├─ Process:
   │  ├─ Use RSA-2048 private key
   │  ├─ Apply PKCS#1 padding
   │  └─ Create signature
   │
   └─ Output: "qwerty1234567..." (344 chars Base64)

4️⃣ GENERATE SIGNATURE ID
   ├─ Create 16 random bytes
   └─ Format: "SIG-XXXXXXXXXXXX" (12 hex chars)
      └─ Example: "SIG-A1B2C3D4E5F6"

5️⃣ CREATE DIGITALSIGNATUREDATA
   └─ Object containing all signature info:
      ├─ SignatureId
      ├─ SignerName
      ├─ SignerTitle
      ├─ SignerOrganization
      ├─ SignedDate
      ├─ DocumentHash
      ├─ SignatureValue
      └─ PublicKey
```

### Verification Process Flow:

```
1️⃣ RECREATE HASH FROM CURRENT DOCUMENT
   ├─ Input: PassModel object (current state)
   │
   └─ Output: New hash value

2️⃣ COMPARE HASHES
   ├─ New Hash == Stored Hash?
   │
   ├─ If NO: ❌ Document was modified!
   │
   └─ If YES: ✅ Document is unchanged

3️⃣ VERIFY RSA SIGNATURE
   ├─ Load public key from signature
   │
   ├─ Using public key:
   │  ├─ Verify signature value
   │  └─ Against stored hash
   │
   └─ Result: ✅ Valid or ❌ Invalid
```

### Key Management:

```
RSA Key Pair Generation:

1️⃣ CREATE 2048-BIT RSA
   └─ Two related keys:
      ├─ Private Key (secret, never share)
      │  └─ Used for: Signing documents
      │
      └─ Public Key (can share publicly)
         └─ Used for: Verifying signatures

2️⃣ EXPORT KEYS AS XML
   ├─ Private Key XML: Contains both keys
   │  └─ Format: <RSAKeyValue>...</RSAKeyValue>
   │
   └─ Public Key XML: Public key only
      └─ Format: <RSAKeyValue>...</RSAKeyValue>

3️⃣ PERSIST KEYS (Optional)
   ├─ Save to: Documents/CIAL_Signature_Keys/
   ├─ Files:
   │  ├─ cial_public_key.xml
   │  └─ cial_private_key.xml
   │
   └─ Purpose: Reuse same keys on restart
      └─ All signatures verifiable with same public key
```

---

## 📁 File 5: PassGeneratorService.cs

**Location:** `Pass_Api Maker/passgeneratorservice.cs`  
**Execution Order:** 5️⃣ Called when pass is approved  
**Language:** C#  
**Purpose:** Generate professional pass images

### Pass Card Dimensions:

```
Width: 468 pixels
Height: 564 pixels
Format: PNG (24-bit color)
```

### Pass Layout Structure:

```
┌─────────────────────────────────────────────────┐
│ (10,10) CIAL LOGO (75x50)                      │
│ ┌──────────────┐  AERODOME ENTRY PERMIT       │
│ │     CIAL     │  (Header)                     │
│ │   (green)    │                              │
│ └──────────────┘                              │
├─────────────────────────────────────────────────┤
│  │           ┌──────────────────┐             │
│  │ UNDER     │                  │  DOB:       │
│  │ ESCORT    │    PHOTO         │  [DATE]     │
│  │           │    (112x160)     │             │
│  │           └──────────────────┘             │
│  │                                            │
│  ├─ Labour Id: ┌──────────────────┐          │
│  │              │ LABOR123         │          │
│  │              └──────────────────┘          │
│  ├─ Contractor: ABC Construction             │
│  │                                            │
├─────────────────────────────────────────────────┤
│  ┌─────────────────┐  ┌──────────────────┐   │
│  │ Validity        │  │ Access Gates:    │   │
│  │ From: 2025-05-22│  │ Areas: Terminal 1│   │
│  │      08:00 AM   │  │ Gates: Gate A, B │   │
│  │ To:   2025-05-23│  │                  │   │
│  │      04:00 PM   │  │                  │   │
│  └─────────────────┘  └──────────────────┘   │
│                                               │
│  [OFFICER SIGNATURE IMAGE]                    │
│                                               │
├─────────────────────────────────────────────────┤
│         TEMPORARY PASS                         │
│      (Blue background, white text)             │
└─────────────────────────────────────────────────┘
```

### Drawing Process:

```
GenerateApprovedPass()
  ↓
1️⃣ CREATE BITMAP (468 × 564)
   └─ Set high-quality rendering options:
      ├─ SmoothingMode = AntiAlias
      ├─ InterpolationMode = HighQualityBicubic
      └─ TextRenderingHint = AntiAlias

2️⃣ CLEAR BACKGROUND (White)
   └─ Fill entire bitmap with Color.White

3️⃣ DRAW COMPONENTS (IN ORDER)
   ├─ DrawDottedBorder()
   │  └─ Black dotted rectangle around edge
   │
   ├─ DrawCIALLogo()
   │  ├─ Green rectangle (75×50)
   │  ├─ Position: (10, 10)
   │  └─ Text: "CIAL" (white, bold)
   │
   ├─ DrawUnderEscortBanner()
   │  ├─ Red vertical band on left
   │  └─ Text: "UNDER ESCORT" (rotated -90°)
   │
   ├─ DrawHeader()
   │  ├─ Text: "AERODOME ENTRY PERMIT"
   │  └─ Font: Arial 18pt Bold
   │
   ├─ DrawEmployeePhoto()
   │  ├─ Border: Black rectangle (112×160)
   │  ├─ Image: From Base64
   │  └─ Fallback: Light gray placeholder
   │
   ├─ DrawLeftSideDetails()
   │  ├─ Labour ID with box
   │  └─ Contractor Name
   │
   ├─ DrawRightSideDetails()
   │  └─ Date of Birth
   │
   ├─ DrawValiditySection()
   │  ├─ "From:" date and time
   │  └─ "To:" date and time
   │
   ├─ DrawAccessGatesSection()
   │  ├─ Permitted Areas
   │  └─ Permitted Gates
   │
   ├─ DrawFooter()
   │  ├─ Blue background (bottom)
   │  └─ Text: "TEMPORARY PASS" (white, bold)
   │
   └─ DrawApprovalSignature()
      └─ Officer signature image (200×80)

4️⃣ SAVE TO DISK
   ├─ Folder: Documents/CIAL_Entry_Passes/
   │
   ├─ Filename: Pass_{LaborID}_{Timestamp}.png
   │  └─ Example: Pass_LABOR123_20250522_143722.png
   │
   └─ Format: PNG (24-bit)
      └─ File saved ✅
```

### Photo Handling:

```
Input: Base64-encoded image

Process:
├─ Convert Base64 → Byte array
├─ Load from MemoryStream
├─ Draw on pass (112×160 area)
│
└─ If error or empty:
   └─ Draw light gray placeholder

Result:
├─ Photo successfully embedded on pass
└─ Or placeholder shows photo was missing
```

---

# API Endpoints

## Summary Table

| # | Method | Endpoint | Handler | Status |
|---|--------|----------|---------|--------|
| 1 | POST | `/api/auth/register` | AuthController.Register() | ✅ |
| 2 | POST | `/api/auth/login` | AuthController.Login() | ✅ |
| 3 | POST | `/api/auth/generate-password` | AuthController.GeneratePassword() | ✅ |
| 4 | POST | `/api/auth/change-password` | AuthController.ChangePassword() | ✅ |
| 5 | POST | `/api/passes` | PassesController.SubmitPass() | ✅ |
| 6 | GET | `/api/passes` | PassesController.GetAllPasses() | ✅ |
| 7 | GET | `/api/passes/approved` | PassesController.GetApprovedPasses() | ✅ |
| 8 | GET | `/api/passes/{laborId}` | PassesController.GetPassByLaborId() | ✅ |
| 9 | POST | `/api/passes/update-status` | PassesController.UpdatePassStatus() | ✅ |
| 10 | POST | `/api/passes/mark-downloaded/{laborId}` | PassesController.MarkAsDownloaded() | ✅ |
| 11 | DELETE | `/api/passes/clear` | PassesController.ClearAllPasses() | ✅ |

---

# Request-Response Examples

## 📝 Example 1: User Registration

### Request:
```http
POST /api/auth/register HTTP/1.1
Host: localhost:5135
Content-Type: application/json

{
  "username": "admin",
  "password": "SecurePass@123"
}
```

### Response:
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "User registered successfully",
  "username": "admin"
}
```

---

## 🔑 Example 2: User Login

### Request:
```http
POST /api/auth/login HTTP/1.1
Host: localhost:5135
Content-Type: application/json

{
  "username": "admin",
  "password": "SecurePass@123"
}
```

### Response:
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Login successful",
  "username": "admin",
  "token": "AbCdEfGhIjKlMnOpQrStUvWxYz1234567890=="
}
```

---

## 📝 Example 3: Submit Labour Pass

### Request:
```http
POST /api/passes HTTP/1.1
Host: localhost:5135
Content-Type: application/json

{
  "laborID": "LABOR123",
  "fullName": "John Doe",
  "dob": "1990-01-15",
  "contractorName": "ABC Construction",
  "area": "Terminal 1",
  "gateAccess": "Gate A, Gate B",
  "entryDate": "2025-05-22",
  "entryTime": "08:00 AM",
  "exitDate": "2025-05-23",
  "checkoutTime": "04:00 PM",
  "labourImageBase64": "iVBORw0KGgoAAAANS..."
}
```

### Response:
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Pass submitted successfully",
  "laborID": "LABOR123",
  "status": "Pending"
}
```

### Console Output:
```
[14:35:22] ✅ New pass submitted - LaborID: LABOR123, Name: John Doe
```

---

## ✅ Example 4: Get All Passes

### Request:
```http
GET /api/passes HTTP/1.1
Host: localhost:5135
```

### Response:
```json
HTTP/1.1 200 OK
Content-Type: application/json

[
  {
    "laborID": "LABOR123",
    "fullName": "John Doe",
    "dob": "1990-01-15",
    "contractorName": "ABC Construction",
    "area": "Terminal 1",
    "gateAccess": "Gate A, Gate B",
    "entryDate": "2025-05-22",
    "entryTime": "08:00 AM",
    "exitDate": "2025-05-23",
    "checkoutTime": "04:00 PM",
    "labourImageBase64": "iVBORw0KGgoAAAANS...",
    "status": "Pending",
    "submittedAt": "2025-05-22T14:35:22",
    "approvedBy": "",
    "approvedAt": null,
    "rejectionReason": "",
    "isDownloaded": false,
    "downloadedAt": null,
    "digitalSignature": null
  }
]
```

### Console Output:
```
[14:33:15] 📋 Fetching all passes. Total: 1
```

---

## 🔄 Example 5: Approve Pass with Digital Signature

### Request:
```http
POST /api/passes/update-status HTTP/1.1
Host: localhost:5135
Content-Type: application/json

{
  "laborID": "LABOR123",
  "status": "Approved",
  "approvedBy": "Mr. Sharma",
  "approvedAt": "2025-05-22T14:37:00",
  "reason": null,
  "digitalSignature": {
    "signatureId": "SIG-A1B2C3D4E5F6",
    "signerName": "Mr. Sharma",
    "signerTitle": "Assistant GM - Security",
    "signerOrganization": "CIAL",
    "signedDate": "2025-05-22T14:37:00",
    "documentHash": "abc123def456...",
    "signatureValue": "qwerty1234567...",
    "publicKey": "<RSAKeyValue>...</RSAKeyValue>"
  }
}
```

### Response:
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Pass approved successfully",
  "laborID": "LABOR123",
  "status": "Approved",
  "approvedBy": "Mr. Sharma",
  "signatureId": "SIG-A1B2C3D4E5F6"
}
```

### Console Output:
```
[14:37:22] 🔄 Pass status updated - LaborID: LABOR123, Status: Approved, By: Mr. Sharma
[14:37:22] ✅ Digital signature received: SIG-A1B2C3D4E5F6
```

---

## ❌ Example 6: Reject Pass

### Request:
```http
POST /api/passes/update-status HTTP/1.1
Host: localhost:5135
Content-Type: application/json

{
  "laborID": "LABOR123",
  "status": "Rejected",
  "approvedBy": "Mr. Sharma",
  "approvedAt": "2025-05-22T14:40:00",
  "reason": "Invalid documentation",
  "digitalSignature": { ... }
}
```

### Response:
```json
HTTP/1.1 200 OK
Content-Type: application/json

{
  "message": "Pass rejected successfully",
  "laborID": "LABOR123",
  "status": "Rejected",
  "approvedBy": "Mr. Sharma",
  "signatureId": "SIG-X1Y2Z3A4B5C6"
}
```

---

# Security & Deployment

## 🔐 Current Security Measures

| Feature | Implementation | Status |
|---------|-----------------|--------|
| Password Hashing | SHA-256 | ✅ |
| Digital Signatures | RSA-2048 + SHA-256 | ✅ |
| CORS | AllowAnyOrigin | ✅ |
| Thread Safety | lock(lockObj) | ✅ |
| Input Validation | Data checks | ✅ |

## ⚠️ Current Limitations

| Issue | Impact | Severity |
|-------|--------|----------|
| In-Memory Storage | Data lost on restart | 🔴 HIGH |
| No Database | No persistence | 🔴 HIGH |
| No JWT Authentication | Token not validated on requests | 🔴 HIGH |
| AllowAnyOrigin CORS | Security risk in production | 🔴 HIGH |
| No HTTPS | Unencrypted communication | 🔴 HIGH |

## 📋 Production Deployment Checklist

```
DATABASE
☐ Implement SQL Server or PostgreSQL
☐ Create schema for passes, users, signatures
☐ Setup database backups
☐ Enable encryption at rest

AUTHENTICATION
☐ Implement JWT tokens
☐ Add token validation middleware
☐ Implement refresh tokens
☐ Add role-based access control (RBAC)

SECURITY
☐ Enable HTTPS/SSL certificates
☐ Restrict CORS to specific origins
☐ Add API rate limiting
☐ Implement request/response logging
☐ Add exception handling & monitoring

KEY MANAGEMENT
☐ Move RSA keys to Azure Key Vault
☐ Or use HashiCorp Vault
☐ Implement key rotation policy
☐ Never commit keys to version control

MONITORING
☐ Setup Application Insights
☐ Configure error alerting
☐ Monitor API performance
☐ Track usage metrics

DEPLOYMENT
☐ Setup CI/CD pipeline (GitHub Actions)
☐ Configure environment variables
☐ Load testing
☐ Security audit
☐ Performance optimization
```

---

# Troubleshooting

## ❌ Issue 1: API Server Not Responding

### Symptom:
```
Cannot reach http://localhost:5135
Connection refused
```

### Root Causes & Solutions:

| Cause | Check | Solution |
|-------|-------|----------|
| app.Run() not executing | Debug Program.cs line 177 | Verify no errors before app.Run() |
| Port already in use | `netstat -ano \| findstr :5135` | Change port in launchSettings.json |
| Firewall blocking | Check Windows Firewall | Add exception for port 5135 |
| App crashed | Check console for errors | Fix error & restart |

---

## ❌ Issue 2: Pass Not Found (404)

### Symptom:
```
GET /api/passes/LABOR123 → 404 Not Found
```

### Root Causes & Solutions:

| Cause | Check | Solution |
|-------|-------|----------|
| Wrong LaborID | Verify ID spelling | Recheck exact ID |
| App restarted | In-memory data cleared | Resubmit the pass |
| Duplicate pending pass | Try different LaborID | Reject existing & resubmit |

---

## ❌ Issue 3: CORS Error

### Symptom:
```javascript
Access to XMLHttpRequest blocked by CORS policy
```

### Root Causes & Solutions:

| Cause | Check | Solution |
|-------|-------|----------|
| CORS not enabled | Line 14: app.UseCors() | Enable CORS middleware |
| Wrong origin | Check request origin | Update CORS policy |
| Endpoint typo | Verify URL spelling | Fix endpoint path |

---

## ❌ Issue 4: Signature Verification Failed

### Symptom:
```
Signature validation returns false
```

### Root Causes & Solutions:

| Cause | Check | Solution |
|-------|-------|----------|
| Document modified | Hash changed? | Don't modify signed documents |
| Key mismatch | Public key loaded? | Verify key persistence |
| Incomplete signature | All fields present? | Check DigitalSignatureData |

---

## ❌ Issue 5: Duplicate Pass Error

### Symptom:
```json
{
  "message": "A pending pass already exists for LaborID: LABOR123"
}
```

### Root Causes & Solutions:

| Cause | Check | Solution |
|-------|-------|----------|
| Duplicate pending | Status == "Pending" | Reject or approve existing first |
| Multiple submissions | Same LaborID? | Use different ID or wait for approval |

---

## 🧪 Testing Endpoints (cURL Examples)

### Test 1: Register User
```bash
curl -X POST http://localhost:5135/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Pass@123"}'
```

### Test 2: Login
```bash
curl -X POST http://localhost:5135/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Pass@123"}'
```

### Test 3: Get All Passes
```bash
curl -X GET http://localhost:5135/api/passes
```

### Test 4: Clear All Passes
```bash
curl -X DELETE http://localhost:5135/api/passes/clear
```

---

## 📊 Database Schema (For Production)

```sql
-- Users Table
CREATE TABLE Users (
  UserId INT PRIMARY KEY IDENTITY,
  Username NVARCHAR(50) UNIQUE NOT NULL,
  PasswordHash NVARCHAR(MAX) NOT NULL,
  CreatedAt DATETIME NOT NULL
);

-- Passes Table
CREATE TABLE Passes (
  PassId INT PRIMARY KEY IDENTITY,
  LaborID NVARCHAR(50) NOT NULL,
  FullName NVARCHAR(100) NOT NULL,
  DOB DATE,
  ContractorName NVARCHAR(100),
  Area NVARCHAR(100),
  GateAccess NVARCHAR(MAX),
  EntryDate DATE,
  EntryTime TIME,
  ExitDate DATE,
  CheckoutTime TIME,
  LabourImageBase64 NVARCHAR(MAX),
  Status NVARCHAR(20) DEFAULT 'Pending',
  SubmittedAt DATETIME NOT NULL,
  ApprovedBy NVARCHAR(100),
  ApprovedAt DATETIME,
  RejectionReason NVARCHAR(MAX),
  IsDownloaded BIT DEFAULT 0,
  DownloadedAt DATETIME,
  CreatedAt DATETIME DEFAULT GETDATE()
);

-- Digital Signatures Table
CREATE TABLE DigitalSignatures (
  SignatureId NVARCHAR(20) PRIMARY KEY,
  PassId INT NOT NULL FOREIGN KEY REFERENCES Passes(PassId),
  SignerName NVARCHAR(100),
  SignerTitle NVARCHAR(100),
  SignerOrganization NVARCHAR(100),
  SignedDate DATETIME,
  DocumentHash NVARCHAR(MAX),
  SignatureValue NVARCHAR(MAX),
  PublicKey NVARCHAR(MAX)
);
```

---

## 🔗 Repository Links

| Repository | Type | Link |
|------------|------|------|
| **Pass-Backend-Server** | ASP.NET Core API | https://github.com/10rishikrishna/Pass-Backend-Server |
| **Entry-Pass-Generator-CIAL** | WinForms App | https://github.com/10rishikrishna/Entry-Pass-Generator-CIAL |
| **Entry-Pass-Authenticator-CIAL** | .NET MAUI App | https://github.com/10rishikrishna/Entry-Pass-Authenticator-CIAL |

---

**Document Information:**
- 📅 Generated: 2025-05-22
- 👤 Author: Rishi Krishna (@10rishikrishna)
- 📌 System Version: 1.0
- 🏢 Organization: Cochin International Airport Limited (CIAL)
