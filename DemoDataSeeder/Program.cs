using Core;
using Core.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoDataSeeder
{
    class Program
    {
        private const string Endpoint = "https://localhost:8081";
        private const string Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const int CollectionThroughput = 500;
        private static DocumentClient _documentClient;
        private static Database _database;
        private static List<StockIngredient> _stockIngredients = new List<StockIngredient>();
        private static List<Dish> _dishes = new List<Dish>();
        private static List<Order> _orders = new List<Order>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Creating tables and seeding data...");
            _documentClient = new DocumentClient(new Uri(Endpoint), Key);

            await EnsureDatabase();
            await EnsureAndSeedStock();
            await EnsureAndSeedDishes();
            await EnsureAndSeedOrders();

            Console.WriteLine("Tables and data created.");
        }

        private static async Task EnsureDatabase()
        {
            _database = await _documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = Constants.DatabaseName });
        }

        private static async Task EnsureAndSeedStock()
        {
            Console.WriteLine($"Ensuring {Constants.StockCollectionName} table and data...");
            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(_database.SelfLink, 
                new DocumentCollection { Id = Constants.StockCollectionName },
                new RequestOptions { OfferThroughput = CollectionThroughput });
            var collectionUri = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.StockCollectionName);

            var ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Salt",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Pepper",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Ground beef meat",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Beef steak slice",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Chili",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Potato",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Tomato",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Bread bun",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Ketchup",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mustard",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mayonnaise",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Egg",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Hotdog",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Cheddar",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Vegetable oil",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Bacon",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Beer",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);
            ingredient = new StockIngredient
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Soft drink",
                StockQuantity = 100
            };
            _stockIngredients.Add(ingredient);


            foreach(var toAdd in _stockIngredients)
            {
                await _documentClient.CreateDocumentAsync(collectionUri, toAdd, disableAutomaticIdGeneration: true);
            }

            Console.WriteLine($"{Constants.StockCollectionName} table and data seeded.");
        }

        private static async Task EnsureAndSeedDishes()
        {
            Console.WriteLine($"Ensuring {Constants.DishesCollectionName} table and data...");

            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(_database.SelfLink,
                new DocumentCollection { Id = Constants.DishesCollectionName },
                new RequestOptions { OfferThroughput = CollectionThroughput });
            var collectionUri = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.DishesCollectionName);

            var dish = new Dish
            {
                Title = "Hamburger",
                Description = "Juicy hamburger",
                ImageUrl = "",
                Price = 4.5m,
                Type = DishType.Main,
                Id = Guid.NewGuid().ToString(),
                Recipe = new Recipe
                {
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep
                        {
                            StepName = "Make hamburger meat",
                            StepOrder = 1,
                            SecondsRequired = 10
                        }, 
                        new RecipeStep
                        {
                            StepName = "Cook hamburger meat",
                            StepOrder = 2,
                            SecondsRequired = 60
                        },
                        new RecipeStep
                        {
                            StepName = "Prepare bun",
                            StepOrder = 3,
                            SecondsRequired = 20
                        },
                        new RecipeStep
                        {
                            StepName = "Combine everything",
                            StepOrder = 4,
                            SecondsRequired = 5
                        }
                    }
                },
                Ingredients = new List<DishIngredient>
                {
                    new DishIngredient
                    {
                        Name = "Ground beef meat",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Ground beef meat", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Salt",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Salt", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Pepper",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Pepper", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Bread bun",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Bread bun", StringComparison.OrdinalIgnoreCase)).Id
                    }
                }
            };
            _dishes.Add(dish);
            dish = new Dish
            {
                Title = "Hotdog",
                Description = "Juicy hotdog",
                ImageUrl = "",
                Price = 4.5m,
                Type = DishType.Main,
                Id = Guid.NewGuid().ToString(),
                Recipe = new Recipe
                {
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep
                        {
                            StepName = "Boil hotdog",
                            StepOrder = 1,
                            SecondsRequired = 120
                        },
                        new RecipeStep
                        {
                            StepName = "Prepare bun",
                            StepOrder = 2,
                            SecondsRequired = 20
                        },
                        new RecipeStep
                        {
                            StepName = "Combine everything",
                            StepOrder = 3,
                            SecondsRequired = 5
                        }
                    }
                },
                Ingredients = new List<DishIngredient>
                {
                    new DishIngredient
                    {
                        Name = "Hotdog",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Hotdog", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Mustard",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Mustard", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Ketchup",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Ketchup", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Bread bun",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Bread bun", StringComparison.OrdinalIgnoreCase)).Id
                    }
                }
            };
            _dishes.Add(dish);
            dish = new Dish
            {
                Title = "French fries",
                Description = "Crispy french fries",
                ImageUrl = "",
                Price = 4.5m,
                Type = DishType.Side,
                Id = Guid.NewGuid().ToString(),
                Recipe = new Recipe
                {
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep
                        {
                            StepName = "Fry potatoes",
                            StepOrder = 1,
                            SecondsRequired = 120
                        }
                    }
                },
                Ingredients = new List<DishIngredient>
                {
                    new DishIngredient
                    {
                        Name = "Potato",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Potato", StringComparison.OrdinalIgnoreCase)).Id
                    },
                    new DishIngredient
                    {
                        Name = "Salt",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Salt", StringComparison.OrdinalIgnoreCase)).Id
                    }
                }
            };
            _dishes.Add(dish);
            dish = new Dish
            {
                Title = "Cheese platter",
                Description = "Who doesn't love cheese",
                ImageUrl = "",
                Price = 4.5m,
                Type = DishType.Starter,
                Id = Guid.NewGuid().ToString(),
                Recipe = new Recipe
                {
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep
                        {
                            StepName = "Put cheese on plate",
                            StepOrder = 1,
                            SecondsRequired = 10
                        }
                    }
                },
                Ingredients = new List<DishIngredient>
                {
                    new DishIngredient
                    {
                        Name = "Cheddar",
                        QuantityNeeded = 1,
                        StockIngredientId = _stockIngredients
                            .FirstOrDefault(x => x.Name.Equals("Cheddar", StringComparison.OrdinalIgnoreCase)).Id
                    }
                }
            };
            _dishes.Add(dish);

            foreach (var toAdd in _dishes)
            {
                await _documentClient.CreateDocumentAsync(collectionUri, toAdd, disableAutomaticIdGeneration: true);
            }

            Console.WriteLine($"{Constants.DishesCollectionName} table and data seeded.");
        }

        private static async Task EnsureAndSeedOrders()
        {
            Console.WriteLine($"Ensuring {Constants.OrdersCollectionName} table and data...");

            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(_database.SelfLink,
                new DocumentCollection { Id = Constants.OrdersCollectionName },
                new RequestOptions { OfferThroughput = CollectionThroughput });
            var collectionUri = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.OrdersCollectionName);

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TimePlaced = DateTime.UtcNow,
                Status = OrderStatus.New,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        DishId = _dishes.FirstOrDefault(x => x.Title.Equals("French fries", StringComparison.OrdinalIgnoreCase)).Id,
                        Quantity = 1
                    },
                    new OrderItem
                    {
                        DishId = _dishes.FirstOrDefault(x => x.Title.Equals("Hamburger", StringComparison.OrdinalIgnoreCase)).Id,
                        Quantity = 1
                    }
                }
            };
            _orders.Add(order);
            order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TimePlaced = DateTime.UtcNow,
                Status = OrderStatus.New,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        DishId = _dishes.FirstOrDefault(x => x.Title.Equals("French fries", StringComparison.OrdinalIgnoreCase)).Id,
                        Quantity = 1
                    },
                    new OrderItem
                    {
                        DishId = _dishes.FirstOrDefault(x => x.Title.Equals("Cheese platter", StringComparison.OrdinalIgnoreCase)).Id,
                        Quantity = 1
                    }
                }
            };
            _orders.Add(order);
            order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                TimePlaced = DateTime.UtcNow,
                Status = OrderStatus.New,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        DishId = _dishes.FirstOrDefault(x => x.Title.Equals("Hotdog", StringComparison.OrdinalIgnoreCase)).Id,
                        Quantity = 6
                    }
                }
            };
            _orders.Add(order);

            foreach (var toAdd in _orders)
            {
                await _documentClient.CreateDocumentAsync(collectionUri, toAdd, disableAutomaticIdGeneration: true);
            }

            Console.WriteLine($"{Constants.OrdersCollectionName} table and data seeded.");
        }
    }
}