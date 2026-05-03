# Whiteboard

This project demonstrates a real-time collaboration board.

Whenever a new client connects to the board, they are given a connection id and the current state of the board is loaded. Any changes to the board are propagated to every client in realtime.

## Frontend React project

* Uses react library to build the UI
* Implements a drawing loop to render a canvas element whenever new events are recieved
* Subscribes and sends events to backend project

### Organisation

```
components/ = reusable UI
features/   = feature-specific UI + logic
hooks/      = reusable React hooks
domain/     = pure business logic, classes, algorithms
services/   = outside-world code: API, storage, analytics
lib/        = small generic utilities
```

## Backend .NET project

* Uses .NET and Signalr