using Microsoft.DurableTask;

namespace Niobium.AI
{
    [DurableTask]
    public class ConvertImageReference : TaskActivity<Uri, ImageReference>
    {
        public override async Task<ImageReference> RunAsync(TaskActivityContext context, Uri input)
            => await input.ToImageReferenceAsync();
    }
}
