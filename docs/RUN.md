# Running Clean Model Context Protocol

There are two pre-configured environments, `local` and `docker`. The matching `appsettings.*.json` files decide where logs go: the console when running locally, and a [Seq](https://datalust.co/seq) container when running in Docker.

## .NET CLI

`launchSettings.json` sets the ports, environment and logging, so one command is enough.

``` bash
dotnet run --project ./src/Presentation
```

## Docker Compose

`docker-compose.yml` starts the app, the [MCP Inspector](https://github.com/modelcontextprotocol/inspector) already pointed at it, and a Seq container for logs.

``` bash
docker compose up -d --build
```

- App: <http://localhost:8080/mcp> (MCP endpoint, Streamable HTTP)
- MCP Inspector: <http://localhost:6274>. Flip the switch on the server card to connect, then open **Tools** to see every tool with its description and input schema and run it.
- Seq: <http://localhost:5341>

To shut the containers down:

``` bash
docker compose down
```

## Tests

``` bash
dotnet test
```
