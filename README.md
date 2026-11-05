[![Clean Model Context Protocol](https://raw.githubusercontent.com/LeanMeanCleanApps/brand/main/assets/banners/png/mcp.png)](https://leanmeancleanapps.github.io/mcp/)

[![Build Pipeline](https://img.shields.io/github/actions/workflow/status/LeanMeanCleanApps/CleanModelContextProtocol/build-pipeline.yml?branch=main&label=Build%20Pipeline&logo=github&style=for-the-badge)](https://github.com/LeanMeanCleanApps/CleanModelContextProtocol/actions/workflows/build-pipeline.yml)
[![NuGet](https://img.shields.io/nuget/v/CleanModelContextProtocol.Template?label=NuGet&logo=nuget&logoColor=white&style=for-the-badge)](https://www.nuget.org/packages/CleanModelContextProtocol.Template)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CleanModelContextProtocol.Template?label=Downloads&logo=nuget&logoColor=white&style=for-the-badge)](https://www.nuget.org/packages/CleanModelContextProtocol.Template)
[![License](https://img.shields.io/github/license/LeanMeanCleanApps/CleanModelContextProtocol?label=License&style=for-the-badge)](LICENSE.md)

A `dotnet new` template for a [Model Context Protocol](https://modelcontextprotocol.io) server using the [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) in .NET 10, built with a streamlined Clean Architecture. All the layers you need, none of the ceremony you don't.

It's one of five [Lean Mean Clean Apps](https://leanmeancleanapps.github.io/) templates. They share the same Application and Infrastructure layers and differ only in how they face the world. Same genes, different haircuts.

The guides, the reference and the reasoning behind every opinion live at **[leanmeancleanapps.github.io/mcp](https://leanmeancleanapps.github.io/mcp/)**. This is the version for people who skim. We see you.

## Get started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). [Docker](https://www.docker.com/products/docker-desktop) is optional, but it's the quickest way to see it running.

```bash
# Install the template
dotnet new install CleanModelContextProtocol.Template

# Create a solution
dotnet new cleanmodelcontextprotocol --name WorksOnMyMachine.Movies --authorName "Your Name"
cd WorksOnMyMachine.Movies
```

| Option | What it does |
| --- | --- |
| `--name`, `-n` | Names the solution and its root namespace |
| `--authorName`, `-an` | Puts your name on the assembly instead of ours |
| `--authority`, `-iss` | Your OAuth authorization server. Leave it out and the server runs without authentication |
| `--audience`, `-aud` | The audience tokens must be issued for, usually the server's own URL |
| `--no-restore` | Skips the package restore that normally runs once the solution is created |

Rather click than type? Visual Studio and Rider show the same options in their new project dialogs, and `dotnet new cleanmodelcontextprotocol --help` lists them in the terminal.

That's three layers, four test projects, a Dockerfile and a CI pipeline. Go and tell your team you've been working on it all week.

## Run it

```bash
# Build the image and start it with sample data, plus the MCP Inspector, a collector and the Aspire dashboard
docker compose up -d --build

# Ask the movies_search tool for a page of movies
curl http://localhost:8080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "movies_search",
      "arguments": { "query": "global", "limit": 2 }
    }
  }'
```

The answer comes back as an event stream, and its result carries the movies as structured content:

```json
{
  "items": [
    {
      "id": "30fca414-247a-779e-105a-d81aa2506f58",
      "title": "Assimilated global infrastructure",
      "reviewCount": 1,
      "averageStars": 1
    },
    {
      "id": "c27d6600-79a6-9346-2595-a52b7f07dc3e",
      "title": "Automated global alliance",
      "reviewCount": 3,
      "averageStars": 3.3
    }
  ],
  "totalCount": 2
}
```

Yes, the sample movies are named by a corporate buzzword generator, and the critics are harsh.

Curl is a fine way to prove it works and a terrible way to spend an afternoon. Open the [MCP Inspector](https://github.com/modelcontextprotocol/inspector) at <http://localhost:6274> instead. It's already pointed at the server, so flip the switch to connect and open **Tools**.

There are six tools shaped around jobs rather than tables: `movies_search`, `movies_get`, `authors_search`, `reviews_list`, `reviews_write` and `reviews_delete`. Each has a description written for agents and hints that say what's read-only and what's destructive. Lists come back ten at a time, with a `totalCount` and a `nextOffset` when there's more to come, so an agent doesn't drown in its own context window. There are resources for every movie, author and review, and prompts for jobs like comparing two movies. Its logs, traces and metrics land in the [Aspire dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard/standalone) at <http://localhost:18888>.

No Docker? `dotnet run --project src/Presentation` works too. Every other way to run it, from plain `docker run` to stamping a version into the image, is in [docs/RUN.md](docs/RUN.md).

The server is open until you give it an authorization server. Pass `--authority` and `--audience` when you create the solution, or set `Authentication:Authority` later. `/mcp` then asks for a bearer token, and the server publishes [protected resource metadata](https://modelcontextprotocol.io/specification/latest/basic/authorization) so MCP clients can find your authorization server and sign in by themselves. [Securing the server](https://leanmeancleanapps.github.io/mcp/guides/secure-the-server/) has the details.

## What's inside

Three layers, loosely based on Clean Architecture. Fewer layers than an onion, more than a sandwich.

- **Presentation** is the MCP server, stateless Streamable HTTP at `/mcp`. Its tools, resources and prompts send each request through [Mediator](https://github.com/martinothamar/Mediator), and Application errors become tool errors that tell the agent how to recover.
- **Application** owns the rules. Commands and queries are validated by [FluentValidation](https://github.com/FluentValidation/FluentValidation) and mapped by [Mapperly](https://mapperly.riok.app).
- **Infrastructure** owns the data, with [Entity Framework Core](https://learn.microsoft.com/ef/core) on SQLite. Everything in it is `internal`, so Presentation can wire it up but can't move in.

There's a little CQRS in how commands and queries are shaped, but reads and writes share one database underneath. CQRS-curious, not CQRS-committed.

And around the edges:

- **Tests** use [xUnit](https://xunit.net), [Shouldly](https://github.com/shouldly/shouldly) and [NSubstitute](https://github.com/nsubstitute/NSubstitute), with coverage from [Coverlet](https://github.com/coverlet-coverage/coverlet). Each test gets its own throwaway in-memory database, so no test can spoil another one's data.
- **Sample data** comes from [Bogus](https://github.com/bchavez/Bogus), and only when `Database:SeedSampleData` is on. A real deployment never starts with fake reviews.
- **Telemetry** is [OpenTelemetry](https://opentelemetry.io) all the way: a span for every request, tool call and use case, metrics from ASP.NET Core, the MCP SDK, EF Core and the .NET runtime, and [Serilog](https://github.com/serilog/serilog) logs that know which trace they belong to. It all leaves over OTLP when `OTEL_EXPORTER_OTLP_ENDPOINT` is set, so point it at whatever backend you like. Docker Compose runs an [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/) that forwards it to the Aspire dashboard, the same shape you'd run in production. Health checks answer at `/health/live` and `/health/ready`.
- **Build rules** live in `Directory.Build.props`. Code style and the recommended analyzers run on every build, warnings fail it, and package versions sit in one place with [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management).
- **The container** runs as a non-root user and has its own health check.
- **The pipeline** builds, checks formatting, tests with a 95% coverage bar, makes sure the packed template still works, runs [CodeQL](https://codeql.github.com), reviews new dependencies and their licences, starts the image to check it actually answers, and scans the container and Dockerfile with [Trivy](https://trivy.dev). Then it sums the lot up in one comment on your pull request, so you know exactly who to blame. It needs no accounts, tokens or secrets, so your copy works the moment you push it. Trust is earned.
- **Dependabot** bundles the month's dependency updates into one pull request on the 15th.

The [architecture page](https://leanmeancleanapps.github.io/mcp/explanation/architecture/) has the long version, with full credit to [Jason Taylor's template](https://github.com/jasontaylordev/CleanArchitecture) and [Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) for the ideas.

## Releases

A new version lands on the 1st of each month, as long as something changed, with notes for every pull request in the [GitHub release](https://github.com/LeanMeanCleanApps/CleanModelContextProtocol/releases). No changes, no release. We're not here to pad the download stats. Each release also carries an SPDX bill of materials listing every package the server ships with, for anyone whose security team asks nicely (or not so nicely).

```bash
dotnet new update
```

## The family

| Template | Presentation |
| --- | --- |
| [Clean Minimal API](https://github.com/LeanMeanCleanApps/CleanMinimalApi) | ASP.NET Core Minimal APIs |
| **Clean Model Context Protocol** | Model Context Protocol server, you are here |
| [Clean GraphQL API](https://github.com/LeanMeanCleanApps/CleanGraphQLApi) | GraphQL with Hot Chocolate |
| [Clean Web API](https://github.com/LeanMeanCleanApps/CleanWebApi) | ASP.NET Core controllers |
| [Clean Lambda Function](https://github.com/LeanMeanCleanApps/CleanLambdaFunction) | AWS Lambda |

## Contributing

Bug reports, ideas and pull requests are all welcome, and typo fixes doubly so. Start with the [contributing guide](CONTRIBUTING.md) and the [code of conduct](CODE_OF_CONDUCT.md). Security problems go through the [security policy](SECURITY.md), not an issue.

## Who built this

[<img src="https://github.com/stphnwlsh.png?size=96" width="48" align="left" alt="">](https://github.com/stphnwlsh)
Built by **Stephen Walsh** in spare time, with strong coffee and a healthy disregard for pomp and ceremony. Say hello on [GitHub](https://github.com/stphnwlsh), [LinkedIn](https://www.linkedin.com/in/stphnwlsh), [Medium](https://stphnwlsh.medium.com) or at [stphnwlsh.github.io](https://stphnwlsh.github.io).

If this template saves you some time, [sponsoring the work](https://github.com/sponsors/stphnwlsh) keeps all five templates current and the coffee flowing, which is the real dependency here.

## License

[MIT](LICENSE.md). Take it, change it, ship it. Just don't blame us for what your agent does with it.
