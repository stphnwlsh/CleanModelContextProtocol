---
name: mcp-inspector
description: How to try the MCP server by hand with the official MCP Inspector that docker compose starts beside it - where it runs, how it connects, and what to check before calling a tool, resource or prompt change done. Use when you want to see tools, resources or prompts as a client sees them, or debug what the model receives.
metadata:
  scope: presentation
---

# MCP Inspector

## The rule

`docker-compose.yml` runs the official [MCP Inspector](https://github.com/modelcontextprotocol/inspector) next to the server, already pointed at `http://api:8080/mcp` over streamable HTTP. It's the quickest way to see exactly what a client gets: every tool with its description, input and output schema and annotations, every resource and template, and every prompt. The image version is pinned in the compose file, and the port is bound to `127.0.0.1` only.

| Service | Address |
| --- | --- |
| MCP server | <http://localhost:8080/mcp> |
| MCP Inspector | <http://localhost:6274> |
| Aspire dashboard (logs, traces, metrics) | <http://localhost:18888> |

## Steps

1. Start everything: `docker compose up -d --build`.
2. Open <http://localhost:6274>, flip the switch on the server card to connect, then open **Tools**, **Resources** or **Prompts**.
3. After changing a tool, check its description and every argument's description read well, the annotations are right, and a call returns the result model you expect.
4. Try the failure paths too: an unknown id, a star rating of 6. The model should get a message it can act on.
5. Find the tool call in the Aspire dashboard if something looks wrong: its trace shows the tool, the use case and each SQL command, and its logs sit beside it (`telemetry`).
6. With authentication on, the Inspector needs a token from your authority. Leave `Authentication:Authority` empty for local checks.

## Chat demo

To try the tools the way a model does, add the optional `demo` profile: `docker compose --profile demo up -d --build`. It starts [Open WebUI](https://github.com/open-webui/open-webui) at <http://localhost:3000>, already connected to the server and using Kilo's free gateway as the model, with no account or key. It's online only, and your prompts and tool results go to Kilo, a third party, so use it with sample data. Plain `docker compose up` doesn't start it, and solutions created with `--include-auth` leave it out, because Open WebUI has no token to send.

## Don't

- Expose port 6274 beyond `127.0.0.1`. The Inspector can call every tool.
- Use `latest` for the Inspector image. Change the pinned version on purpose.
- Treat a manual check as a test. Write the integration test as well (`presentation-integration-tests`).

## Check

``` bash
docker compose up -d --build && curl -fsS http://localhost:8080/health/ready
docker compose down
```
