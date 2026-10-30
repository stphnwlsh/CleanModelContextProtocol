<picture>
  <source media="(prefers-color-scheme: light)" srcset="https://raw.githubusercontent.com/LeanMeanCleanApps/brand/main/assets/logos/mcp/png/mcp-light-256.png">
  <img src="https://raw.githubusercontent.com/LeanMeanCleanApps/brand/main/assets/logos/mcp/png/mcp-256.png" alt="Clean Model Context Protocol logo" width="128" align="right">
</picture>

# Clean Model Context Protocol

[![Build Pipeline](https://img.shields.io/github/actions/workflow/status/stphnwlsh/CleanModelContextProtocol/build-pipeline.yml?branch=main&label=Build%20Pipeline&logo=github&style=for-the-badge)](https://github.com/stphnwlsh/CleanModelContextProtocol/actions/workflows/build-pipeline.yml)
[![Code Coverage](https://img.shields.io/codecov/c/github/stphnwlsh/CleanModelContextProtocol?label=Code%20Coverage&logo=codecov&logoColor=white&style=for-the-badge)](https://codecov.io/gh/stphnwlsh/CleanModelContextProtocol)
[![NuGet](https://img.shields.io/nuget/v/CleanModelContextProtocol.Template?label=NuGet&logo=nuget&logoColor=white&style=for-the-badge)](https://www.nuget.org/packages/CleanModelContextProtocol.Template)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CleanModelContextProtocol.Template?label=Downloads&logo=nuget&logoColor=white&style=for-the-badge)](https://www.nuget.org/packages/CleanModelContextProtocol.Template)
[![License](https://img.shields.io/github/license/stphnwlsh/CleanModelContextProtocol?label=License&style=for-the-badge)](LICENSE.md)
[![GitHub Sponsors](https://img.shields.io/github/sponsors/stphnwlsh?label=Sponsor&logo=githubsponsors&logoColor=white&color=ea4aaa&style=for-the-badge)](https://github.com/sponsors/stphnwlsh)

A `dotnet new` template for a [Model Context Protocol](https://modelcontextprotocol.io) server built with a streamlined version of Clean Architecture and the [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk). All the layers you need, none of the ceremony you don't.

It's one of five [Lean Mean Clean Apps](https://leanmeancleanapps.github.io/) templates that share the same Application and Infrastructure layers and differ only in how they present them. Same genes, different haircuts. See [Related templates](#related-templates).

The full documentation, with guides, a reference and the reasoning behind every opinion in here, lives at **[leanmeancleanapps.github.io/mcp](https://leanmeancleanapps.github.io/mcp/)**. This README is the short version for people who skim. We see you.

## Contents

- [Getting started](#getting-started)
- [Running the project](#running-the-project)
- [Architecture](#architecture)
- [Features](#features)
- [Releases](#releases)
- [Related templates](#related-templates)
- [Resources](#resources)
- [Contributing](#contributing)
- [The team](#the-team)
- [License](#license)

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/products/docker-desktop) (optional, to build and run the container image)

### Install the template

``` bash
dotnet new install CleanModelContextProtocol.Template
```

### Create a solution

``` bash
dotnet new cleanmodelcontextprotocol --name {YOUR_SOLUTION_NAMESPACE} --authorName "{YOUR_NAME}"
```

That's it. You now have three layers, four test projects, a Dockerfile and a CI pipeline. Go and tell your team you've been working on it all week.

### Protect the server with OAuth (optional)

The server runs without authentication unless you give it an authorization server. Pass one when you create the solution, or set `Authentication:Authority` and `Authentication:Audience` in `appsettings.json` later.

``` bash
dotnet new cleanmodelcontextprotocol --name {YOUR_SOLUTION_NAMESPACE} --authority https://{YOUR_AUTHORIZATION_SERVER} --audience https://{YOUR_SERVER_URL}
```

With an authority set, `/mcp` needs a bearer token issued for the audience, and the server publishes [protected resource metadata](https://modelcontextprotocol.io/specification/latest/basic/authorization) at `/.well-known/oauth-protected-resource/mcp` so MCP clients can find the authorization server and sign in. List the scopes clients should ask for in `Authentication:Scopes`.

## Running the project

``` bash
docker compose up -d
```

Then open the [MCP Inspector](https://github.com/modelcontextprotocol/inspector) at <http://localhost:6274>. It's already pointed at the MCP endpoint (<http://localhost:8080/mcp>), so connect and open **Tools** to browse and run them. See [docs/RUN.md](docs/RUN.md) for every way to run it.

The `Dockerfile` has stages for testing, publishing and running, so you can build just the part you need. You can also stamp a version into the assembly.

``` bash
docker build . --target run --tag cleanmodelcontextprotocol:latest --build-arg VERSION_PREFIX={VERSION_NUMBER} --build-arg VERSION_SUFFIX={PRERELEASE_NAME}
```

## Architecture

This solution is loosely based on Clean Architecture. It's by no means perfect, so I prefer to call it "Lean Mean Clean Architecture". Fewer layers than an onion, more than a sandwich. Inspiration comes from [Jason Taylor's Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture), with some ideas taken further and others scaled back.

There's a little CQRS in the style of the commands and queries, but reads and writes use the same data source underneath. CQRS-curious, not CQRS-committed.

The one deliberate break from Clean Architecture is that the Presentation project references the Infrastructure project, so it can wire up dependency injection. To protect that boundary, every class in Infrastructure is `internal`, so it can't be used from Presentation by accident. Presentation can introduce itself, but it can't move in.

### Project structure

1. **Presentation** - Exposes the Application layer to AI clients as an MCP server over HTTP. Tools are grouped by feature, send commands and queries through Mediator, and turn Application errors into MCP tool errors.
1. **Application** - Owns the domain and business logic. Commands and queries are validated and handled here, and each domain type has its own repository interface. This layer doesn't care what fulfils those contracts, as long as something does.
1. **Infrastructure** - Owns the data. It implements the repository interfaces to fetch, create, update and remove data, and maps its own data models onto the Application entities so persistence details don't leak upwards.

## Features

- MCP server using the [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk), stateless Streamable HTTP at `/mcp`
- Six task-shaped tools (`movies_search`, `movies_get`, `authors_search`, `reviews_list`, `reviews_write`, `reviews_delete`) with titles, read-only, destructive and idempotent hints, and descriptions written for agents
- Resources for each movie, author and review (`movies://{movieId}`, `authors://{authorId}`, `reviews://{reviewId}`) and the server version (`server://version`)
- Prompts for common jobs (`summarise_movie_reception`, `compare_movies`, `review_movie`) that embed the movies they're about, with completions that suggest movie titles and author names as you type
- Search with paging and small results by default: every list returns 10 items, a total count and the offset of the next page, with names and average ratings next to ids
- Optional OAuth protection for `/mcp`: JWT bearer tokens plus MCP protected resource metadata, turned on by setting an authority
- Logging using [Serilog](https://github.com/serilog/serilog), with [Seq](https://datalust.co/seq) when running in Docker
- Mediator pattern using [Mediator](https://github.com/martinothamar/Mediator)
- Validation using [FluentValidation](https://github.com/FluentValidation/FluentValidation) in an Application pipeline behaviour
- Typed tool results published with an output schema and returned as structured content as well as JSON text
- Errors mapped from Application exceptions by request filters: tool errors tell the agent how to recover, and resources and prompts return MCP protocol errors such as resource not found
- Object mapping using [Mapperly](https://mapperly.riok.app)
- Data stored in [SQLite](https://learn.microsoft.com/ef/core/providers/sqlite/) using [Entity Framework Core](https://learn.microsoft.com/ef/core) migrations, seeded with [Bogus](https://github.com/bchavez/Bogus) only when `Database:SeedSampleData` is on. Each test uses its own throwaway in-memory SQLite database, so tests never touch a real one or each other's data
- Testing using [xUnit](https://xunit.net), [Shouldly](https://github.com/shouldly/shouldly) and [NSubstitute](https://github.com/nsubstitute/NSubstitute), with coverage from [Coverlet](https://github.com/coverlet-coverage/coverlet)
- Build rules in `Directory.Build.props`: code style and the recommended analyzers are checked on every build, warnings fail it, and package versions live in one place with [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management)
- CI with GitHub Actions: build and test, a `dotnet format` check, 95% coverage gate with at least 90% on the lines each pull request changes, [Trivy](https://trivy.dev) container scan, [CodeQL](https://codeql.github.com) static analysis, then a monthly NuGet release and GitHub release from `main` using [trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
- Health checks at `/health/live` and `/health/ready`, using [ASP.NET Core health checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- Grouped dependency updates on the 15th of each month using [Dependabot](https://docs.github.com/code-security/dependabot), including the pinned Seq image
- A Docker image that runs as a non-root user with a health check

## Releases

The template is released once a month. On the 1st, at 9am US Central time, the pipeline checks for changes since the last release. If anything outside `.github/` has changed, it builds and tests `main`, publishes the new version to [NuGet](https://www.nuget.org/packages/CleanModelContextProtocol.Template) and creates a [GitHub release](https://github.com/stphnwlsh/CleanModelContextProtocol/releases) with notes for every pull request merged since the previous one. If nothing has changed, there's no release that month, because nobody needs a new version number for the same code. Dependency updates arrive on the 15th, so they're merged in time for the next release.

Versions keep the `YYYY.MM.DD.RUN` format. Up to the end of October 2026, every merge to `main` was released, so the older releases are much smaller. To release outside the monthly cycle, run the **Build Pipeline** workflow by hand. Tick **force** to release even when only `.github/` has changed.

To update an installed template to the latest release:

``` bash
dotnet new update
```

## Related templates

All five templates share the same Application and Infrastructure layers, tests, CI pipeline and repository setup.

| Template | Presentation | Package |
| --- | --- | --- |
| [Clean Minimal API](https://github.com/stphnwlsh/CleanMinimalApi) | ASP.NET Core Minimal APIs | [CleanMinimalApi.Template](https://www.nuget.org/packages/CleanMinimalApi.Template) |
| **Clean Model Context Protocol** (this repo) | Model Context Protocol server | [CleanModelContextProtocol.Template](https://www.nuget.org/packages/CleanModelContextProtocol.Template) |
| [Clean GraphQL API](https://github.com/stphnwlsh/CleanGraphQLApi) | GraphQL with Hot Chocolate | [CleanGraphQLApi.Template](https://www.nuget.org/packages/CleanGraphQLApi.Template) |
| [Clean Web API](https://github.com/stphnwlsh/CleanWebApi) | ASP.NET Core controllers | [CleanWebApi.Template](https://www.nuget.org/packages/CleanWebApi.Template) |
| [Clean Lambda Function](https://github.com/stphnwlsh/CleanLamdaFunction) | AWS Lambda function | [CleanLambdaFunction.Template](https://www.nuget.org/packages/CleanLambdaFunction.Template) |

## Resources

This template wouldn't exist without these resources. If you're on your own learning adventure, they're well worth a read. Better than the comments section, anyway.

- [What is the Model Context Protocol (MCP)?](https://modelcontextprotocol.io/docs/getting-started/intro)
- [MCP C# SDK documentation](https://modelcontextprotocol.github.io/csharp-sdk/)
- [James Montemagno - Build a Model Context Protocol (MCP) server in C#](https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp)
- [Sam Basu - Build & Leverage MCP Servers in C# for AI-Driven Development](https://platform.uno/blog/build-leverage-mcp-servers-in-c-for-ai-driven-development)
- [Laurent Kempé - SSE-Powered MCP Server with C# and .NET in 15.7MB executable](https://laurentkempe.com/2025/04/05/sse-powered-mcp-server-with-csharp-and-dotnet-in-157mb-executable)
- [Will Velida - Building a Sports-Themed MCP Server Using .NET](https://dev.to/willvelida/building-a-sports-themed-mcp-server-using-net-22ln)
- [Jose Maria Flores Zazo - From API to MCP: How to Expose Your Endpoints Securely to AI](https://medium.com/@jmfloreszazo/from-api-to-mcp-how-to-expose-your-endpoints-securely-to-ai-4aecc84cee28)
- [Andrew Lock - Reducing log verbosity with Serilog RequestLogging](https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-reducing-log-verbosity/)
- [Jason Taylor - Clean Architecture Solution Template](https://github.com/jasontaylordev/CleanArchitecture)
- [Robert C. Martin - The Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## Contributing

Bug reports, ideas and pull requests are welcome, and typo fixes are welcome with open arms. Please read the [contributing guide](CONTRIBUTING.md) and the [code of conduct](CODE_OF_CONDUCT.md) first. To report a security vulnerability, follow the [security policy](SECURITY.md) instead of opening an issue.

## The team

This template is part of [Lean Mean Clean Apps](https://leanmeancleanapps.github.io/), a small family of .NET templates built and looked after in spare time.

| | Who | What they do | Find them |
| --- | --- | --- | --- |
| <img src="https://github.com/stphnwlsh.png?size=96" width="48" alt=""> | **Stephen Walsh** | Maintainer. Builds every template, keeps them current with each .NET release, and drinks the coffee. | [GitHub](https://github.com/stphnwlsh) · [LinkedIn](https://www.linkedin.com/in/stphnwlsh) · [Medium](https://stphnwlsh.medium.com) · [Website](https://stphnwlsh.github.io) |

Want your name in this table? Pull requests are the way in, so start with the [contributing guide](CONTRIBUTING.md). For help using the template, see [SUPPORT.md](SUPPORT.md).

### Support the work

If this template saves you time, please consider [sponsoring the work on GitHub](https://github.com/sponsors/stphnwlsh). It keeps all five templates current with each .NET release, and it keeps the coffee flowing, which is the real dependency here.

## License

Released under the [MIT License](LICENSE.md). Take it, change it, ship it. Just don't blame us for your production incidents.
