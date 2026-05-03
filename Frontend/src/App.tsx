import { useState, useRef, useEffect } from 'react'
import * as signalR from "@microsoft/signalr";
import { HitTesting } from './domain/classes/hitTesting';
import { GraphicRender } from './domain/classes/graphics';
import { createPostIt } from "./domain/functions/postIt";


function App() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const [connection, setConnection] = useState<signalR.HubConnection>();
  const [messageList, setMessageList] = useState<Array<string>>([]);
  const [postIts, setPostIts] = useState<Array<PostIt>>([]);
  const [draggedItemId, setDraggedItemId] = useState<string>("");
  const [editingItemId, setEditingItemId] = useState<string>("");
  const [editorValue, setEditorValue] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [connectionStatus, setConnectionStatus] = useState("connecting");
  const [disable, setDisable] = useState(false);

  // click offset stored as ref to stop react re-rendering then updated
  const offsetRef = useRef({ x: 0, y: 0 });

  useEffect(() => {
    draw();
  }, [postIts]);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5025/whiteboardHub")
      // retry connect with back off until die
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Information)
      .build();


    connection.onreconnecting(error => {
      console.warn("SignalR reconnecting:", error);
      setConnectionStatus("reconnecting");
      setDisable(true);
      setErrorMessage("Connection lost. Reconnecting...");
    });

    connection.onreconnected(connectionId => {
      console.info("SignalR reconnected:", connectionId);
      setDisable(false);
      setConnectionStatus("connected");
      setErrorMessage("");
    });

    connection.onclose(error => {
      console.error("SignalR connection closed:", error);
      setConnectionStatus("disconnected");
      setErrorMessage(
        "The whiteboard server connection was lost. Please refresh the page to try again."
      );
    });

    setConnection(connection);
  }, []);

  async function startConnection() {
      try {
        await connection?.start();
        console.info("SignalR connected");
        setConnectionStatus("connected");
        setErrorMessage("");
      } catch (error) {
        console.error("SignalR connection failed:", error);
        setDisable(true);
        setConnectionStatus("unable to establish connection");
        setErrorMessage(
          "Could not connect to the whiteboard server. Please refresh the page to try again."
        );
      }
    }

  useEffect(() => {
    // Notify when a new user has joined
    connection?.on("RecieveNotification", (message, whiteboard) => {
      console.log(message);
      setMessageList(prev => prev.concat(message));
      setPostIts(whiteboard.postIts);
    });

    connection?.on("ReceiveMessage", (board => {
      setPostIts(board.postIts);
    }));

    startConnection();

    return () => {
      connection?.stop();
    }
  }, [connection]);

  function draw() {
    const canvas = canvasRef.current;
    const ctx = canvas?.getContext("2d");
    if (!ctx || !canvas) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const render = new GraphicRender();
    for (const item of postIts) {
      render.drawPostIt(ctx, item);
    }
  }

  function getMousePos(event) {
    const rect = canvasRef?.current?.getBoundingClientRect();
    if (!rect) throw Error("Unable to get mouse position!");

    return {
      x: event.clientX - rect.left,
      y: event.clientY - rect.top
    };
  }

  function startEditing(item: PostIt) {
    setEditingItemId(item.id);
    setEditorValue(item.label);

    setTimeout(() => {
      inputRef.current?.focus();
      inputRef.current?.select();
    }, 0)
  }

  async function sendWhiteboardUpdate(postIts: Array<PostIt>){
    try {
        // local cache?
        //setPostIts(postIts);
        
        // send to server for all clients
        await connection?.invoke("UpdateWhiteBoard", { PostIts: postIts })
        setErrorMessage("");
    } catch (error) {
      console.error("Failed to update whiteboard:", error);
      setConnectionStatus("connection failed");
      // not needed as we now disable canvas on reconnect?
      // setErrorMessage(
      //   "Your change could not be saved. Please check your connection and try again."
      // );
    }
  }

  function saveEdit() {
    if (!editingItemId) return;

    const newWhiteboard = postIts.map(item =>
      item.id === editingItemId
        ? { ...item, label: editorValue }
        : item
    );

    sendWhiteboardUpdate(newWhiteboard);
    setEditingItemId("");
  }

  function cancelEdit() {
    setEditingItemId("");
  }

  function handleMouseDown(event) {
    const hitTests = new HitTesting();
    const mouse = getMousePos(event);

    for (let i = postIts.length - 1; i >= 0; i--) {
      const item = postIts[i];

      if (hitTests.editButtonHit(item, mouse.x, mouse.y)) {
        startEditing(item);
        return;
      }

      if (hitTests.postItHit(item, mouse.x, mouse.y)) {
        cancelEdit();

        // track where on the postit the mouse clicked so it doesnt jump to exact mouse cursor postion when redrawn
        offsetRef.current = {
          x: mouse.x - item.x,
          y: mouse.y - item.y
        };

        // make it so the current item being dragged always appears on top
        const reorderedItems = [...postIts];
        const [selectedItem] = reorderedItems.splice(i, 1);
        reorderedItems.push(selectedItem);

        sendWhiteboardUpdate(reorderedItems);
        setDraggedItemId(selectedItem.id);
        return;
      }
    }
  }

  function handleMouseMove(event) {
    if (!draggedItemId) return;

    const mouse = getMousePos(event);
    const newWhiteboard = postIts.map(item =>
      item.id === draggedItemId
        ? {
          ...item,
          x: mouse.x - offsetRef.current.x,
          y: mouse.y - offsetRef.current.y
        }
        : item
    );
    sendWhiteboardUpdate(newWhiteboard);
  }

  function stopDragging() {
    setDraggedItemId("");
  }

  function addItem() {
    const newItem = createPostIt();
    sendWhiteboardUpdate([...postIts, newItem]);
  }

  function deleteItem() {
    if (!editingItemId) return;
    const newWhiteboard = postIts.filter(item => item.id !== editingItemId);
    sendWhiteboardUpdate(newWhiteboard);
    setEditingItemId("");
  }


  const editingItem = postIts.find(item => item.id === editingItemId) ?? null;

  const editorStyle = editingItem
    ? {
      left: editingItem.x,
      top: editingItem.y + editingItem.h + 8,
      width: editingItem.w
    }
    : {
      display: "none"
    };

  return (
    <div className="min-h-screen bg-slate-100 p-8 text-slate-900">
      <div className="mx-auto max-w-4xl">

        <div className="mb-4 flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold">Canvas Board</h1>
            <p className="text-sm text-slate-500">
              Drag, edit, add, and delete canvas items.
            </p>
          </div>

          <button
            onClick={addItem}
            className={`rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-slate-700 ${disable ? "cursor-not-allowed" : ""}`}
            disabled={disable}
          >
            + Add item
          </button>
        </div>

        <div className="relative overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-lg">

          <canvas
            ref={canvasRef}
            width={896}
            height={440}
            onMouseDown={handleMouseDown}
            onMouseMove={handleMouseMove}
            onMouseUp={stopDragging}
            onMouseLeave={stopDragging}
            className={disable ? "block pointer-events-none cursor-not-allowed" : "block active:cursor-grabbing"}
          />

          {editingItemId &&
            <div style={editorStyle} className='absolute flex'>
              <input
                ref={inputRef}
                value={editorValue}
                onChange={event => setEditorValue(event.target.value)}
                onBlur={saveEdit}
                style={{ width: "100%" }}
                onKeyDown={event => {
                  if (event.key === "Enter") saveEdit();
                  if (event.key === "Escape") cancelEdit();
                }}
                className='shrink-0 rounded-sm border border-slate-300 bg-white px-2 py-1 text-sm shadow-md outline-none ring-2 ring-blue-500'
              />

              <button
                type="button"
                className="rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-slate-700"
                aria-label="Delete item"
                onMouseDown={event => event.preventDefault()}
                onClick={deleteItem}
              >
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 16 16"
                  xmlns="http://www.w3.org/2000/svg"
                  aria-label="Delete"
                  role="img"
                >
                  <path
                    d="M6 2h4l1 1h3v2H2V3h3l1-1z"
                    fill="currentColor"
                  />
                  <path
                    d="M4 6h8l-.6 8H4.6L4 6z"
                    fill="currentColor"
                  />
                  <path
                    d="M6.5 7.5v5M9.5 7.5v5"
                    stroke="white"
                    strokeWidth="1"
                    strokeLinecap="round"
                  />
                </svg>
              </button>
            </div>
          }
        </div>

        <div className='pt-4'>
          <p>Connection status: {connectionStatus}</p>
          <p className='pb-4'>{errorMessage}</p>

          <h3 className='pb-2 font-bold'>Messages:</h3>
          {messageList.map(message => <p className='text-sm text-slate-500'>{message}</p>)}
        </div>
      </div>
    </div>
  );
}

export default App
