import { useState, useRef, useEffect } from 'react'
import * as signalR from "@microsoft/signalr";
import { HitTesting } from './domain/classes/hitTesting';
import { GraphicRender } from './domain/classes/graphics';
import { createPostIt } from "./domain/functions/postIt";

async function AddPostItCommand(connection: signalR.HubConnection | undefined, postIt: PostIt){
  try{
    await connection?.invoke("AddPostIt", postIt)
  }catch(error)
  {
    // todo return object to let user know something failed
    console.error("Failed to add postit");
  }
}

async function MovePostItCommand(connection: signalR.HubConnection | undefined, postIt: PostIt){
  try{
    await connection?.invoke("MovePostIt", postIt.id, postIt.x, postIt.y, postIt.version);
  }catch(error)
  {
    console.error("Failed to move postit");
  }
}

async function UpdateTextPostItCommand(connection: signalR.HubConnection | undefined, postIt: PostIt){
  try{
    await connection?.invoke("UpdatePostItText", postIt.id, postIt.label, postIt.version);
  }catch(error)
  {
    console.error("Failed to update text postit");
  }
}

async function DeletePostItCommand(connection: signalR.HubConnection | undefined, postIt: PostIt){
  try{
    await connection?.invoke("DeletePostIt", postIt.id, postIt.version)
  }catch(error)
  {
    console.error("Failed to add postit");
  }
}

async function GetBoardCommand(connection: signalR.HubConnection | undefined){
  try{
    await connection?.invoke("GetBoard")
  }catch(error)
  {
    console.error("Failed to get board");
  }
}

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
      .withUrl("http://localhost:5274/whiteboardHub")
      // retry connect with back off until die
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

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
    // todo introduce reconnect stratergy if connection fails while running
    // todo introduce onload stratergy to load board from server for any new clients 

    connection?.on("GetBoard", (board) => {
      setPostIts(board.postIts);
    })

    connection?.on("PostItAdded", (postIt) => {
      console.log("remote post added");
      setPostIts(previousPostIts => {
        const newPosts = [...previousPostIts, postIt];
        console.log(newPosts);
        return newPosts;
      });
    });

    connection?.on("PostItConflict", (result) => {
      // todo handle conflict
      // todo look at potentially introducing throttling to control mouse events
      console.log("conflict occoured!");
      console.log(result);
    });

    connection?.on("PostItMoved", (id, x, y, version) => {
      console.log("remote post moved");
      setPostIts(previousPostIts => {
        const newPosts = previousPostIts.map(item => {
          if(item.id == id)
          {
            return {
              ...item,
              x: x,
              y: y,
              version: version
            };
          }
          else{
            return item;
          }
        });
        console.log(newPosts);
        return newPosts;
      });
    });

    connection?.on("PostItTextUpdated", (id, text, version) => {
      console.log("remote post text updated");
      setPostIts(previousPostIts => {
        const newPosts = previousPostIts.map(item => {
          if(item.id == id)
          {
            return {
              ...item,
              label: text,
              version: version
            };
          }
          else{
            return item;
          }
        });
        console.log(newPosts);
        return newPosts;
      });
    });

    connection?.on("PostItDeleted", (id) => {
      console.log("remote post deleted");
      setPostIts(prevPostIts => prevPostIts.filter(item => item.id !== id));
    });

    connection?.on("Connected", (board, connectionId) => {
      setConnectionStatus("connected");
      setPostIts(board.postIts);
      setMessageList(prevmessages => [...prevmessages, connectionId]);
    });

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

  function saveEdit() {
    if (!editingItemId) return;

    setPostIts(prevPostIts => prevPostIts.map(item => {
      if(item.id === editingItemId)
      {
        const newPostIt = {
          ...item,
          label: editorValue,
          version: item.version + 1
        };

        UpdateTextPostItCommand(connection, newPostIt);

        return newPostIt;
      }
      else{
        return item;
      }
    }));

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

        // todo rework this back in
        // make it so the current item being dragged always appears on top
        // const reorderedItems = [...postIts];
        // const [selectedItem] = reorderedItems.splice(i, 1);
        // reorderedItems.push(selectedItem);

      
        // sendWhiteboardUpdate(reorderedItems);
        setDraggedItemId(item.id);
        return;
      }
    }
  }

  function handleMouseMove(event) {
    if (!draggedItemId) return;

    const mouse = getMousePos(event);
    setPostIts(previousPostIts => previousPostIts.map(item => {
      if (item.id === draggedItemId)
      {
        const newPostit = {
          ...item,
          x: mouse.x - offsetRef.current.x,
          y: mouse.y - offsetRef.current.y,
          version: item.version + 1
        }

        // send change to server
        MovePostItCommand(connection, newPostit);

        return newPostit;
      } else{
        return item;
      }
    }));
  }

  function stopDragging() {
    setDraggedItemId("");
  }

  function addItem() {
    const newItem = createPostIt();

    AddPostItCommand(connection, newItem);
    setPostIts(previousPostIts => [...previousPostIts, newItem]);
  }

  function deleteItem() {
    if (!editingItemId) return;

    const index = postIts.findIndex(i => i.id === editingItemId);
    const itemToDelete = {...postIts[index], version: postIts[index].version + 1};
    DeletePostItCommand(connection, itemToDelete);
    
    setPostIts(prevPostIts => prevPostIts.filter(item => item.id !== editingItemId));

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

        <button type="button" className='mt-3 rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-slate-700' onClick={() => GetBoardCommand(connection)}>Sync</button>

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
