using System.Runtime.Serialization;

namespace Niobium.AI.WebBrowser.Playwright
{
    public enum WebBrowserLoadState
    {
        [EnumMember(Value = "load")]
        Load,
        [EnumMember(Value = "domcontentloaded")]
        DOMContentLoaded,
        [EnumMember(Value = "networkidle")]
        NetworkIdle,
    }
}
