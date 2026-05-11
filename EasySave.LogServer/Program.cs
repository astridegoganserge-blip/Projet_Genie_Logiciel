using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string logDirectory = Path.Combine(AppContext.BaseDirectory, "centralized-logs");
Directory.CreateDirectory(logDirectory);

app.MapGet("/", () => Results.Ok(new
{
    Service = "EasySave Log Server",
    Status = "Running"
}));

app.MapPost("/log", async (HttpRequest request) =>
{
    try
    {
        using JsonDocument document = await JsonDocument.ParseAsync(request.Body);

        string machineName = "UnknownMachine";

        if (document.RootElement.TryGetProperty("MachineName", out JsonElement machineElement))
        {
            machineName = machineElement.GetString() ?? "UnknownMachine";
        }

        var centralizedEntry = new
        {
            ReceivedAt = DateTimeOffset.Now,
            MachineName = machineName,
            Payload = document.RootElement.Clone()
        };

        string fileName = $"{DateTime.Now:yyyy-MM-dd}.ndjson";
        string filePath = Path.Combine(logDirectory, fileName);

        string line = JsonSerializer.Serialize(centralizedEntry);

        await File.AppendAllTextAsync(
            filePath,
            line + Environment.NewLine);

        return Results.Ok(new
        {
            Status = "received",
            MachineName = machineName
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            Status = "invalid_payload",
            Error = ex.Message
        });
    }
});

app.Run("http://0.0.0.0:5000");