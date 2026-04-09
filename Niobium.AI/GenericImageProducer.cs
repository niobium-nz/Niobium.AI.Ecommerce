namespace Niobium.AI
{
    public abstract class GenericImageProducer<TInput, TOutput>(IImageClientFactory clientFactory) : IImageProducer<TInput, TOutput> where TInput : IImageInstruction
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

        protected virtual Task OnGettingResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<TOutput> GetResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken)
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
            IEnumerable<BinaryData> result = await client.RunAsync(
                 conversationID,
                 prompt,
                 width,
                 height,
                 variantCount: this.VariantCount,
                 references: input.References,
                 cancellationToken: cancellationToken);
            return await this.OnResponseGotAsync(conversationID, input, result, cancellationToken);
        }

        protected abstract Task<TOutput> OnResponseGotAsync(string conversationID, TInput input, IEnumerable<BinaryData> results, CancellationToken cancellationToken);
    }
}
