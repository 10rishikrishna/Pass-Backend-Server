var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();
app.UseStaticFiles();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
Console.WriteLine("API Server running on http://localhost:5135");
Console.WriteLine("Static files accessible at: http://localhost:5135/images/");

app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>CIAL Pass API</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            background: linear-gradient(135deg, #0f0c29, #302b63, #24243e);
            color: white;
            font-family: 'Segoe UI', Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            overflow: hidden;
        }
        .container {
            text-align: center;
            padding: 50px;
            background: rgba(255,255,255,0.05);
            border-radius: 24px;
            border: 1px solid rgba(255,255,255,0.1);
            backdrop-filter: blur(10px);
            box-shadow: 0 25px 50px rgba(0,0,0,0.4);
            max-width: 520px;
            width: 90%;
        }
        .logo {
            background: linear-gradient(135deg, #27AE60, #2ECC71);
            width: 80px;
            height: 80px;
            border-radius: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
            margin: 0 auto 24px;
            box-shadow: 0 10px 30px rgba(39,174,96,0.4);
        }
        h1 {
            font-size: 26px;
            font-weight: 700;
            margin-bottom: 6px;
            letter-spacing: 0.5px;
        }
        .subtitle {
            color: rgba(255,255,255,0.5);
            font-size: 13px;
            margin-bottom: 32px;
            font-style: italic;
        }
        .status-card {
            background: rgba(39,174,96,0.15);
            border: 1px solid rgba(39,174,96,0.3);
            border-radius: 14px;
            padding: 16px 24px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
            margin-bottom: 24px;
        }
        .pulse-dot {
            width: 12px;
            height: 12px;
            background: #27AE60;
            border-radius: 50%;
            animation: pulse 1.5s infinite;
            flex-shrink: 0;
        }
        @keyframes pulse {
            0% { box-shadow: 0 0 0 0 rgba(39,174,96,0.6); }
            70% { box-shadow: 0 0 0 10px rgba(39,174,96,0); }
            100% { box-shadow: 0 0 0 0 rgba(39,174,96,0); }
        }
        .status-text { font-size: 15px; font-weight: 600; color: #2ECC71; }
        .endpoints {
            display: flex;
            flex-direction: column;
            gap: 10px;
            margin-bottom: 28px;
        }
        .endpoint {
            background: rgba(255,255,255,0.05);
            border: 1px solid rgba(255,255,255,0.08);
            border-radius: 10px;
            padding: 12px 18px;
            display: flex;
            align-items: center;
            gap: 12px;
            text-align: left;
        }
        .method {
            background: linear-gradient(135deg, #3498DB, #2980B9);
            color: white;
            font-size: 10px;
            font-weight: 700;
            padding: 3px 8px;
            border-radius: 6px;
            letter-spacing: 0.5px;
            flex-shrink: 0;
        }
        .method.post { background: linear-gradient(135deg, #E67E22, #D35400); }
        .endpoint-path { font-size: 13px; color: rgba(255,255,255,0.8); font-family: monospace; }
        .footer {
            color: rgba(255,255,255,0.3);
            font-size: 11px;
        }
        .uptime {
            display: inline-block;
            background: rgba(255,255,255,0.05);
            border-radius: 20px;
            padding: 4px 14px;
            font-size: 11px;
            color: rgba(255,255,255,0.4);
            margin-top: 16px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>🛂</div>
        <h1>CIAL Entry Pass API</h1>
        <p class='subtitle'>Cochin International Airport Limited</p>

        <div class='status-card'>
            <div class='pulse-dot'></div>
            <span class='status-text'>All Systems Operational</span>
        </div>

        <div class='endpoints'>
            <div class='endpoint'>
                <span class='method'>GET</span>
                <span class='endpoint-path'>/api/passes</span>
            </div>
            <div class='endpoint'>
                <span class='method post'>POST</span>
                <span class='endpoint-path'>/api/passes</span>
            </div>
            <div class='endpoint'>
                <span class='method post'>POST</span>
                <span class='endpoint-path'>/api/passes/update-status</span>
            </div>
        </div>

        <div class='footer'>
            CIAL Security Operations Division
            <div class='uptime'>⚡ Hosted on Render</div>
            <div style='margin-top:12px; color:rgba(255,255,255,0.25); font-size:11px;'>
                Built & maintained by 
                <a href='https://github.com/10rishikrishna' target='_blank' 
                   style='color:rgba(255,255,255,0.45); text-decoration:none; font-weight:600;'>
                   Rishi Krishna
                </a>
            </div>
        </div>
    </div>
</body>
</html>
", "text/html"));

app.Run();