using System.Runtime.Serialization;

namespace Niobium.AI.Web
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
