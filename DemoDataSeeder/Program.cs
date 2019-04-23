using Core;
using Core.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoDataSeeder
{
    class Program
    {
        private static CloudStorageAccount _storageAccount;
        private static CloudTableClient _tableClient;
        private static List<StockIngredient> _stockIngredients = new List<StockIngredient>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Creating tables and seeding data...");
            _storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            _tableClient = _storageAccount.CreateCloudTableClient();

            //await EnsureAndSeedStock();
            await EnsureAndSeedDishes();
            await EnsureAndSeedOrders();

            Console.WriteLine("Tables and data created.");
        }

        static async Task EnsureAndSeedStock()
        {
            Console.WriteLine($"Ensuring {Constants.StockTableName} table and data...");

            var table = _tableClient.GetTableReference(Constants.StockTableName);
            await table.CreateIfNotExistsAsync();

            var ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Salt",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Pepper",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Minced beef meat",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Beef steak slice",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Chili",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Potato",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Tomato",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Bread",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Ketchup",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Mustard",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Mayonnaise",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Egg",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Hot dog",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Cheddar",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Vegetable oil",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                RowKey = Guid.NewGuid().ToString(),
                Name = "Bacon",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);

            var batchOperation = new TableBatchOperation();

            foreach(var toAdd in _stockIngredients)
            {
                batchOperation.Insert(toAdd);
            }
            await table.ExecuteBatchAsync(batchOperation);

            Console.WriteLine($"{Constants.StockTableName} table and data seeded.");
        }

        static async Task EnsureAndSeedDishes()
        {
            Console.WriteLine($"Ensuring {Constants.DishesTableName} table and data...");



            Console.WriteLine($"{Constants.DishesTableName} table and data seeded.");
        }

        static async Task EnsureAndSeedOrders()
        {
            Console.WriteLine($"Ensuring {Constants.OrdersTableName} table and data...");



            Console.WriteLine($"{Constants.OrdersTableName} table and data seeded.");
        }
    }
}
