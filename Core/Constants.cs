using System.Diagnostics.CodeAnalysis;

namespace Core
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public const string DefaultPartitionName                        = "default";
        public const string StockCollectionName                         = "RestaurantStock";
        public const string OrdersCollectionName                        = "Orders";
        public const string DishesCollectionName                        = "Menu";
        public const string DatabaseName                                = "RoboRestaurant";
        public const string SupplierOrdersCollectionName                = "SupplierOrders";
        public const string SuppliersCollectionName                     = "Suppliers";

        public const string CosmosDbEndpointKeyName                     = "CosmosDbEndpoint";
        public const string CosmosDbKeyKeyName                          = "CosmosDbKey";
        public const string CosmosDbConnectionStringKeyName             = "CosmosDbConnection";

        public const string OrderOrchestratorFunctionName               = "OrderOrchestrator";
        public const string GetMenuFunctionName                         = "GetMenu";
        public const string PlaceOrderFunctionName                      = "PlaceOrder";
        public const string GetOrderStatusFunctionName                  = "GetOrderStatus";
        public const string GetSupplierOrdersFunctionName               = "GetSupplierOrders";
        public const string GetStockIngredientsFunctionName             = "GetStockIngredients";
        public const string GetOrdersFunctionName                       = "GetOrders";
        public const string CheckStockActivityFunctionName              = "CheckStock";
        public const string CreateSupplierOrderActivityFunctionName     = "CreateSupplierOrder";
        public const string DishOrchestratorFunctionName                = "DishOrchestrator";
        public const string DishStepActivityFunctionName                = "DishStep";
        public const string HandleOrderActivityFunctionName             = "HandleOrder";
        public const string OrderListenerFunctionName                   = "OrderListener";
        public const string StockCheckerOrchestratorFunctionName        = "StockCheckerOrchestrator";
        public const string SupplierFinderActivityFunctionName          = "SupplierFinder";
        //public const string FunctionName = "";
    }
}