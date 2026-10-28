namespace SmartInventory.Common
{
    public class DatabaseStatusService
    {
        public bool IsConnectedToMySql { get; set; } = true;
        public string ProviderName { get; set; } = "MySQL (Pomelo EF Core)";
        public string ConnectionInfo { get; set; } = "localhost:3306 / smart_inventory_db";
        public string? DiagnosticsMessage { get; set; }
    }
}
