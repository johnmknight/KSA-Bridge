using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace KSABridge;

/// <summary>
/// Lightweight HTTP file server for serving mission control consoles.
/// Uses System.Net.HttpListener — no external dependencies.
/// Serves static files from the "web" directory next to the mod DLL.
/// </summary>
public class StaticFileServer : IDisposable
{
    private HttpListener?         _listener;
    private CancellationTokenSource _cts = new();
    private Task?                 _listenTask;
    private readonly string       _webRoot;
    private readonly int          _port;
    private bool                  _running;

    // Common MIME types for web content
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".htm"]  = "text/html; charset=utf-8",
        [".css"]  = "text/css; charset=utf-8",
        [".js"]   = "application/javascript; charset=utf-8",
        [".json"] = "application/json; charset=utf-8",
        [".png"]  = "image/png",
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"]  = "image/gif",
        [".svg"]  = "image/svg+xml",
        [".ico"]  = "image/x-icon",
        [".woff"] = "font/woff",
        [".woff2"]= "font/woff2",
        [".ttf"]  = "font/ttf",
        [".txt"]  = "text/plain; charset=utf-8",
        [".md"]   = "text/plain; charset=utf-8",
        [".toml"] = "text/plain; charset=utf-8",
        [".xml"]  = "application/xml; charset=utf-8",
    };

    public string WebRoot => _webRoot;
    public int Port       => _port;
    public bool IsRunning => _running;
    public string BaseUrl => $"http://127.0.0.1:{_port}/";

    public StaticFileServer(string webRoot, int port)
    {
        _webRoot = Path.GetFullPath(webRoot);
        _port    = port;
    }

    public void Start()
    {
        if (_running) return;

        if (!Directory.Exists(_webRoot))
        {
            Console.WriteLine($"[KSA-Bridge] Web root not found: {_webRoot}");
            Console.WriteLine($"[KSA-Bridge] HTTP server not started. Create the 'web' directory to enable it.");
            return;
        }

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
            // Also listen on localhost in case 127.0.0.1 doesn't resolve
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _listener.Start();
            _running = true;

            Console.WriteLine($"[KSA-Bridge] ──────────────────────────────────────────");
            Console.WriteLine($"[KSA-Bridge] HTTP server started on port {_port}");
            Console.WriteLine($"[KSA-Bridge] Console URL: http://127.0.0.1:{_port}/");
            Console.WriteLine($"[KSA-Bridge] Serving from: {_webRoot}");
            Console.WriteLine($"[KSA-Bridge] ──────────────────────────────────────────");

            _listenTask = Task.Run(() => ListenLoop(_cts.Token));
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine($"[KSA-Bridge] HTTP server failed to start on port {_port}: {ex.Message}");
            Console.WriteLine($"[KSA-Bridge] The port may be in use. Change [webserver] port in config.");
            _running = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] HTTP server error: {ex.Message}");
            _running = false;
        }
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener != null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync().WaitAsync(ct);
                // Handle each request on a thread pool thread — don't block the listener
                _ = Task.Run(() => HandleRequest(context));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (HttpListenerException)
            {
                // Listener was stopped
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KSA-Bridge] HTTP listener error: {ex.Message}");
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        var request  = context.Request;
        var response = context.Response;

        try
        {
            // Decode URL path and strip leading slash
            string urlPath = Uri.UnescapeDataString(request.Url?.AbsolutePath ?? "/");

            // Default document
            if (urlPath == "/")
                urlPath = "/index.html";

            // Security: prevent path traversal
            string fullPath = Path.GetFullPath(Path.Combine(_webRoot, urlPath.TrimStart('/')));
            if (!fullPath.StartsWith(_webRoot, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 403;
                response.Close();
                return;
            }

            // If path is a directory, look for index.html
            if (Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(fullPath, "index.html");
            }

            if (!File.Exists(fullPath))
            {
                // 404 — return a simple text response
                response.StatusCode  = 404;
                response.ContentType = "text/plain";
                byte[] msg = System.Text.Encoding.UTF8.GetBytes($"404 Not Found: {urlPath}");
                response.ContentLength64 = msg.Length;
                response.OutputStream.Write(msg, 0, msg.Length);
                response.Close();
                return;
            }

            // Determine MIME type
            string ext = Path.GetExtension(fullPath);
            response.ContentType = MimeTypes.TryGetValue(ext, out var mime)
                ? mime
                : "application/octet-stream";

            // CORS headers — allow consoles to fetch local data files
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");

            // Cache control — no caching for development, browsers fetch fresh each time
            response.AddHeader("Cache-Control", "no-cache");

            // Serve the file
            byte[] fileBytes = File.ReadAllBytes(fullPath);
            response.StatusCode      = 200;
            response.ContentLength64 = fileBytes.Length;
            response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] HTTP request error: {ex.Message}");
            try
            {
                response.StatusCode = 500;
                response.Close();
            }
            catch { /* response may already be closed */ }
        }
    }

    public void Dispose()
    {
        _running = false;
        _cts.Cancel();

        try
        {
            _listener?.Stop();
            _listener?.Close();
        }
        catch { }

        _listenTask = null;
        Console.WriteLine("[KSA-Bridge] HTTP server stopped.");
    }
}
