namespace Niobium.AI
{
    public abstract class GenericImageProducer(IImageClientFactory clientFactory) : IImageProducer
    {
        public abstract string Id { get; }

        protected virtual string Model => Models.GPT_IMAGE_LATEST;

        protected virtual int VariantCount => 1;

        protected virtual Type InstructionsResourceBaseType => this.GetType();

        protected virtual async Task<string> GetInstructionsAsync(CancellationToken cancellationToken)
        {
            string resource = $"{this.InstructionsResourceBaseType.Namespace}.{this.Id}.md";
            using Stream stream = this.InstructionsResourceBaseType.Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Instructions resource not found: {resource}");
            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        protected virtual Task OnGettingResponseAsync(string conversationID, ImageInstruction input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<IEnumerable<Uri>> GetResponseAsync(string conversationID, ImageInstruction input, CancellationToken cancellationToken)
        {
            int width, height;
            switch (input.Form)
            {
                case ImageForm.Squared:
                    width = height = 1024;
                    break;
                case ImageForm.Vertical:
                    width = 1024;
                    height = 1536;
                    break;
                case ImageForm.Horizontal:
                    width = 1536;
                    height = 1024;
                    break;
                default:
                    width = height = 1024;
                    break;
            }

            await this.OnGettingResponseAsync(conversationID, input, cancellationToken);

            string prompt = await this.GetInstructionsAsync(cancellationToken);
            IImageClient client = clientFactory.CreateClient(this.Model);
            IEnumerable<Uri> result = await client.RunAsync(
                 conversationID,
                 prompt,
                 width,
                 height,
                 variantCount: this.VariantCount,
                 references: input.References,
                 cancellationToken: cancellationToken);
            return await this.OnResponseGotAsync(conversationID, input, result, cancellationToken);
        }

        protected virtual Task<IEnumerable<Uri>> OnResponseGotAsync(string conversationID, ImageInstruction input, IEnumerable<Uri> results, CancellationToken cancellationToken)
            => Task.FromResult(results);
    }
}
