using System.Diagnostics.CodeAnalysis;

namespace Core
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        // Cosmos DB settings keys
        public const string CosmosDbEndpointKeyName                         = "CosmosDbEndpoint";
        public const string CosmosDbKeyKeyName                              = "CosmosDbKey";
        public const string CosmosDbConnectionStringKeyName                 = "CosmosDbConnection";


        // Function names
        public const string OrderOrchestratorFunctionName                   = "OrderOrchestrator";
        public const string GetMenuFunctionName                             = "GetMenu";
        public const string PlaceOrderFunctionName                          = "PlaceOrder";
        public const string GetOrderStatusFunctionName                      = "GetOrderStatus";
        public const string GetSupplierOrdersFunctionName                   = "GetSupplierOrders";
        public const string GetStockIngredientsFunctionName                 = "GetStockIngredients";
        public const string GetOrdersFunctionName                           = "GetOrders";
        public const string CheckAndReserveStockActivityFunctionName        = "CheckAndReserveStock";
        public const string CreateSupplierOrderActivityFunctionName         = "CreateSupplierOrder";
        public const string DishOrchestratorFunctionName                    = "DishOrchestrator";
        public const string RecipeStepActivityFunctionName                  = "RecipeStep";
        public const string GetDishActivityFunctionName                     = "GetDish";
        public const string UpdateOrderActivityFunctionName                 = "UpdateOrder";
        public const string OrderListenerFunctionName                       = "OrderListener";
        public const string StockCheckerOrchestratorFunctionName            = "StockCheckerOrchestrator";
        public const string SupplierFinderActivityFunctionName              = "SupplierFinder";
        public const string UpdateStockActivityFunctionName                 = "UpdateStock";
        public const string InventoryCheckerEternalOrchestratorFunctionName = "InventoryCheckerEternalOrchestrator";
        public const string GetStockActivityFunctionName                    = "GetStock";
        public const string SupplierOrderReceiverOrchestratorFunctionName   = "SupplierOrderReceiverOrchestrator";
        public const string SupplierOrderMonitorOrchestratorFunctionName    = "SupplierOrderMonitorOrchestrator";

        //public const string FunctionName = "";

        // Application constants
        public const int DefaultTryUpdateRetryCount                         = 10;
        public const int DefaultUrgentIngredientOrderQuantity               = 50;
        public const int DefaultRegularIngredientOrderQuantity              = 100;
        public const int RegularInventoryCheckMinimumThreshold              = 30;
        public const int RegularInventoryCheckSleepTimeInSeconds            = 60;
        public const int DefaultSupplierOrderCheckSleepInSeconds            = 60;
        public const int DefaultOrchestratorTimeoutInSeconds                = 180;
        public const string InventoryCheckerOrchestratorId                  = "InventoryChecker";
        public const string SupplierOrderReceiverOrchestratorId             = "SupplierOrderReceiver";
        public const string DefaultPartitionName                            = "default";
        public const string StockCollectionName                             = "RestaurantStock";
        public const string OrdersCollectionName                            = "Orders";
        public const string DishesCollectionName                            = "Menu";
        public const string DatabaseName                                    = "RoboRestaurant";
        public const string SupplierOrdersCollectionName                    = "SupplierOrders";
        public const string SuppliersCollectionName                         = "Suppliers";
    }
}