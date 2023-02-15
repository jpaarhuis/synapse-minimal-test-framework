# synapse-minimal-test-framework

Minimal test framework to test Azure Synapse pipelines with MSTest (CI included).
The framework is build with Synapse in mind, but with a little modification it would also be able to support ADF.

## How to use this project?

To get started quick, copy the entire folder structure and use the example in `ExampleTests.cs` to get started.

## How to run test?

Run `dotnet test` or use  Visual Studio's Test Explorer.

## How does authentication work?

The Azure.Identity library is being used for authentication using Managed Identities. This will use the current users identity to connect. Use Native Visual studio integration or otherwise the `az login` command to authenticate, [more info here](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet).

## CI - GitHub



## CI - Azure DevOps

