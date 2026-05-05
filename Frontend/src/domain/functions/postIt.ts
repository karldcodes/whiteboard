import { randomWebSafeColor } from "./colors";

export function createPostIt(): PostIt {
  return {
    id: crypto.randomUUID(),
    x: 100,
    y: 100,
    w: 100,
    h: 60,
    color: randomWebSafeColor(),
    label: "New Item",
    version: 1
  };
}