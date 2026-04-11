<div align="center">

# 🛂   Pass API

### *The Live Backend Powering the   Entry Pass System*

[![Platform](https://img.shields.io/badge/Platform-ASP.NET%20Core%209.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com)
[![Language](https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Hosted](https://img.shields.io/badge/Hosted%20on-Render-46E3B7?style=for-the-badge&logo=render)](https://render.com)
[![Live](https://img.shields.io/badge/Status-Live-27AE60?style=for-the-badge)]()

> The central REST API that connects the **Pass Generator** and **Pass Authenticator** apps — handling pass submissions, status updates, digital signatures

**🌐 Live Server:** [https://pass-api-e4so.onrender.com](https://pass-api-e4so.onrender.com)

---

</div>

## 📋 Overview

The **  Pass API** is an ASP.NET Core 9 REST API deployed on Render, acting as the backbone of the entire   Entry Pass ecosystem. It receives pass requests from the Pass Generator app, stores them in memory, and serves them to the Authenticator app for review. Once a decision is made, the API records the approval or rejection along with the officer's digital signature.

It also handles **aerodrome entry pass image generation** — rendering complete pass cards as PNG images using `System.Drawing`.

---

## 🔗 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/passes` | Fetch all current passes |
| `POST` | `/api/passes` | Submit a new pass for approval |
| `POST` | `/api/passes/update-status` | Update pass status (Approve / Reject) |

---

## 📦 Request & Response

### Submit a Pass — `POST /api/passes`
```json
{
  "laborID": "LAB001",
  "fullName": "John Doe",
  "dob": "1990-01-01",
  "contractorName": "ABC Contractors",
  "area": "Terminal 1",
  "gateAccess": "Gate 3, Gate 5",
  "entryDate": "2025-04-11",
  "exitDate": "2025-04-11",
  "entryTime": "08:00 AM",
  "checkoutTime": "06:00 PM",
  "labourImageBase64": "..."
}
```

## ✨ Features

### 🖼️ Pass Image Generation
Generates aerodrome entry permit PNG images using `System.Drawing` with:
- logo and header
- Employee photo (from Base64)
- Labor ID, contractor, DOB details
- Validity dates and access gate info
- Approval signature overlay
- "UNDER ESCORT" banner and "TEMPORARY PASS" footer

### 🔐 Digital Signature Support
Accepts and stores RSA digital signature data alongside approvals, including signer identity, title, organization, and a unique signature ID.

### 🔄 CORS Enabled
Fully open CORS policy allowing the desktop apps (Pass Generator and Authenticator) to communicate with the API from any origin.

### 📁 Static File Serving
Serves generated pass images via `/images/` from the `wwwroot` folder.

---

## 🏗️ Architecture

```
Pass_Api Maker/
│
├── Controllers/
│   └── PassesController.cs     # GET & POST endpoints
│
├── PassGeneratorService.cs     # Pass image rendering (System.Drawing)
├── DigitalSignatureService.cs  # Signature handling
├── Program.cs                  # App bootstrap + homepage route
├── appsettings.json
└── Dockerfile                  # Docker deployment config
```

---

## 🚀 Deployment

Hosted on **Render** using Docker.

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5135

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pass_Api Maker.dll"]
```

### Running Locally
```bash
cd "Pass_Api Maker"
dotnet run
# API runs at http://localhost:5135
```

---

## 🔗 Related Projects

| Project | Description |
|---------|-------------|
| **Entry Pass Generator** | Windows Forms app for contractors to submit pass requests |
| **Entry Pass Authenticator** | .NET MAUI app for security personnel to approve/reject passes |

---

## ⚠️ Note on Data Persistence

Passes are currently stored **in memory**. Data resets if the server restarts. Persistent database support (PostgreSQL) is planned for a future update.

---

<div align="center">

Built & maintained by [Rishi Krishna](https://github.com/10rishikrishna)


</div>
