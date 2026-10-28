using System.Collections.Generic;

namespace SmartInventory.ViewModels
{
    public class RestockItemSessionModel
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int SuggestedRestockQty { get; set; }
        public decimal UnitCostPrice { get; set; }
        public string Unit { get; set; } = "pcs";
        public decimal TotalItemCost => SuggestedRestockQty * UnitCostPrice;
    }

    public class RestockOrderViewModel
    {
        public List<RestockItemSessionModel> Items { get; set; } = new List<RestockItemSessionModel>();
        public decimal GrandTotalCost { get; set; }
        public int TotalItemsCount { get; set; }
        public string? OrderNotes { get; set; }
        public string OrderReferenceNumber { get; set; } = string.Empty;
    }
}
