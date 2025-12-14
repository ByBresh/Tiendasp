namespace Tiendasp.API.Products.Dto.Product
{
    public class AdjustStockRequest
    {
        /// <summary>
        /// Positive to increase stock, negative to decrease stock.
        /// </summary>
        public required int Quantity { get; set; }
    }
}
