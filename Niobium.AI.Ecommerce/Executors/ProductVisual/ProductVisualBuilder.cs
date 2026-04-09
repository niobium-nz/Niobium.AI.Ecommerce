using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts.ProductVisual;

namespace Niobium.AI.Ecommerce.Executors.ProductVisual
{
    internal class ProductVisualBuilder(IImageClientFactory clientFactory, ILogger<ProductVisualBuilder> logger)
        : GenericImageProducer<ProductVisualBuilderInput, ProductCreativityOutput>(clientFactory)
    {
        public override string Id => nameof(ProductVisualBuilder);

        protected override async Task<ProductCreativityOutput> OnResponseGotAsync(string conversationID, ProductVisualBuilderInput input, IEnumerable<BinaryData> results, CancellationToken cancellationToken)
        {
            ProductCreativityOutput result = new();
            if (results.Count() != 1)
            {
                logger.LogWarning("Expected exactly one product visual, but got {Count}.", results.Count());
                return result;
            }

            string tempFilename = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFilename, results.First().ToArray(), cancellationToken);
            result.ProductVisual = tempFilename;
            return result;
        }
    }
}
