export class GraphicRender {
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

  public drawPostIt(ctx: CanvasRenderingContext2D, item: PostIt) {
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