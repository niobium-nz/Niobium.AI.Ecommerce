using Microsoft.DurableTask;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class CheckDuplicateSignal : TaskActivity<IEnumerable<string>, bool>
    {
        public override async Task<bool> RunAsync(TaskActivityContext context, IEnumerable<string> input)
        {
            string indexDir = $"/artifacts/signal/index";
            if (!Directory.Exists(indexDir))
            {
                return false;
            }

            string[] index = Directory.GetFiles(indexDir, "*.txt");
            return input.Any(adArchiveId => index.Any(f => Path.GetFileNameWithoutExtension(f) == adArchiveId));
        }
    }
}
