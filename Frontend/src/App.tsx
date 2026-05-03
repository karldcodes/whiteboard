import { useState, useRef, useEffect } from 'react'
import * as signalR from "@microsoft/signalr";
import { randomWebSafeColor } from "./domain/functions/colors";
import { HitTesting } from './domain/classes/hitTesting';

function createItem(overrides = {}): PostIt {
  return {
    id: crypto.randomUUID(),
    x: 100,
    y: 100,
    w: 100,
    h: 60,
    color: "tomato",
    label: "New Item",
    ...overrides
  };
}

class PostItRender {
  private roundRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, radius: number, fill: string) {
    ctx.beginPath();
    ctx.roundRect(x, y, w, h, radius);
    ctx.fillStyle = fill;
    ctx.fill();
  }

  private drawEditButton(ctx: CanvasRenderingContext2D, item: PostIt) {
    ctx.fillStyle = "rgba(255,255,255,0.9)";
    this.roundRect(ctx, item.x + item.w - 38, item.y + 6, 32, 18, 2, ctx.fillStyle);

    ctx.fillStyle = "#334155";
    ctx.font = "12px sans-serif";
    ctx.fillText("Edit", item.x + item.w - 22, item.y + 15);
  }

  public drawItem(ctx: CanvasRenderingContext2D, item: PostIt) {
    this.roundRect(ctx, item.x, item.y, item.w, item.h, 6, item.color);

    ctx.fillStyle = "rgba(255,255,255,0.18)";
    ctx.fillRect(item.x, item.y, item.w, 28);

    ctx.fillStyle = "white";
    ctx.font = "600 15px sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(item.label, item.x + item.w / 2, item.y + item.h / 2 + 8);

    this.drawEditButton(ctx, item);
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

  // click offset stored as ref to stop react re-rendering then updated
  const offsetRef = useRef({ x: 0, y: 0 });

  useEffect(() => {
    draw();
  }, [postIts]);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5025/whiteboardHub")
      .withAutomaticReconnect()
      .build();
    setConnection(connection);
  }, []);

  useEffect(() => {
      // Notify when a new user has joined
      connection?.on("RecieveNotification", (message, whiteboard) => {
        console.log(message);
        setMessageList(prev => prev.concat(message));
        setPostIts(whiteboard.postIts);
      });

      // todo add logging
      connection?.start().catch((err) => console.error(err));

      connection?.on("ReceiveMessage", (board => {
        setPostIts(board.postIts);
      }));
    

    return () => { 
      connection?.stop(); 
    }
  }, [connection]);

  function draw() {
    const canvas = canvasRef.current;
    const ctx = canvas?.getContext("2d");
    if (!ctx || !canvas) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const render = new PostItRender();
    for (const item of postIts) {
      render.drawItem(ctx, item);
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

    const newWhiteboard = postIts.map(item =>
      item.id === editingItemId
        ? { ...item, label: editorValue }
        : item
    );

    connection?.invoke("UpdateWhiteBoard", { PostIts: newWhiteboard })
      .then(x => console.log("sent"))
      .catch(err => console.error(err));

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

        const reorderedItems = [...postIts];
        const [selectedItem] = reorderedItems.splice(i, 1);
        reorderedItems.push(selectedItem);

        connection?.invoke("UpdateWhiteBoard", { PostIts: reorderedItems })
          .then(x => console.log("sent"))
          .catch(err => console.error(err));
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

    connection?.invoke("UpdateWhiteBoard", { PostIts: newWhiteboard })
      .then(x => console.log("sent"))
      .catch(err => console.error(err));
  }

  function stopDragging() {
    setDraggedItemId("");
  }

  function addItem() {
    const newItem = createItem({
      color: randomWebSafeColor()
    });

    connection?.invoke("UpdateWhiteBoard", { PostIts: [...postIts, newItem] })
      .then(x => console.log("sent"))
      .catch(err => console.error(err));
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

  function deleteItem() {
    if (!editingItemId) return;

    const newWhiteboard = postIts.filter(item => item.id !== editingItemId);

    connection?.invoke("UpdateWhiteBoard", { PostIts: newWhiteboard })
      .then(x => console.log("sent"))
      .catch(err => console.error(err));

    setEditingItemId("");
  }

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
            className="rounded-xl bg-slate-900 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-slate-700"
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
            className="block active:cursor-grabbing"
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
          <h3 className='pb-2 font-bold'>Messages:</h3>
          {messageList.map(message => <p className='text-sm text-slate-500'>{message}</p>)}
        </div>
      </div>
    </div>
  );
}

export default App
