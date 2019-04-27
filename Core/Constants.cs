using System.Diagnostics.CodeAnalysis;

namespace Core
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public const string DefaultPartitionName            = "default";
        public const string StockCollectionName             = "RestaurantStock";
        public const string OrdersCollectionName            = "Orders";
        public const string DishesCollectionName            = "Menu";
        public const string DatabaseName                    = "RoboRestaurant";
        public const string SupplierOrdersCollectionName    = "SupplierOrders";
        public const string SuppliersCollectionName         = "Suppliers";

        public const string CosmosDbEndpointKeyName         = "CosmosDbEndpoint";
        public const string CosmosDbKeyKeyName              = "CosmosDbKey";
    }
}