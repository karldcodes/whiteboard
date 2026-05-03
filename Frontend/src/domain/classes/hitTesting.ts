export class HitTesting {
  public postItHit(item: PostIt, x: number, y: number) {
    return (
      x >= item.x &&
      x <= item.x + item.w &&
      y >= item.y &&
      y <= item.y + item.h
    );
  }

  public editButtonHit(item: PostIt, x: number, y: number) {
    return (
      x >= item.x + item.w - 38 &&
      x <= item.x + item.w - 6 &&
      y >= item.y + 6 &&
      y <= item.y + 24
    );
  }
}