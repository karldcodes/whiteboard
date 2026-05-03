export function randomWebSafeColor() {
  // white option removed for clarity
  const colors = [
    "black", "silver", "gray", "maroon", "red", "purple", "fuchsia",
    "green", "lime", "olive", "yellow", "navy", "blue", "teal", "aqua"
  ];

  return colors[Math.floor(Math.random() * colors.length)];
}