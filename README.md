# Reproduction of Open Telemetry issue with Orleans

This project is based on the Simple Streaming sample. It has been updated to Orleans 9 and .NET 9.

## Getting started

1. Start the Event Hubs emulator locally using [these instructions](https://learn.microsoft.com/en-us/azure/event-hubs/test-locally-with-event-hub-emulator?tabs=automated-script%2Cusing-kafka)

2. Start the standalone Aspire Dashboard and open it at `http://localhost:18888`

```pwsh
docker run --rm -it `
    -p 18888:18888 -p 4317:18889 `
    -d --name aspire-dashboard `
    -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS="true" `
    mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

3. Start the `SiloHost` project
4. Start the `Client` project


Here's what's happening:
- Producer grain sends messages to a stream backed by an Event Hub
- Consumer grain is subscribed to the stream and receives the messages
- Consumer grain forwards the message to a Processing grain

## What's wrong?
If you look closely at the traces in the Aspire Dashboard, you should see some traces that contain dozens to hundreds of spans. Most of these should be part of other traces.

![image](https://github.com/user-attachments/assets/5aaf5002-e130-4e15-8975-47c64825dc6f)
