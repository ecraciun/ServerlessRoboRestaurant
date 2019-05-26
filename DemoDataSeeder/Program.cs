using Core;
using Core.Entities;
using Core.Services;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DemoDataSeeder
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        private const string Endpoint = "https://localhost:8081";
        private const string Key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const int CollectionThroughput = 500;
        private static DocumentClient _documentClient;
        private static Database _database;
        private static List<StockIngredient> _stockIngredients = new List<StockIngredient>();
        private static List<Dish> _dishes = new List<Dish>();
        private static List<Supplier> _suppliers = new List<Supplier>();

        static async Task Main(string[] args)
        {
            //await SetupAndSeed();

            //await PlaceMultipleOrders(10);
        }

        private static async Task PlaceMultipleOrders(int numberOfOrders)
        {
            Console.WriteLine($"Creating {numberOfOrders} orders...");
            var ordersRepoFactory = new CosmosDbBaseRepositoryFactory<Order>();
            var ordersRepo = ordersRepoFactory.GetInstance(Endpoint, Key, Constants.OrdersCollectionName);
            var dishesRepoFactory = new CosmosDbBaseRepositoryFactory<Dish>();
            var dishesRepo = dishesRepoFactory.GetInstance(Endpoint, Key, Constants.DishesCollectionName);

            var allDishes = await dishesRepo.GetAllAsync();
            var random = new Random();

            for(int i = 0; i < numberOfOrders; i++)
            {
                var now = DateTime.UtcNow;
                var order = new Order
                {
                    LastModifiedUtc = now,
                    TimePlacedUtc = now,
                    Status = OrderStatus.New,
                    OrderItems = new List<OrderItem>()
                };
                var differentItemsToOrder = random.Next(1, 6);
                for(var j = 0; j < differentItemsToOrder; j++)
                {
                    var quantity = random.Next(1, 5);
                    var dishIndex = random.Next(1, allDishes.Count);

                    order.OrderItems.Add(new OrderItem
                    {
                        DishId = allDishes[dishIndex - 1].Id,
                        Quantity = quantity
                    });
                }

                await ordersRepo.AddAsync(order);
            }
            Console.WriteLine("Orders created");
        }

        private static async Task SetupAndSeed()
        {
            Console.WriteLine("Creating collections and seeding data...");
            _documentClient = new DocumentClient(new Uri(Endpoint), Key);

            await EnsureDatabase();

            await RemoveCollections();

            await EnsureAndSeedStock();
            await EnsureAndSeedDishes();
            await EnsureAndSeedOrders();
            await EnsureAndSeedSuppliers();
            await EnsureAndSeedSupplierOrders();

            Console.WriteLine("Collections and data created.");
        }

        private static async Task RemoveCollections()
        {
            Console.WriteLine("Removing all collections...");
            await _documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.StockCollectionName));
            await _documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.DishesCollectionName));
            await _documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.OrdersCollectionName));
            await _documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.SuppliersCollectionName));
            await _documentClient.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.SupplierOrdersCollectionName));
            Console.WriteLine("Collections removed.");
        }

        private static async Task EnsureDatabase()
        {
            _database = await _documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = Constants.DatabaseName });
        }

        private static async Task EnsureAndSeedStock()
        {
            Console.WriteLine($"Ensuring {Constants.StockCollectionName} collection and data...");
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


            foreach (var toAdd in _stockIngredients)
            {
                await _documentClient.CreateDocumentAsync(collectionUri, toAdd, disableAutomaticIdGeneration: true);
            }

            Console.WriteLine($"{Constants.StockCollectionName} collection and data seeded.");
        }

        private static async Task EnsureAndSeedDishes()
        {
            Console.WriteLine($"Ensuring {Constants.DishesCollectionName} collection and data...");

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

            Console.WriteLine($"{Constants.DishesCollectionName} collection and data seeded.");
        }

        private static async Task EnsureAndSeedOrders()
        {
            Console.WriteLine($"Ensuring {Constants.OrdersCollectionName} collection and data...");

            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(_database.SelfLink,
                new DocumentCollection { Id = Constants.OrdersCollectionName },
                new RequestOptions { OfferThroughput = CollectionThroughput });

            Console.WriteLine($"{Constants.OrdersCollectionName} collection and data seeded.");
        }

        private static async Task EnsureAndSeedSuppliers()
        {
            Console.WriteLine($"Ensuring {Constants.SuppliersCollectionName} collection and data...");

            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(_database.SelfLink,
                new DocumentCollection { Id = Constants.SuppliersCollectionName },
                new RequestOptions { OfferThroughput = CollectionThroughput });
            var collectionUri = UriFactory.CreateDocumentCollectionUri(Constants.DatabaseName, Constants.SuppliersCollectionName);

            var supplier = new Supplier
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Supplier 1",
                TimeToDelivery = 20,
                IngredientsForSale = new List<SupplierIngredient>
                {
                    new SupplierIngredient
                    {
                        Name = "Salt",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Pepper",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Ground beef meat",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Beef steak slice",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Chili",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Potato",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Tomato",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Bread bun",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Ketchup",
                        UnitPrice = 2
                    },
                    new SupplierIngredient
                    {
                        Name = "Mustard",
                        UnitPrice = 2
                    }
                }
            };
            _suppliers.Add(supplier);

            supplier = new Supplier
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Supplier 2",
                TimeToDelivery = 15,
                IngredientsForSale = new List<SupplierIngredient>
                {
                    new SupplierIngredient
                    {
                        Name = "Ketchup",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Mustard",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Mayonnaise",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Egg",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Hotdog",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Cheddar",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Vegetable oil",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Bacon",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Beer",
                        UnitPrice = 3
                    },
                    new SupplierIngredient
                    {
                        Name = "Soft drink",
                        UnitPrice = 3
                    }
                }
            };
            _suppliers.Add(supplier);

            foreach (var toAdd in _suppliers)
            {
                await _documentClient.CreateDocumentAsync(collectionUri, toAdd, disableAutomaticIdGeneration: true);
            }

            Console.WriteLine($"{Constants.SuppliersCollectionName} collection and data seeded.");
        }

        private static async Task EnsureAndSeedSupplierOrders()
        {
            Console.WriteLine($"Ensuring {Constants.SupplierOrdersCollectionName} collection and data...");

            await _documentClient.CreateDocumentCollectionIfNotExistsAsync(_database.SelfLink,
                new DocumentCollection { Id = Constants.SupplierOrdersCollectionName },
                new RequestOptions { OfferThroughput = CollectionThroughput });

            Console.WriteLine($"{Constants.SupplierOrdersCollectionName} collection and data seeded.");
        }
    }
}