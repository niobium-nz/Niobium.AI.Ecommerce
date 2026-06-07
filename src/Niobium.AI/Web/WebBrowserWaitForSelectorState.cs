using System.Runtime.Serialization;

namespace Niobium.AI.Web
{
    public enum WebBrowserWaitForSelectorState
    {
        [EnumMember(Value = "attached")]
        Attached,
        [EnumMember(Value = "detached")]
        Detached,
        [EnumMember(Value = "visible")]
        Visible,
        [EnumMember(Value = "hidden")]
        Hidden
    }
}
