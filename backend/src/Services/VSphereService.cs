using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Backend.Models;

namespace Backend.Services;

public class VSphereService : IVSphereService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISessionStore _sessionStore;
    private readonly IExportTaskStore _taskStore;
    private readonly IMinioService _minioService;

    public VSphereService(IHttpClientFactory httpClientFactory, ISessionStore sessionStore, IExportTaskStore taskStore, IMinioService minioService)
    {
        _httpClientFactory = httpClientFactory;
        _sessionStore = sessionStore;
        _taskStore = taskStore;
        _minioService = minioService;
    }

    private HttpClient CreateClient(string host, string? token = null)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        var client = new HttpClient(handler);
        client.BaseAddress = new Uri($"https://{host}/");
        if (token != null)
        {
            client.DefaultRequestHeaders.Add("vmware-api-session-id", token);
        }
        return client;
    }

    private static string ExtractSessionToken(HttpResponseMessage response, string body)
    {
        if (response.Headers.TryGetValues("vmware-api-session-id", out var headerValues))
        {
            foreach (var value in headerValues)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        var trimmed = body.Trim();
        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
            {
                return root.GetString() ?? string.Empty;
            }

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("value", out var valueElement))
            {
                if (valueElement.ValueKind == JsonValueKind.String)
                {
                    return valueElement.GetString() ?? string.Empty;
                }
            }
        }
        catch (JsonException)
        {
            // Fallback to raw body parsing below.
        }

        return trimmed.Trim('"');
    }

    public async Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // Sanitize host input
        request.Host = request.Host.Replace("https://", "").Replace("http://", "").TrimEnd('/');

        using var client = CreateClient(request.Host);
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{request.Username}:{request.Password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

        // Try vSphere 7+ REST API
        var response = await client.PostAsync("api/session", null, cancellationToken);
        
        // Fallback for older versions if needed, but spec says vSphere 8+
        if (!response.IsSuccessStatusCode)
        {
             // Try legacy REST API path if 404
             if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
             {
                 response = await client.PostAsync("rest/com/vmware/cis/session", null, cancellationToken);
             }
        }
        
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var token = ExtractSessionToken(response, body);
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("無法取得 vSphere session token");
        }

        var sessionId = Guid.NewGuid().ToString();
        _sessionStore.Add(sessionId, new SessionData(request.Host, token, request.Username, request.Password));

        return sessionId;
    }

    public async Task<IReadOnlyCollection<VmInfo>> GetVmsAsync(string sessionId, CancellationToken cancellationToken)
    {
        var session = _sessionStore.Get(sessionId) ?? throw new UnauthorizedAccessException("Invalid Session");
        using var client = CreateClient(session.Host, session.Token);

        var response = await client.GetAsync("api/vcenter/vm", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        // Parse JSON. Structure is usually { value: [ ... ] }
        using var doc = JsonDocument.Parse(content);
        var vms = new List<VmInfo>();
        
        // Handle both array directly or wrapped in "value"
        JsonElement root = doc.RootElement;
        JsonElement items = root;
        
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("value", out var valueArray))
        {
            items = valueArray;
        }

        if (items.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in items.EnumerateArray())
            {
                vms.Add(new VmInfo
                {
                    Id = element.TryGetProperty("vm", out var vmId) ? vmId.GetString() ?? "" : "",
                    Name = element.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                    PowerState = element.TryGetProperty("power_state", out var state) ? state.GetString() ?? "" : "UNKNOWN",
                    ProvisionedBytes = 0 // API might not return this directly in list
                });
            }
        }
        return vms;
    }

    public async Task<Guid> ExportVmAsync(string sessionId, string vmId, string vmName, CancellationToken cancellationToken)
    {
        var session = _sessionStore.Get(sessionId) ?? throw new UnauthorizedAccessException("Invalid Session");
        
        // Create a task to track progress
        var task = _taskStore.Create(sessionId, vmId);
        
        // Run export in background
        _ = Task.Run(async () => 
        {
            try 
            {
                await PerformExport(session, vmId, vmName, task);
            }
            catch (Exception ex)
            {
                task.Status = ExportTaskStatus.Failed;
                task.Error = ex.Message;
                task.DownloadUrl = null;
                Console.WriteLine($"[Export Error] Task {task.Id} for VM {vmId} failed: {ex}");
                _taskStore.Update(task);
            }
        });

        return task.Id;
    }

    private async Task PerformExport(SessionData session, string vmId, string vmName, ExportTask task)
    {
        task.Status = ExportTaskStatus.Running;
        task.Progress = 0;
        _taskStore.Update(task);

        var tempDir = Path.Combine(Path.GetTempPath(), "exports", task.Id.ToString());
        Directory.CreateDirectory(tempDir);
        
        // Use vmName for the file name if provided, otherwise fallback to vmId
        var fileName = string.IsNullOrWhiteSpace(vmName) ? vmId : vmName;
        // Sanitize filename to remove invalid characters
        fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        
        var ovaPath = Path.Combine(tempDir, $"{fileName}.ova");
        var success = false;

        try
        {
            // Construct OVF Tool command
            // vi://user:pass@host/?moref=vmId
            var userEncoded = System.Net.WebUtility.UrlEncode(session.Username);
            var passEncoded = System.Net.WebUtility.UrlEncode(session.Password);
            
            // Log debug info (mask password)
            Console.WriteLine($"[Export Debug] VM ID: {vmId}, Name: {vmName}");
            
            // Try adding type prefix for OVF Tool
            var moref = vmId.StartsWith("vim.VirtualMachine:") ? vmId : $"vim.VirtualMachine:{vmId}";
            var source = $"vi://{userEncoded}:{passEncoded}@{session.Host}/?moref={moref}";

            Console.WriteLine($"[Export Debug] Source URL: vi://{userEncoded}:***@{session.Host}/?moref={moref}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ovftool",
                Arguments = $"--acceptAllEulas --noSSLVerify --diskMode=thin --powerOffSource --X:vimSessionTimeout=1 --X:connectionReconnectCount=3 \"{source}\" \"{ovaPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            
            process.OutputDataReceived += (sender, e) => 
            {
                if (e.Data != null)
                {
                    Console.WriteLine($"[OVFTool] {e.Data}");
                    // Parse progress: "Progress: 10%" or "Disk progress: 10%"
                    var match = Regex.Match(e.Data, @"(?:Disk )?[Pp]rogress: (\d+)%");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int progress))
                    {
                        // Map 0-100 export progress to 0-80 task progress (leave 20 for upload)
                        task.Progress = (int)(progress * 0.8);
                        _taskStore.Update(task);
                    }
                }
            };
            
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null) Console.WriteLine($"[OVFTool Error] {e.Data}");
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"OVF Tool failed with exit code {process.ExitCode}");
            }

            // Upload to Minio
            task.Progress = 80;
            _taskStore.Update(task);

            var bucketName = "exports";
            await _minioService.EnsureBucketExistsAsync(bucketName, CancellationToken.None);
            
            var objectName = $"{fileName}.ova";
            using var stream = File.OpenRead(ovaPath);
            
            await _minioService.UploadAsync(bucketName, objectName, stream, CancellationToken.None);
            
            task.Progress = 100;
            task.Status = ExportTaskStatus.Completed;
            task.DownloadUrl = await _minioService.GetPresignedUrlAsync(bucketName, objectName, TimeSpan.FromHours(1));
            _taskStore.Update(task);
            success = true;
        }
        catch (Exception ex)
        {
            task.Status = ExportTaskStatus.Failed;
            task.Error = ex.Message;
            task.DownloadUrl = null;
            _taskStore.Update(task);
            Console.WriteLine($"[Export Error] Task {task.Id} for VM {vmId} failed: {ex}");
            return;
        }
        finally
        {
            if (success && Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            else if (!success)
            {
                Console.WriteLine($"[Export Debug] Temporary export data kept at: {tempDir}");
            }
        }
    }

    public async Task PowerOffAsync(string sessionId, string vmId, CancellationToken cancellationToken)
    {
        var session = _sessionStore.Get(sessionId) ?? throw new UnauthorizedAccessException("Invalid Session");
        using var client = CreateClient(session.Host, session.Token);

        var response = await client.PostAsync($"api/vcenter/vm/{vmId}/power/stop", null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = $"vSphere power-off failed (status {(int)response.StatusCode}): {body}";

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            message = "無法關機：VM 目前非執行中或狀態不允許";
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            message = "權限不足，請確認 vSphere 帳號具備關機權限";
        }

        throw new InvalidOperationException(message);
    }
}
