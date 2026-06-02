namespace Niobium.AI
{
    public abstract class GenericImageProducer<TInput, TOutput>(IImageClientFactory clientFactory) : IImageProducer<TInput, TOutput> where TInput : IImageInstruction
    {
        public abstract string Id { get; }

        protected virtual string Model => Models.GPT_IMAGE_LATEST;

        protected virtual int VariantCount => 1;

        protected virtual Type InstructionsResourceBaseType => this.GetType();

        protected virtual async Task<string> GetInstructionsAsync(TInput input, CancellationToken cancellationToken)
        {
            string resource = $"{this.InstructionsResourceBaseType.Namespace}.{this.Id}.md";
            using Stream stream = this.InstructionsResourceBaseType.Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Instructions resource not found: {resource}");
            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        protected virtual Task OnGettingResponseAsync(TInput input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<TOutput> RunAsync(TInput input, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;
            int width, height;
            switch (input.Form)
            {
                case ImageForm.Square:
                    width = height = 1024;
                    break;
                case ImageForm.Portrait:
                    width = 1024;
                    height = 1536;
                    break;
                case ImageForm.Landscape:
                    width = 1536;
                    height = 1024;
                    break;
                default:
                    width = height = 1024;
                    break;
            }

            await this.OnGettingResponseAsync(input, cancellationToken.Value);

            string prompt = await this.GetInstructionsAsync(input, cancellationToken.Value);
            IImageClient client = clientFactory.CreateClient(this.Model);
            IEnumerable<BinaryData> result = await client.RunAsync(
                 prompt,
                 width,
                 height,
                 variantCount: this.VariantCount,
                 references: input.References,
                 cancellationToken: cancellationToken.Value);
            return await this.OnResponseGotAsync(input, result, cancellationToken.Value);
        }

        protected abstract Task<TOutput> OnResponseGotAsync(TInput input, IEnumerable<BinaryData> results, CancellationToken cancellationToken);
    }
}
