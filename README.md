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

The tests can run in CI using GitHub Actions. You can find the GitHub Workflow YAML here: [.github/workflows/ci.yml](.github/workflows/ci.yml)

The authentication used here to make GitHub Actions connect to Azure is using federated credentials:
[Configure App Service with Federated Credentials](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Cwindows#add-federated-credentials-preview). You can also use a service principal secret: [Configure App Service with Service Principal Secret](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Cwindows#use-the-azure-login-action-with-a-service-principal-secret)

The GitHub Action will publish a test report: ![GitHub Test Report](docs/github-test-report.png)

## CI - Azure DevOps

The tests can also run in CI with Azure DevOps. You can find the Azure DevOps YAML here: [AzDevOps/ci.yml](AzDevOps/ci.yml)

Make sure you replace `[ServiceConnectionHere]` with your service connection name. The tests will run on the Service Connection Service Principal's identity.

![Azure DevOps Test Report](docs/azure-devops-test-report.png)