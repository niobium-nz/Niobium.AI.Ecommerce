using Niobium.AI.Web;

namespace Niobium.AI.Playwright
{
    public class PlaywrightOptions
    {
        public WebBrowserKind Browser { get; set; } = WebBrowserKind.Chrome;

        public bool Headless { get; set; } = true;

        public bool AcceptDownloads { get; set; } = false;

        public string? BaseUrl { get; set; }

        public string? UserDataDir { get; set; } = "/BrowserUserData";

        public string? ExecutablePath { get; set; }

        public string? Locale { get; set; }

        public float? SlowMo { get; set; }

        public float? TimeoutMs { get; set; }

        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36";

        public int ViewportWidth { get; set; } = 1920;

        public int ViewportHeight { get; set; } = 1080;
    }
}
