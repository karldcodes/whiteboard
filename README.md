# Whiteboard

This project demonstrates a real-time collaboration board.

Whenever a new client connects to the board, they are given a connection id and the current state of the board is loaded. Any changes to the board are propagated to every client in realtime.

<img src="whiteboard.png" alt="whiteboard running" />

### Currently you can

* Add a new postit
* Edit the label
* Delete a postit
* Organise postit's by clicking and dragging

### Error handling

* Uses exponential backoff when attempting to reconnect
* Manual sync button if things appear off (automatic refresh in future)

## Prerequisites 

* .NET 10 SDK
* node LTS

## Running the project

1. download repo
2. create a terminal in backend project and type `dotnet run --project src/Whiteboard.Api`
3. create a terminal in frontend project and type `npm run dev`
4. load url provided by terminal in frontend project

## Frontend React project

* Uses react library to build the UI
* Uses tailwindcss for styling
* Implements a drawing loop to render a canvas element whenever new events are recieved
* Subscribes and sends events to backend project

## Backend .NET project

* Uses .NET and Signalr to provide real time events
* Provides state management to hold the state of the board while the backend is running


# Todo

* Investigate ways to restore or merge local state with server state so users can work offline
* Implement central logger with tracing
* Investigate signalr backplane for scaling in microservices
* Add unit tests to frontend
* Change the backend code to save the board state in a database instead of in memory
* Updated frontend so you can write large amounts of text
* Add user management and permissions
* Add board management UI screens
* Remove references to hardcoded urls
* Organise backend project into correct folders for interfaces, services etc
* Finish off conflicts in FE for syncing stale state
* Add back in automatic reconnecting and state refresh after branch merge