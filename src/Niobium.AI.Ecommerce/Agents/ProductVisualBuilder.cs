using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class ProductVisualBuilder(IImageClientFactory clientFactory, ILogger<ProductVisualBuilder> logger)
        : GenericImageProducer<ProductVisualBuilderInput, ProductCreativityOutput>(clientFactory)
    {
        public override string Id => nameof(ProductVisualBuilder);

        protected override async Task<ProductCreativityOutput> OnResponseGotAsync(ProductVisualBuilderInput input, IEnumerable<BinaryData> results, CancellationToken cancellationToken)
        {
            ProductCreativityOutput result = new();
            if (results.Count() != 1)
            {
                logger.LogWarning("Expected exactly one product visual, but got {Count}.", results.Count());
                return result;
            }

            string tempFilePath = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFilePath, results.First().ToArray(), cancellationToken);
            result.ProductVisual = new Uri(tempFilePath, UriKind.Absolute);
            result.MediaType = results.First().MediaType;
            return result;
        }
    }
}
