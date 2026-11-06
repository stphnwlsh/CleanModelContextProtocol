![Clean Model Context Protocol](https://raw.githubusercontent.com/LeanMeanCleanApps/brand/main/assets/banners/png/mcp.png)

A `dotnet new` template for a [Model Context Protocol](https://modelcontextprotocol.io) server using the [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) in .NET 10, built with a streamlined Clean Architecture. All the layers you need, none of the ceremony you don't.

It's one of five [Lean Mean Clean Apps](https://leanmeancleanapps.github.io/) templates, for Minimal APIs, Model Context Protocol, GraphQL, Web API controllers and AWS Lambda. They share the same Application and Infrastructure layers and differ only in how they face the world. The full docs live at [leanmeancleanapps.github.io/mcp](https://leanmeancleanapps.github.io/mcp/).

| Template | Presentation | Package | Docs |
| --- | --- | --- | --- |
| Clean Minimal API | ASP.NET Core Minimal APIs | [CleanMinimalApi.Template](https://www.nuget.org/packages/CleanMinimalApi.Template) | [Docs](https://leanmeancleanapps.github.io/minimalapi/) |
| **Clean Model Context Protocol** | Model Context Protocol server, you are here | [CleanModelContextProtocol.Template](https://www.nuget.org/packages/CleanModelContextProtocol.Template) | [Docs](https://leanmeancleanapps.github.io/mcp/) |
| Clean GraphQL API | GraphQL with Hot Chocolate | [CleanGraphQLApi.Template](https://www.nuget.org/packages/CleanGraphQLApi.Template) | [Docs](https://leanmeancleanapps.github.io/graphql/) |
| Clean Web API | ASP.NET Core controllers | [CleanWebApi.Template](https://www.nuget.org/packages/CleanWebApi.Template) | [Docs](https://leanmeancleanapps.github.io/webapi/) |
| Clean Lambda Function | AWS Lambda | [CleanLambdaFunction.Template](https://www.nuget.org/packages/CleanLambdaFunction.Template) | [Docs](https://leanmeancleanapps.github.io/lambda/) |

## Get started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). [Docker](https://www.docker.com/products/docker-desktop) is optional, but it's the quickest way to see it running.

```bash
# Install the template
dotnet new install CleanModelContextProtocol.Template

# Create a solution
dotnet new cleanmodelcontextprotocol --name WorksOnMyMachine.Movies --authorName "Your Name"
cd WorksOnMyMachine.Movies
```

`cleanmcp` works too, for anyone who'd rather not type `cleanmodelcontextprotocol` every time, and so do `lmca-mcp` and `leanmeancleanapps-mcp`. `dotnet new search leanmeanclean` lists the whole family.

| Option | Default | What it does |
| --- | --- | --- |
| `--name`, `-n` | The folder's name | Names the solution and its root namespace |
| `--authorName`, `-an` | `Stephen Walsh` | Puts your name on the assembly instead of ours |
| `--include-auth` | Off | Adds OAuth for MCP clients. Leave it out and the server has no auth code at all |
| `--authority`, `-iss` | Empty | Your OAuth authorization server. Required with `--include-auth` |
| `--audience`, `-aud` | Empty | The audience tokens must be issued for, usually the server's own URL. Only used with `--include-auth` |
| `--no-restore` | Off | Skips the package restore that normally runs once the solution is created |

Rather click than type? Visual Studio and Rider show the same options in their new project dialogs. They always show the authority and audience fields, but only use them when **Include authentication** is ticked. In the terminal, `dotnet new cleanmodelcontextprotocol --help` lists them all.

## Run it

```bash
# Build the image and start it with sample data, plus the MCP Inspector, a collector and the Aspire dashboard
docker compose up -d --build

# Run the tests while you're here
dotnet test
```

Then open the [MCP Inspector](https://github.com/modelcontextprotocol/inspector) at <http://localhost:6274>. It's already pointed at the server at <http://localhost:8080/mcp>, so flip the switch to connect and open **Tools**.

The server is open unless you create it with `--include-auth --authority` (and usually `--audience`), and then `/mcp` asks for a bearer token and publishes protected resource metadata so clients can sign in. Leave `--include-auth` out and there's no auth code at all.

## What's inside

- **Presentation**: the MCP server, with six task-shaped tools, resources and prompts over stateless Streamable HTTP
- **Application**: commands and queries through [Mediator](https://github.com/martinothamar/Mediator), validated by [FluentValidation](https://github.com/FluentValidation/FluentValidation) and mapped by [Mapperly](https://mapperly.riok.app)
- **Infrastructure**: [Entity Framework Core](https://learn.microsoft.com/ef/core) on SQLite, with optional sample data
- **Tests**: [xUnit](https://xunit.net), [Shouldly](https://github.com/shouldly/shouldly) and [NSubstitute](https://github.com/nsubstitute/NSubstitute), each on its own throwaway database
- **Extras**: [OpenTelemetry](https://opentelemetry.io) traces, metrics and [Serilog](https://github.com/serilog/serilog) logs over OTLP, health checks, a Dockerfile that runs as a non-root user, Docker Compose, and a GitHub Actions pipeline that won't let your coverage slip

## Releases

A new version lands on the 1st of each month, as long as something changed. No changes, no release. We're not here to pad the download stats.

```bash
dotnet new update
```

## More help

- [Docs](https://leanmeancleanapps.github.io/mcp/) for guides, a reference and why it's built this way
- [GitHub](https://github.com/LeanMeanCleanApps/CleanModelContextProtocol) for the code, [issues](https://github.com/LeanMeanCleanApps/CleanModelContextProtocol/issues) and [release notes](https://github.com/LeanMeanCleanApps/CleanModelContextProtocol/releases)
- [Security policy](https://github.com/LeanMeanCleanApps/CleanModelContextProtocol/security/policy) to report a vulnerability privately

## Who built this

Built by [Stephen Walsh](https://github.com/stphnwlsh) in spare time, with strong coffee and a healthy disregard for pomp and ceremony. If this template saves you some time, [sponsoring the work](https://github.com/sponsors/stphnwlsh) keeps all five templates current and the coffee flowing.
