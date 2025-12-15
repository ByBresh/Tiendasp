using System.ComponentModel.DataAnnotations;

namespace Tiendasp.API.Products.Dto.Product
{
    public class AdjustStockRequest
    {
        /// <summary>
        /// Positive to increase stock, negative to decrease stock.
        /// </summary>
        [DeniedValues(0, ErrorMessage = "Quantity cannot be zero")]
        public int Quantity { get; set; }
    }
}
