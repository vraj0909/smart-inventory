namespace SmartInventory.Models
{
    public enum TransactionType
    {
        StockIn = 1,
        StockOut = 2,
        Adjustment = 3,
        Return = 4
    }

    public enum StockStatusFilter
    {
        All = 0,
        InStock = 1,
        LowStock = 2,
        OutOfStock = 3
    }
}
