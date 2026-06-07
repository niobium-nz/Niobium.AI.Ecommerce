using Microsoft.Extensions.AI;

namespace Niobium.AI.Ecommerce.Tools
{
    public class ToolBox(AdsLibraryTool adsLibraryTool) : AI.ToolBox
    {
        public AITool AdsLibraryTool => AIFunctionFactory.Create(adsLibraryTool.SearchAds);
    }
}
