# Task 1: Code Analysis & Refactoring Identification

## Objective
Analyze the existing codebase, run and test the application, and identify areas that you believe should be improved, refactored, or fixed.

## Instructions
1. **Explore the Code:** Review the current structure, following the flow from the API layer down to the Domain and Infrastructure layers.
2. **Run the Application:** Start the application, interact with the Swagger UI, check the Hangfire dashboard, and observe the application's behavior.
3. **Identify Improvements:** Look for issues related to clean architecture principles, design patterns, error handling, performance, or overall code quality.
4. **Document Findings:** You **do not** need to implement these changes right now. Simply document your findings directly in this file.

---

## Your Findings

### Clean code and clean architecture violations

[ProductsController.cs](src/Demo.Api/Controllers/ProductsController.cs) uses `namespace Ecommerce.Api.Controllers`, but everything else in the project lives under `Demo.*`. Looks like a leftover from a copy-paste or a rename that never got finished. Should be `Demo.Api.Controllers`.

`IInventoryModule` and `InventoryModule` are both defined in [InventoryModule.cs](src/Demo.Application/Modules/InventoryModule.cs). For a project this small it works, but since we are following clean architecture, contracts and implementations should not share a file. The interface should live on its own, under a folder that makes the separation obvious.

The [Demo.Infrastructure.csproj](src/Demo.Infrastructure/Demo.Infrastructure.csproj) has a `ProjectReference` to `Demo.Application`. That is backwards. Infrastructure should only know about Domain. Application depends on Domain, and the API wires everything at the composition root. With this reference in place, Infrastructure can see Application types, which defeats the layering. It should point to `Demo.Domain` instead.

The SQLite connection string in [Program.cs](src/Demo.Api/Program.cs) is hardcoded as `"Data Source=demo.db"`. It should come from `appsettings.json` or an environment variable so we can change it per environment without touching the code.

The controller in [ProductsController.cs](src/Demo.Api/Controllers/ProductsController.cs) returns the `Product` domain entity straight to the client. If someone adds a field to `Product` for internal tracking, it leaks into the API response. We should map to a DTO before returning.

`FakeMessagePublisher` is registered in DI but nothing in the codebase ever injects or calls `IMessagePublisher`. It is dead code right now. Either remove it or wire up the use case it was meant for.

### Error handling

`ProductRepository.GetProduct` uses `FirstAsync`, which throws `InvalidOperationException` when no product matches the SKU. The controller has no try/catch around it, so the client gets a raw 500 with a stack trace in Development mode. The repository should use `FirstOrDefaultAsync` and the Application layer should return a proper 404 when the result is null.

There is no global exception handling at all: no middleware, no `UseExceptionHandler`, no `ProblemDetails` setup. Any unhandled throw surfaces as raw error details or a blank response. We need at least a catch-all middleware that logs the exception and returns a consistent error payload.

### Performance

None of the methods across the stack (controller, module, repository) accept or forward a `CancellationToken`. If a client drops the connection mid-request, EF Core keeps running the query anyway. The token should flow from the controller action all the way down to the EF call.

### Domain modeling and DI configuration

The `Product` entity in [Product.cs](src/Demo.Domain/Modules/Inventory/Product.cs) uses `Sku` as its primary key (configured in `DemoDbContext`). SKU is a business identifier. It can change if we rebrand a product or restructure the catalog, and when it does, every foreign key pointing to it needs a cascading update. A surrogate `Id` (int or Guid) as the PK with SKU as a unique index avoids that problem.

In Program.cs, `IProductRepository`, `IMessagePublisher`, and `IInventoryModule` are all registered with `AddTransient`. Since `DemoDbContext` is scoped, the repositories should be scoped too. With Transient, two components in the same request can end up with separate repository instances backed by different context scopes, which is a subtle bug waiting to happen. These should be `AddScoped`.

---

## Dismissed findings (acceptable in an assessment context)

I noticed a few more things, but they are reasonable shortcuts for a starter project that is not going to production.

- The database seed block in Program.cs inserts products directly during startup. In a real app this should be its own class, but here it keeps things self-contained for whoever is evaluating.

- [UnitTest1.cs](src/Demo.UnitTests/UnitTest1.cs) has an empty test body. It is clearly a placeholder for the candidate, not something someone forgot to write.

- [HangfireSetup.cs](src/Demo.Infrastructure/HangfireSetup.cs) uses `UseMemoryStorage()`, so job state is lost on restart. The README says this on purpose; it avoids making evaluators set up extra infrastructure.

- Swagger is only enabled in Development. That is the default `dotnet new webapi` template behavior and fine for a project that only runs locally.
