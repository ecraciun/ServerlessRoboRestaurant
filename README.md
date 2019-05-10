# Serverless Robo Restaurant [![Build status](https://emilcraciun.visualstudio.com/FunctionsPlayground/_apis/build/status/ServerlessRoboRestaurant%20BUILD)](https://emilcraciun.visualstudio.com/FunctionsPlayground/_build/latest?definitionId=6) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
ITCamp 2019 presentation solution

## Motivation

### Scope

Sample solution to highlight [Azure Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview) concepts, advantages and disatvantages.

This solution models a simplified, theoretical version of a restaurant with focus on the main workfllows that support the "business". Through this scenario, the following concepts were showcased:
- client, orchestrator and activity funcitons
- function chaining
- fan out/fan in
- monitor
- sub-orchestration
- durable timer
- sending and listening for external events
- eternal orchestrations
- singleton orchestrations
- error handling
- logging
- unit testing

### Description

![High level architecture](/content/hla.png)

This is high level architecture of the solution. It comprised of the following main modules:
- ClientAPI - several HTTP endpoints with which the clients interact with the restaurant
- BackofficeAPI - several HTTP endpoints for accessing internal restaurant data
- Restaurant - implements the main restaurant workflows and logic, being the main focus of this sample solution

Key features and workflows of the solution:
- a restaurant must have a stock of ingredients
- a restaurant must have a menu with dishes, which are made from a recipe with several chained steps, and with several ingredients
- a restaurant must be able to accept orders
- a restaurant must regulary check its inventory and order low quantity ingredients to replenish stock
- a restaurant must prepare an order, by checking all required ingredients available quantities, ordering them fast if they are not sufficient, cooking the recipes, step by step and delivering the items

## Run locally 

### Prerequisites
- [.Net Core Sdk 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2)
- [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) and make sure you check the following workloads when installing:
  - .NET Core cross-platform development
  - Azure development (this will install, among others, the Microsoft Azure Storage Emulator, which you will need)
- [CosmosDB emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)

### Debugging / running

- clone the code
- start the CosmosDB emulator
- make sure that all 3 function projects (ClientAPI, BackofficeAPI and Restaurant) have a valid *local.settings.json* file (sample below)
- if you want some starting sample data, run the *DemoDataSeeder* console app (but make sure you provide valid values for the *Endpoint* and *Key* constants at the beginning of the *Program.cs* file)
- select the functions project you want to run and make it the startup project
- hit F5 and have fun!

For testing orchestrators separately, I have created a HTTP starter function for them in Debug builds.

Sample local.setting.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "FUNCTIONS_EXTENSION_VERSION": "~2",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "CosmosDbEndpoint": "https://localhost:8081",
    "CosmosDbKey": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", // don't worry, this is just the emulator key :)
    "CosmosDbConnection": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", // will probably construct it from the above 2 in future revisions
    "APPINSIGHTS_INSTRUMENTATIONKEY": "[your app insights instrumentation key]"
  }
}
```


## Run on Azure

[Comming soon]