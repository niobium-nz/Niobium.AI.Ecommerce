namespace Niobium.AI.Ecommerce.Contracts
{
    internal record ProductCost(
            double COGSPerUnit,
            double ExtraUnitCOGSPerOrder,
            string SalesTax,
            string PaymentProcessingFees
    );
}
