using Microsoft.WindowsAzure.Storage.Table;

namespace Core.Entities
{
    public class StockIngredient : TableEntity
    {
        public StockIngredient()
        {
            this.PartitionKey = Constants.DefaultPartitionName;
        }

        public string Name { get; set; }
        public int StockQuantity { get; set; }
    }
}