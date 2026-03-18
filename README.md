# Clean Model Context Protocol

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/stphnwlsh/cleanmodelcontextprotocol/build-pipeline.yml?label=Build%20Pipeline%20&logo=github&style=for-the-badge)](https://github.com/stphnwlsh/CleanModelContextProtocol/actions/workflows/build-pipeline.yml)
[![Codecov](https://img.shields.io/codecov/c/github/stphnwlsh/CleanModelContextProtocol?label=Code%20Coverage&logo=codecov&logoColor=white&style=for-the-badge)](https://codecov.io/gh/stphnwlsh/CleanModelContextProtocol)
[![Nuget](https://img.shields.io/nuget/v/CleanModelContextProtocol.Template?label=nuget%20template&logo=nuget&logoColor=white&style=for-the-badge)](https://www.nuget.org/packages/CleanModelContextProtocol.Template/)
[![GitHub Sponsors](https://img.shields.io/static/v1?label=GitHub%20Sponsors&message=$1&logo=githubsponsors&logoColor=white&color=ea4aaa&style=for-the-badge)](https://github.com/sponsors/stphnwlsh/sponsorships?sponsor=stphnwlsh&tier_id=333950)
[![Buy Me A Coffee](https://img.shields.io/static/v1?label=Buy%20Me%20A%20Coffee&message=$1&logo=buymeacoffee&logoColor=white&color=ffdd00&style=for-the-badge)](https://www.buymeacoffee.com/stphnwlsh)

This is a template API using a streamlined version of [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) alongside .NET's [Model Context Protocol](https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/).

The MCP NuGet packagages [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) and [ModelContextProtocol.AspNetCore](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore) are in preview and are bound to change.  The implmentation will remain fairly basic here until they are officially released.

## Prerequisites

This solution in built on the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), you need to install that before it will work for you.  If you want to build the Dockerfile you will need to install [Docker](https://www.docker.com/products/docker-desktop) as well.

## Installation

This is a template and you can install it using the [dotnet new cli](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new).  To install the lastest version of the template run the following command.

``` bash
dotnet new install CleanModelContextProtocol.Template
```

To create a new solution using this template run the following command

```bash
dotnet new cleanmodelcontextprotocol --name {YOUR_SOLUTION_NAMESPACE} --au "{YOU_AUTHORS_NAME}"
```

## Docker

There's a dockerfile included in the build folder and serves the purpose of restoring, building, testing, publishing and then creating a runtime image of the API.  Works on my machine.....you can add a version prefix and suffix to version the service in the assembly.  The Dockerfile does have stages so you can just run the tests or publish the solution depending on your needs.  Review the `build-pipeline.yml` in the github folder for more detailed usage.

``` bash
docker build . -t cleanmodelcontextprotocol:latest --build-arg VERSION_PREFIX {VERSION_NUMBER} -- build-arg VERSION_SUFFIX {PRERELEASE_NAME}
```

The Github Action does publish an image of this API and you check it out for yourself by runnning this command in docker.

``` bash
docker pull stphnwlsh/cleanmodelcontextprotocol
```

## Architecture

This solution is loosely based on Clean Architecture patterns, it's by no means perfect.  I prefer to call it "Lean Mean Clean Architecture".  Inspiration has been taken from [Jason Taylor's Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture), but I have made some structural decisions to take some things further and scaled back others.

There's a little CQRS type stuff going on here but it's more in style than real separated functions for reading and writing as under the covers they are the same data source.

Breaking the Clean Architecture pattern is the fact that the Infrastructure project is referenced by the Presentation project.  This is for **Dependency Injection** purposes, so to protect this a little further, all classes in the Infrastructure project are `internal`.  This stops them being accidentally used in the Presentation project.

### Project Structure

It's streamlined into 3 functional projects.  All serve their own purpose and segregate aspects of the application to allow easier replacement and updating.

1. **Presentation** - Setting up the interactions between the Application layer and the consumer.  In the project that's via Model Context Protocol but it could be many other things.  The Model Context Protocol uses tools to funnel the actions to the layer that owns the domain.
1. **Application** - This project owns the domain and business logic.  There's validation of the Commands and Queries and handling of domain entities in their own separated structures.  Each domain type has it's own interface to a datasource downstream, this project doesn't care what fulfills this contract, as long as someone does.
1. **Infrastructure** - Here's where the database comes into play.  Infra owns the data objects and works with the repository interfaces to fetch, create, update and remove object from the source.  There's some entity mapping here to allow specific models with attributes to remain in this layer and not bleed through to the **Application** layer.

## Features

There are plenty of handy implementations of features throughout this solution, in no particular order here are some that might interest you.

- Logging using [Serilog](https://github.com/serilog/serilog)
- Mediator Pattern using [Mediatr](https://github.com/jbogard/MediatR)
- Validation using [FluentValidation](https://github.com/FluentValidation/FluentValidation)
- Testing using [Shouldly](https://github.com/shouldly/shouldly) and [NSubstitute](https://github.com/nsubstitute/NSubstitute)
- OpenApi using [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- Object Mapping using [Mapperly](https://mapperly.riok.app)

## Resources

This sample would not have been possible without gaining inspiration from the following resources.  If you are on your own learning adventure please read the following blogs and documentation.

- [Sam Basu - Build & Leverage MCP Servers in C# for AI-Driven Development](https://platform.uno/blog/build-leverage-mcp-servers-in-c-for-ai-driven-development)
- [James Montemagno - Build a Model Context Protocol (MCP) server in C#](https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp)
- [Laurent Kempé - SSE-Powered MCP Server with C# and .NET in 15.7MB executable](https://laurentkempe.com/2025/04/05/sse-powered-mcp-server-with-csharp-and-dotnet-in-157mb-executable)
- [Will Velida - Building a Sports-Themed MCP Server Using .NET](https://dev.to/willvelida/building-a-sports-themed-mcp-server-using-net-22ln)
- [Jose Maria Flores Zazo - From API to MCP: How to Expose Your Endpoints Securely to AI](https://medium.com/@jmfloreszazo/from-api-to-mcp-how-to-expose-your-endpoints-securely-to-ai-4aecc84cee28)
- [MCP C# SDK](https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.html)
- [What is the Model Context Protocol (MCP)?](https://modelcontextprotocol.io/docs/getting-started/intro)

## Connect and Support

If you like this, or want to checkout my other work, please connect with me on [LinkedIn](https://www.linkedin.com/in/stphnwlsh), and/or follow me on [Medium](https://stphnwlsh.medium.com) or [GitHub](https://github.com/stphnwlsh).

If you want to see more updates or more projects then please support me at [GitHub Sponsors](https://github.com/stphnwlsh) or [Buy Me A Coffee](https://www.buymeacoffee.com/stphnwlsh)
