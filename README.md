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

Authentication used to make GitHub Actions connect to Azure is with federated credentials:
[Configure App Service with Federated Credentials](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Cwindows#add-federated-credentials-preview). You can also use a service principal secret: [Configure App Service with Service Principal Secret](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Cwindows#use-the-azure-login-action-with-a-service-principal-secret)

## CI - Azure DevOps

